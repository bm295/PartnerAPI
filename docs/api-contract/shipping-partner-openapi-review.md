# Shipping Partner OpenAPI Review

This review documents the current `Shipping.Partner.Integration` HTTP surface as implemented by the minimal API. It is a contract inventory, not a future-state design. All non-public endpoints require the configured API key header; `/health` and `/swagger` are public.

## Authentication and common behavior

| Item | Current behavior |
| --- | --- |
| API key header | Configured by `ShippingPartnerIntegrationOptions.ApiKeyHeaderName`; default configuration uses `X-Shipping-Partner-Key`. |
| Missing API key | `401 Unauthorized` with `{ "error": "Missing {headerName} header." }`. |
| Invalid API key | `403 Forbidden` with `{ "error": "Invalid shipping partner API key." }`. |
| Success content type | JSON. |
| Error content type | JSON for middleware and command-validation failures. |
| Error shape | Current endpoints return a simple object with one `error` string field; `GET /shipping-partners/{id}` returns an empty `404` body. |
| Persistence | In-memory singleton repositories and event store; data is lost on process restart. |

## Endpoint inventory

### `GET /health`

Public liveness endpoint.

#### Request

No headers, query parameters, or body are required.

#### Responses

| Status | Body | Fields |
| --- | --- | --- |
| `200 OK` | `{ "status": "ok" }` | `status` string, currently always `ok`. |

### `POST /shipping-partners/connect`

Creates a shipping partner connection and generates a partner API key value in the returned record.

#### Request fields

| Field | Type | Required | Validation/normalization | Description |
| --- | --- | --- | --- | --- |
| `name` | string | Yes | Trimmed before storage. No explicit empty check yet. | Partner display name. |
| `externalReference` | string | Yes | Trimmed before storage. No explicit uniqueness rule yet. | External CRM, marketplace, or onboarding reference. |

#### Response fields: `ShippingPartnerConnection`

| Field | Type | Description |
| --- | --- | --- |
| `id` | UUID | Server-generated partner connection identifier. |
| `name` | string | Trimmed partner name. |
| `externalReference` | string | Trimmed external reference. |
| `apiKey` | string | Server-generated key value on the connection record. Current auth middleware still validates a configured key rather than this partner-specific key. |
| `connectedAtUtc` | datetime offset | UTC timestamp when the in-memory connection was created. |

#### Status codes and errors

| Status | Body | When |
| --- | --- | --- |
| `201 Created` | `ShippingPartnerConnection` | Partner connection is created. `Location` is `/shipping-partners/{id}`. |
| `401 Unauthorized` | `{ "error": "Missing X-Shipping-Partner-Key header." }` | Required API key header is absent or blank. |
| `403 Forbidden` | `{ "error": "Invalid shipping partner API key." }` | API key header is present but invalid. |

### `GET /shipping-partners`

Lists all connected shipping partners ordered by partner name.

#### Request

No query parameters or body are supported.

#### Response fields

Returns an array of `ShippingPartnerConnection` objects with the fields documented above.

#### Status codes and errors

| Status | Body | When |
| --- | --- | --- |
| `200 OK` | `ShippingPartnerConnection[]` | Request is authenticated. Empty array when no partners exist. |
| `401 Unauthorized` | `{ "error": "Missing X-Shipping-Partner-Key header." }` | Required API key header is absent or blank. |
| `403 Forbidden` | `{ "error": "Invalid shipping partner API key." }` | API key header is present but invalid. |

### `GET /shipping-partners/{id}`

Fetches one connected shipping partner by ID.

#### Request fields

| Location | Name | Type | Required | Description |
| --- | --- | --- | --- | --- |
| Path | `id` | UUID | Yes | Partner connection ID. The route only matches valid GUID values. |

#### Response fields

Returns a single `ShippingPartnerConnection` when found.

#### Status codes and errors

| Status | Body | When |
| --- | --- | --- |
| `200 OK` | `ShippingPartnerConnection` | Partner exists. |
| `404 Not Found` | Empty body | No partner exists for the ID. |
| `401 Unauthorized` | `{ "error": "Missing X-Shipping-Partner-Key header." }` | Required API key header is absent or blank. |
| `403 Forbidden` | `{ "error": "Invalid shipping partner API key." }` | API key header is present but invalid. |

### `POST /shipping-orders`

Creates a shipping order for an existing partner. The current implementation is idempotent by partner ID plus trimmed, case-insensitive order number.

#### Request fields

| Field | Type | Required | Validation/normalization | Description |
| --- | --- | --- | --- | --- |
| `partnerId` | UUID | Yes | Must reference an existing partner. | Partner that owns the order. |
| `orderNumber` | string | Yes | Trimmed; duplicate detection uses uppercase invariant normalized value. | Partner-supplied order identifier. |
| `destinationName` | string | Yes | Trimmed before storage. | Recipient name. |
| `destinationAddress` | string | Yes | Trimmed before storage. | Destination address as a single current string field. |
| `serviceLevel` | string | Yes | Trimmed before storage. No allow-list validation yet. | Requested shipping service level. |
| `totalWeightKg` | decimal | Yes | Must be greater than zero. | Shipment total weight in kilograms. |

#### Response fields: `ShippingOrder`

| Field | Type | Description |
| --- | --- | --- |
| `id` | UUID | Server-generated order identifier. |
| `partnerId` | UUID | Partner that owns the order. |
| `orderNumber` | string | Trimmed order number. |
| `destinationName` | string | Trimmed destination name. |
| `destinationAddress` | string | Trimmed destination address. |
| `serviceLevel` | string | Trimmed requested service level. |
| `totalWeightKg` | decimal | Total package weight in kilograms. |
| `createdAtUtc` | datetime offset | UTC timestamp when the order was first stored. |

#### Status codes and errors

| Status | Body | When |
| --- | --- | --- |
| `201 Created` | `ShippingOrder` | A new order is created. `Location` is `/shipping-orders/{id}`. |
| `200 OK` | `ShippingOrder` | A duplicate partner/order-number request returns the existing order. |
| `400 Bad Request` | `{ "error": "Unknown partner." }` | `partnerId` is not connected. |
| `400 Bad Request` | `{ "error": "TotalWeightKg must be greater than zero." }` | `totalWeightKg` is zero or negative. |
| `401 Unauthorized` | `{ "error": "Missing X-Shipping-Partner-Key header." }` | Required API key header is absent or blank. |
| `403 Forbidden` | `{ "error": "Invalid shipping partner API key." }` | API key header is present but invalid. |

### `GET /shipping-orders`

Lists shipping orders ordered by creation time.

#### Request fields

| Location | Name | Type | Required | Description |
| --- | --- | --- | --- | --- |
| Query | `partnerId` | UUID | No | When present, returns only orders for that partner. |

#### Response fields

Returns an array of `ShippingOrder` objects with the fields documented above.

#### Status codes and errors

| Status | Body | When |
| --- | --- | --- |
| `200 OK` | `ShippingOrder[]` | Request is authenticated. Empty array when no matching orders exist. |
| `401 Unauthorized` | `{ "error": "Missing X-Shipping-Partner-Key header." }` | Required API key header is absent or blank. |
| `403 Forbidden` | `{ "error": "Invalid shipping partner API key." }` | API key header is present but invalid. |

### `POST /shipments/events`

Accepts a shipment lifecycle event for a known partner and stores it in append order.

#### Request fields

| Field | Type | Required | Validation/normalization | Description |
| --- | --- | --- | --- | --- |
| `partnerId` | UUID | Yes | Must reference an existing partner. | Partner that owns the shipment event. |
| `trackingNumber` | string | Yes | Trimmed before storage. | Carrier or partner tracking number. |
| `status` | enum/string | Yes | Parsed as `ShipmentStatus`; endpoint currently passes `Status.ToString()` so valid JSON enum names are expected unless serializer options allow numeric values. | Shipment lifecycle status. |
| `location` | string/null | No | Blank values are stored as `null`; non-blank values are trimmed. | Event location such as facility or city/state. |
| `occurredAtUtc` | datetime offset | Yes | No future/skew validation yet. | When the shipment event occurred. |

Valid `status` values are `LabelCreated`, `PickedUp`, `InTransit`, `OutForDelivery`, `Delivered`, `Exception`, `Returned`, and `Cancelled`.

#### Response fields: `ShipmentEventRecord`

| Field | Type | Description |
| --- | --- | --- |
| `id` | UUID | Server-generated shipment event identifier. |
| `partnerId` | UUID | Partner that owns the event. |
| `trackingNumber` | string | Trimmed tracking number. |
| `status` | enum/string | Stored shipment status. |
| `location` | string/null | Normalized event location. |
| `occurredAtUtc` | datetime offset | Partner-supplied event occurrence time. |
| `receivedAtUtc` | datetime offset | Server UTC timestamp when the event was accepted. |

#### Status codes and errors

| Status | Body | When |
| --- | --- | --- |
| `202 Accepted` | `ShipmentEventRecord` | Event is accepted. `Location` is `/shipments/events/{id}`. |
| `400 Bad Request` | `{ "error": "Unknown partner." }` | `partnerId` is not connected. |
| `400 Bad Request` | `{ "error": "Invalid shipment status." }` | Command status parsing fails. |
| `500 Internal Server Error` | Framework error response | Current event store throws `InvalidOperationException` for invalid lifecycle transitions; it is not converted to a stable `400` response yet. |
| `401 Unauthorized` | `{ "error": "Missing X-Shipping-Partner-Key header." }` | Required API key header is absent or blank. |
| `403 Forbidden` | `{ "error": "Invalid shipping partner API key." }` | API key header is present but invalid. |

### `GET /shipments/events`

Lists shipment events in append order.

#### Request fields

| Location | Name | Type | Required | Description |
| --- | --- | --- | --- | --- |
| Query | `partnerId` | UUID | No | When present, returns only events for that partner. |

#### Response fields

Returns an array of `ShipmentEventRecord` objects with the fields documented above.

#### Status codes and errors

| Status | Body | When |
| --- | --- | --- |
| `200 OK` | `ShipmentEventRecord[]` | Request is authenticated. Empty array when no matching events exist. |
| `401 Unauthorized` | `{ "error": "Missing X-Shipping-Partner-Key header." }` | Required API key header is absent or blank. |
| `403 Forbidden` | `{ "error": "Invalid shipping partner API key." }` | API key header is present but invalid. |

## Current contract gaps to resolve before public launch

- Return response DTOs instead of domain entities so fields like `apiKey` can be controlled.
- Adopt a stable error object with machine-readable `code`, human-readable `message`, and correlation metadata.
- Convert invalid status transitions into stable client errors instead of unhandled exceptions.
- Add OpenAPI examples and security metadata for the API key header.
- Add pagination and filtering for list endpoints before high-volume partner use.
