# Partner Onboarding Flow

This document describes the target operating process for onboarding a new shipping partner from commercial approval through production go-live. It intentionally separates current API behavior from future production controls.

## Actors

| Actor | Responsibility |
| --- | --- |
| Partner success manager | Owns onboarding checklist, partner communication, sandbox readiness, and production enablement approval. |
| Partner developer | Builds and tests the partner-side API integration. |
| Platform engineer | Issues credentials, validates logs/metrics, and supports go-live. |
| Support lead | Confirms escalation contacts and post-launch monitoring. |

## 1. Intake and approval

1. Partner success records the partner legal name, display name, support contacts, billing or commercial reference, and desired launch date.
2. Partner success assigns an `externalReference` that can be traced to the CRM or marketplace onboarding record.
3. Platform engineering confirms the partner use case is supported by the current endpoint set: partner connection, order creation, shipment event ingestion, and event/order queries.
4. Partner success shares documentation for authentication, idempotency, service levels, status lifecycle, errors, and go-live requirements.

## 2. Sandbox credential issuance

1. Platform engineering creates a sandbox partner connection with `POST /shipping-partners/connect` using the partner `name` and `externalReference`.
2. Platform engineering records the returned partner `id` as the sandbox `partnerId`.
3. Until partner-specific credential storage is implemented, sandbox access uses the configured API key header value managed out-of-band. Do not email secrets in plaintext.
4. Partner success sends the partner:
   - base sandbox URL,
   - API key header name,
   - secret delivery instructions,
   - sandbox `partnerId`,
   - support and escalation contacts.

## 3. Partner API testing

The partner must complete these tests before production credentials are issued.

| Test area | Required evidence |
| --- | --- |
| Authentication | Calls without a key fail with `401`; calls with an invalid key fail with `403`; calls with the issued key succeed. |
| Partner lookup | Partner can retrieve its connection record by `partnerId`. |
| Order creation | Partner can create a valid order and receives `201 Created`. |
| Order idempotency | Partner repeats the same partner/order-number request and receives `200 OK` with the original order. |
| Weight validation | Partner submits zero or negative `totalWeightKg` and receives a documented `400` error. |
| Shipment lifecycle | Partner submits a valid sequence such as `LabelCreated` → `PickedUp` → `InTransit` → `OutForDelivery` → `Delivered`. |
| Invalid partner | Partner submits a request with an unknown `partnerId` and receives a documented `400` error. |
| Query verification | Partner can query orders and shipment events filtered by `partnerId`. |
| Retry behavior | Partner confirms network retries preserve idempotency keys or equivalent duplicate-detection rules. |

## 4. Production readiness review

Partner success schedules a readiness review after sandbox tests pass. The review must confirm:

- partner production contacts and incident escalation path;
- expected daily order and shipment event volume;
- supported service levels and carrier mappings;
- retry policy and timeout behavior;
- launch window and rollback contact;
- monitoring coverage for authentication failures, validation errors, and event ingestion failures.

## 5. Production credential issuance

1. Partner success marks the sandbox certification checklist complete.
2. Platform engineering creates or approves the production partner connection.
3. Production credentials are delivered through the approved secret channel.
4. Partner success confirms the partner has stored credentials in a secret manager, not source code or shared documents.
5. Platform engineering performs a smoke test with the partner during the launch window.

## 6. Go-live and hypercare

1. Enable production credentials at the agreed launch time.
2. Partner submits a low-risk production order and lifecycle event.
3. Platform engineering confirms successful authentication, order creation, event ingestion, and query visibility.
4. Partner success monitors the first business day for elevated `401`, `403`, `400`, duplicate, or invalid transition errors.
5. After hypercare, transfer the partner to standard support and document any follow-up work.

## Exit criteria

A partner is considered live only when production credentials are enabled, smoke tests pass, contacts are confirmed, and the go-live checklist is signed off by partner success and platform engineering.
