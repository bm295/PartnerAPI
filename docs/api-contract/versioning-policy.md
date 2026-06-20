# API Versioning Policy

This policy defines how the shipping partner API should evolve without surprising integrated partners.

## Versioning mechanism

| Decision | Policy |
| --- | --- |
| Primary version location | URL path versioning: `/v1/...` for public partner APIs. |
| Current unversioned routes | Current routes remain internal/pre-version launch routes until a public `/v1` contract is introduced. |
| Optional header | `X-Partner-Api-Version` may be accepted for diagnostics but must not override the URL major version. |
| OpenAPI documents | Publish one OpenAPI document per major version, for example `/swagger/v1/swagger.json`. |
| SDKs and examples | Label examples by major version and avoid mixing unversioned and versioned routes. |

## Change classification

### Non-breaking changes

These changes may be made within the same major version:

- adding optional request fields with safe defaults;
- adding response fields;
- adding new endpoints;
- adding enum values only when clients are documented to handle unknown values;
- relaxing validation;
- improving error messages while keeping stable error codes;
- adding pagination metadata while preserving existing list data shape through a planned transition.

### Breaking changes

These changes require a new major version or an explicit migration agreement:

- removing or renaming endpoints, fields, status codes, or error codes;
- changing field types or requiredness;
- changing authentication requirements in a way that breaks existing clients;
- tightening validation for previously accepted requests;
- changing idempotency replay semantics;
- changing shipment status lifecycle rules in a way that rejects previously valid transitions;
- changing service-level codes or carrier mappings without backward-compatible aliases.

## Deprecation windows

| Change type | Minimum notice |
| --- | --- |
| Documentation-only clarification | No notice required, but include in release notes. |
| Non-breaking additive change | 7 calendar days notice for high-volume partners when behavior is visible. |
| Deprecated field or endpoint | 180 calendar days before removal. |
| Major version retirement | 12 months from new major version general availability. |
| Security emergency | As soon as practical; may be shorter than standard windows with executive approval and direct partner communication. |

## Communication rules

1. Publish release notes for every externally visible change.
2. Email named technical contacts for deprecations, major-version releases, and behavior changes.
3. Include effective date, affected endpoints, sample before/after payloads, migration steps, and support contact.
4. Provide a sandbox validation window before production enforcement.
5. Track partner acknowledgements for breaking changes.
6. Do not remove deprecated behavior until monitoring shows no production traffic from active partners or an exception is approved.

## Version lifecycle

| Stage | Description |
| --- | --- |
| Draft | Contract is under design and can change without notice. |
| Preview | Available in sandbox; breaking changes allowed with direct pilot communication. |
| GA | Available for production; follows this versioning policy. |
| Deprecated | Still supported, but partners must migrate by the announced date. |
| Retired | No longer available except under a temporary incident exception. |
