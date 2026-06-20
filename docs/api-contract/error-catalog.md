# Error Catalog

This catalog defines stable, machine-readable error codes for the shipping partner API. The current implementation often returns `{ "error": "..." }`; the target public contract should use the structured envelope below.

## Standard error envelope

```json
{
  "code": "INVALID_API_KEY",
  "message": "The supplied shipping partner API key is invalid.",
  "correlationId": "01J...",
  "details": []
}
```

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `code` | string | Yes | Stable machine-readable error code. |
| `message` | string | Yes | Human-readable explanation safe for partner logs. |
| `correlationId` | string | Yes | Request correlation ID for support. |
| `details` | array | No | Optional field-level details with `field`, `code`, and `message`. |

## Stable errors

| Code | HTTP status | Message | Current equivalent |
| --- | --- | --- | --- |
| `MISSING_API_KEY` | `401 Unauthorized` | The required API key header is missing. | `{ "error": "Missing X-Shipping-Partner-Key header." }` |
| `INVALID_API_KEY` | `403 Forbidden` | The supplied shipping partner API key is invalid. | `{ "error": "Invalid shipping partner API key." }` |
| `PARTNER_NOT_FOUND` | `404 Not Found` or `400 Bad Request` | The requested or referenced partner was not found. | Empty `404` for `GET /shipping-partners/{id}`; `{ "error": "Unknown partner." }` for writes. |
| `INVALID_REQUEST` | `400 Bad Request` | The request payload is malformed or cannot be bound. | Framework model-binding error. |
| `VALIDATION_FAILED` | `400 Bad Request` | One or more request fields failed validation. | Several ad hoc `error` strings. |
| `INVALID_TOTAL_WEIGHT` | `400 Bad Request` | `totalWeightKg` must be greater than zero. | `{ "error": "TotalWeightKg must be greater than zero." }` |
| `INVALID_SHIPMENT_STATUS` | `400 Bad Request` | Shipment status is not supported. | `{ "error": "Invalid shipment status." }` |
| `INVALID_STATUS_TRANSITION` | `400 Bad Request` | The requested shipment status transition is not allowed. | Currently an unhandled `InvalidOperationException`. |
| `DUPLICATE_EVENT` | `409 Conflict` or stable idempotent replay status | The shipment event was already received. | Not implemented. |
| `DUPLICATE_ORDER` | `200 OK` replay or `409 Conflict` conflict | The order was already created. | Current duplicate same order returns `200 OK`. |
| `IDEMPOTENCY_KEY_REUSED_WITH_DIFFERENT_PAYLOAD` | `409 Conflict` | The idempotency key was already used with a different payload. | Not implemented. |
| `PARTNER_REFERENCE_CONFLICT` | `409 Conflict` | The external partner reference is already connected with different data. | Not implemented. |
| `INVALID_SERVICE_LEVEL` | `400 Bad Request` | The requested shipping service level is not supported. | Not implemented. |
| `UNAUTHORIZED_PARTNER_SCOPE` | `403 Forbidden` | The credential cannot access the requested partner resource. | Not implemented. |
| `RATE_LIMIT_EXCEEDED` | `429 Too Many Requests` | Too many requests were sent in the current rate-limit window. | Not implemented. |
| `INTERNAL_ERROR` | `500 Internal Server Error` | An unexpected error occurred. | Framework default. |

## Field-level detail shape

```json
{
  "code": "VALIDATION_FAILED",
  "message": "One or more fields are invalid.",
  "correlationId": "01J...",
  "details": [
    {
      "field": "totalWeightKg",
      "code": "GREATER_THAN_ZERO",
      "message": "totalWeightKg must be greater than zero."
    }
  ]
}
```

## Compatibility rules

- Never reuse a retired error code for a different meaning.
- Error `message` text may improve over time, but `code` and status semantics must remain stable within a major API version.
- Partners should branch on `code`, not message text.
- Internal exception messages must not leak stack traces, secrets, or infrastructure names.
