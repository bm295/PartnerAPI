# Idempotency Rules

Idempotency protects partners and the platform from duplicate side effects during retries, timeouts, and network failures. This document defines the target contract and records the current implementation where behavior already exists.

## Common header

| Item | Rule |
| --- | --- |
| Header name | `Idempotency-Key` for future public `POST` endpoints. |
| Key scope | Scoped by authenticated partner and endpoint family. The same raw key can be reused on different endpoint families without collision. |
| Key length | 8-128 printable characters. Recommended: UUID v4 or ULID. |
| Retention | Minimum 24 hours for write operations; 7 days recommended for order and event APIs. |
| Replay response | Same status code and response body as the first successful request when the request hash matches. |
| Payload mismatch | `409 Conflict` with error code `IDEMPOTENCY_KEY_REUSED_WITH_DIFFERENT_PAYLOAD`. |
| Missing key | Allowed only where an endpoint has a natural business key; otherwise reject with `400 MISSING_IDEMPOTENCY_KEY` after idempotency middleware is introduced. |

## Partner connection

| Aspect | Rule |
| --- | --- |
| Endpoint | `POST /shipping-partners/connect`. |
| Target key | `partner-connect:{normalizedExternalReference}` or explicit `Idempotency-Key`. |
| Normalization | Trim `externalReference`; compare case-insensitively unless the external source requires case-sensitive references. |
| First request | Creates a partner connection and credential record. |
| Duplicate same payload | Returns the original partner connection. |
| Duplicate different payload | Returns `409 PARTNER_REFERENCE_CONFLICT`. |
| Current implementation | Not yet idempotent; every request creates a new in-memory partner connection. |

## Order creation

| Aspect | Rule |
| --- | --- |
| Endpoint | `POST /shipping-orders`. |
| Current natural key | `partnerId` + trimmed, uppercase-invariant `orderNumber`. |
| Target key | Prefer explicit `Idempotency-Key`; also preserve the natural unique key above. |
| First request | Creates an order and returns `201 Created`. |
| Duplicate same natural key | Current implementation returns `200 OK` with the original order. |
| Duplicate with different payload | Future implementation should return `409 ORDER_NUMBER_CONFLICT` when immutable fields differ. |
| Retry-safe fields | `partnerId`, `orderNumber`, `destinationName`, `destinationAddress`, `serviceLevel`, and `totalWeightKg` must hash identically for an exact replay. |

## Shipment event ingestion

| Aspect | Rule |
| --- | --- |
| Endpoint | `POST /shipments/events`. |
| Target key | `partnerId` + normalized `trackingNumber` + `status` + normalized `location` + `occurredAtUtc`, plus optional explicit `Idempotency-Key`. |
| First request | Stores a shipment event and returns `202 Accepted`. |
| Duplicate same event | Return `202 Accepted` with the original event or `200 OK` with a duplicate marker; choose one before public launch and keep it stable. |
| Duplicate different event under same key | Return `409 IDEMPOTENCY_KEY_REUSED_WITH_DIFFERENT_PAYLOAD`. |
| Lifecycle conflict | Return `400 INVALID_STATUS_TRANSITION` when the event is not allowed after the latest known status. |
| Current implementation | No duplicate detector exists; valid repeated event submissions can create multiple event records unless rejected by lifecycle rules. |

## Webhook delivery

Webhook delivery is a future outbound capability. Delivery idempotency should be defined before enabling partner callbacks.

| Aspect | Rule |
| --- | --- |
| Delivery ID | Generate a stable `webhookDeliveryId` for every outbound delivery attempt group. |
| Event ID | Include the source domain event `id` so partners can deduplicate. |
| Header | Send `X-Webhook-Idempotency-Key: {webhookDeliveryId}`. |
| Retry behavior | Retries of the same event to the same subscriber reuse the same idempotency key and increment an attempt count. |
| Success definition | Any partner `2xx` response completes the delivery. |
| Duplicate partner response | Partners should treat repeated deliveries with the same delivery ID as safe duplicates. |
| Retention | Store delivery history for at least 30 days for support investigations. |

## Error responses

Idempotency errors should use the stable error envelope defined in the error catalog:

```json
{
  "code": "IDEMPOTENCY_KEY_REUSED_WITH_DIFFERENT_PAYLOAD",
  "message": "The Idempotency-Key was already used with a different request payload.",
  "correlationId": "..."
}
```

The current implementation still returns `{ "error": "..." }` for command failures.
