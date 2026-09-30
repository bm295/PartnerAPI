# Shipping Service Levels

The current API accepts `serviceLevel` as a free-form string and stores the trimmed value. This document defines the launch-ready service-level catalog that should be enforced before public production use.

## Supported service levels

| Canonical code | Display name | Typical carrier mappings | Delivery promise | Notes |
| --- | --- | --- | --- | --- |
| `STANDARD` | Standard Ground | UPS Ground, FedEx Ground, USPS Ground Advantage, DHL eCommerce Ground | Economical ground delivery; exact transit time depends on lane. | Default for non-urgent domestic parcels. |
| `EXPEDITED` | Expedited | UPS 3 Day Select, FedEx Express Saver, USPS Priority Mail, DHL Parcel Expedited | Faster than ground, generally multi-day. | Use when partner requests faster delivery without overnight commitment. |
| `TWO_DAY` | Two-Day | UPS 2nd Day Air, FedEx 2Day, USPS Priority Mail 2-Day where available | Two business days where supported. | Validate destination and carrier capability. |
| `OVERNIGHT` | Overnight | UPS Next Day Air, FedEx Standard Overnight, USPS Priority Mail Express | Next business day where supported. | Cutoff times and exceptions must be communicated. |
| `INTERNATIONAL_STANDARD` | International Standard | DHL Parcel International Standard, UPS Worldwide Expedited, FedEx International Economy | Cross-border economical service. | Requires customs data in future API versions. |
| `INTERNATIONAL_EXPRESS` | International Express | DHL Express Worldwide, UPS Worldwide Express, FedEx International Priority | Cross-border express service. | Requires customs data in future API versions. |
| `LOCAL_COURIER` | Local Courier | Regional courier, same-city carrier, crowd-sourced courier | Same-day or scheduled local delivery. | Only enabled for configured origin/destination markets. |

## Validation rules

1. Normalize submitted `serviceLevel` by trimming whitespace and uppercasing with invariant culture.
2. Accept only canonical codes listed above for public production traffic.
3. Reject blank, unknown, or deprecated values with `400 INVALID_SERVICE_LEVEL`.
4. Store the canonical code, not the partner display label.
5. Return the canonical code and display name in future response DTOs or metadata endpoints.
6. Validate carrier mapping availability by partner, origin country, destination country, package weight, and requested pickup date once carrier integrations are introduced.
7. Do not silently downgrade a service level. If a carrier cannot support the requested service, return a validation error or require partner confirmation.
8. Preserve backward-compatible aliases during migrations, for example `GROUND` → `STANDARD`, but document retirement dates.

## Current implementation notes

- `POST /shipping-orders` currently accepts any non-null `serviceLevel` value that model binding provides.
- The in-memory order repository trims `serviceLevel` before storage.
- There is no metadata endpoint yet for partners to discover service levels.
