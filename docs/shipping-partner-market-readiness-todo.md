# Shipping Partner API Market Readiness To-Do List

This checklist breaks market readiness into small, implementation-ready tasks. Each task is written so an engineer can create a focused branch, class, endpoint, migration, or test without first decomposing the work.

## 1. Product definition and partner contract

- [x] Create `docs/api-contract/shipping-partner-openapi-review.md` and document every current endpoint, request field, response field, status code, and error shape.
- [x] Create `docs/api-contract/partner-onboarding-flow.md` describing how a new shipping partner receives credentials, tests the API, and goes live.
- [x] Create `docs/api-contract/idempotency-rules.md` defining idempotency keys for partner connection, order creation, shipment event ingestion, and webhook delivery.
- [x] Create `docs/api-contract/versioning-policy.md` defining URL or header versioning, deprecation windows, and breaking-change communication rules.
- [x] Create `docs/api-contract/service-levels.md` listing supported shipping service levels, display names, carrier mappings, and validation rules.
- [x] Create `docs/api-contract/error-catalog.md` with stable error codes such as `PARTNER_NOT_FOUND`, `INVALID_API_KEY`, `DUPLICATE_EVENT`, and `INVALID_STATUS_TRANSITION`.
- [x] Create `docs/api-contract/status-lifecycle.md` explaining valid shipment status transitions and examples for each transition.
- [x] Create `docs/api-contract/go-live-checklist.md` that partner success teams can use before enabling production credentials.

## 2. Authentication, authorization, and credential lifecycle

- [x] Create class `PartnerCredential` in `Domain/Entities` with `Id`, `PartnerId`, `HashedSecret`, `CreatedAtUtc`, `ExpiresAtUtc`, `RevokedAtUtc`, and `LastUsedAtUtc` fields.
- [x] Create interface `IPartnerCredentialRepository` in `Application/Abstractions` for creating, rotating, revoking, and looking up credentials.
- [x] Create class `PartnerCredentialOptions` in `Application/Configuration` with secret length, expiration period, allowed clock skew, and header name settings.
- [x] Replace `ConfigurationApiKeyValidator` with `PartnerCredentialApiKeyValidator` that validates hashed partner-specific credentials instead of a fixed `change-me` value.
- [ ] Create class `ApiKeyHashingService` with methods `HashSecret(string secret)` and `VerifySecret(string secret, string hash)`.
- [ ] Create endpoint `POST /shipping-partners/{partnerId}/credentials` for issuing a new credential to an existing partner.
- [ ] Create endpoint `POST /shipping-partners/{partnerId}/credentials/{credentialId}/rotate` for replacing an active credential.
- [ ] Create endpoint `DELETE /shipping-partners/{partnerId}/credentials/{credentialId}` for revoking a credential.
- [ ] Add authorization checks that ensure partners can only create orders and shipment events for their own `PartnerId`.
- [ ] Add audit fields to authentication failures, credential creation, credential rotation, and credential revocation.
- [ ] Add integration tests for missing credential, invalid credential, expired credential, revoked credential, and valid credential requests.

## 3. Persistent storage and data access

- [ ] Add `src/Shipping.Partner.Integration.Infrastructure.Persistence` namespace or folder for production persistence components.
- [ ] Create `ShippingPartnerDbContext` with `DbSet<ShippingPartnerConnection>`, `DbSet<ShippingOrder>`, `DbSet<ShipmentEventRecord>`, and `DbSet<PartnerCredential>`.
- [ ] Create entity configuration class `ShippingPartnerConnectionConfiguration` with indexes on partner name and external reference.
- [ ] Create entity configuration class `ShippingOrderConfiguration` with a unique index on `PartnerId` plus normalized `OrderNumber`.
- [ ] Create entity configuration class `ShipmentEventRecordConfiguration` with indexes on `PartnerId`, `TrackingNumber`, and `OccurredAtUtc`.
- [ ] Create entity configuration class `PartnerCredentialConfiguration` with indexes on `PartnerId`, credential key identifier, and active credential status.
- [ ] Create repository class `EfShippingPartnerRepository` implementing `IShippingPartnerRepository`.
- [ ] Create repository class `EfShippingOrderRepository` implementing `IShippingOrderRepository`.
- [ ] Create store class `EfShipmentEventStore` implementing `IShipmentEventStore`.
- [ ] Create repository class `EfPartnerCredentialRepository` implementing `IPartnerCredentialRepository`.
- [ ] Add database migrations for the initial shipping partner schema.
- [ ] Add a startup health check that verifies database connectivity.
- [ ] Keep in-memory repositories available only for local development and tests through environment-specific dependency injection.
- [ ] Add transaction handling around order creation and shipment event recording where consistency matters.

## 4. Validation and business rules

- [ ] Create validator class `ConnectShippingPartnerRequestValidator` to require partner name and validate external reference length.
- [ ] Create validator class `CreateShippingOrderRequestValidator` to validate partner ID, order number, destination fields, service level, and positive weight.
- [ ] Create validator class `ShipmentEventRequestValidator` to validate partner ID, tracking number, status, location, and occurrence timestamp.
- [ ] Create reusable class `NormalizedString` or utility `StringNormalizer` for trimming and case-normalizing order numbers and external references.
- [ ] Create class `ShipmentStatusTransitionValidator` that uses `ShipmentStatusLifecycle` to reject invalid status transitions.
- [ ] Create class `ShippingOrderDuplicateDetector` to centralize idempotent duplicate order detection.
- [ ] Create class `ShipmentEventDuplicateDetector` to prevent duplicate event ingestion by partner, tracking number, status, location, and occurrence timestamp.
- [ ] Add validation for future `OccurredAtUtc` timestamps beyond allowed clock skew.
- [ ] Add validation for unrealistic package weights based on configured maximum weight.
- [ ] Add structured validation error responses using a common `ProblemDetails` format.
- [ ] Add unit tests for every validator with valid, boundary, and invalid examples.

## 5. API design, responses, and developer experience

- [ ] Create response DTO `ShippingPartnerResponse` instead of returning domain entities directly.
- [ ] Create response DTO `ShippingOrderResponse` instead of returning domain entities directly.
- [ ] Create response DTO `ShipmentEventResponse` instead of returning domain entities directly.
- [ ] Create response DTO `CredentialCreatedResponse` that returns the raw credential only once during creation.
- [ ] Create mapper class `ShippingPartnerResponseMapper` for partner response conversion.
- [ ] Create mapper class `ShippingOrderResponseMapper` for order response conversion.
- [ ] Create mapper class `ShipmentEventResponseMapper` for shipment event response conversion.
- [ ] Add pagination parameters `pageSize` and `continuationToken` to partner, order, and event list endpoints.
- [ ] Create class `PagedResponse<T>` with `Items`, `ContinuationToken`, and `HasMore` properties.
- [ ] Add filtering by tracking number, status, and occurrence date range to `GET /shipments/events`.
- [ ] Add filtering by order number, service level, and creation date range to `GET /shipping-orders`.
- [ ] Add `GET /shipping-orders/{id}` for fetching a single order.
- [ ] Add `GET /shipments/events/{id}` for fetching a single shipment event.
- [ ] Add OpenAPI examples for each request and response type.
- [ ] Add OpenAPI security definitions for the partner API key header.
- [ ] Add a public `GET /metadata` endpoint with supported statuses, service levels, API version, and documentation links.

## 6. Reliability, concurrency, and idempotency

- [ ] Create class `IdempotencyKey` with normalized key, partner ID, request hash, first response status, and expiration timestamp.
- [ ] Create interface `IIdempotencyStore` for saving and replaying idempotent responses.
- [ ] Create implementation `EfIdempotencyStore` backed by the production database.
- [ ] Add middleware `IdempotencyMiddleware` for `POST` endpoints that accept the `Idempotency-Key` header.
- [ ] Add request hashing so repeated idempotency keys with different payloads return a conflict.
- [ ] Add optimistic concurrency tokens to order and shipment event persistence models.
- [ ] Add retry policies for transient database errors.
- [ ] Add timeout policies for downstream calls once external carrier APIs are introduced.
- [ ] Add integration tests for concurrent duplicate order creation.
- [ ] Add integration tests for repeated shipment event submissions.

## 7. Observability and operations

- [ ] Add structured logging scopes with `PartnerId`, `OrderId`, `TrackingNumber`, and correlation ID.
- [ ] Create middleware `CorrelationIdMiddleware` that reads or creates an `X-Correlation-Id` header.
- [ ] Add OpenTelemetry tracing for middleware, handlers, repositories, and database calls.
- [ ] Add metrics for request count, request duration, authentication failures, order creation, event ingestion, and validation failures.
- [ ] Add health checks for liveness, readiness, database connectivity, and credential store access.
- [ ] Add endpoint `GET /health/ready` for readiness probes.
- [ ] Add endpoint `GET /health/live` for liveness probes.
- [ ] Create dashboard documentation in `docs/operations/dashboards.md`.
- [ ] Create alert documentation in `docs/operations/alerts.md` for high error rate, high latency, authentication spikes, and database failures.
- [ ] Add runbook `docs/operations/credential-rotation-runbook.md`.
- [ ] Add runbook `docs/operations/incident-response-runbook.md`.

## 8. Security and compliance

- [ ] Ensure all secrets are loaded from a secret manager or environment variables, never from committed configuration.
- [ ] Add configuration validation that fails startup when production uses default secrets.
- [ ] Add rate limiting per partner credential and per source IP.
- [ ] Add request body size limits for all write endpoints.
- [ ] Add security headers appropriate for API responses.
- [ ] Add audit log entity `PartnerAuditLogEntry` with actor, action, resource ID, timestamp, IP address, and correlation ID.
- [ ] Create repository interface `IPartnerAuditLogRepository`.
- [ ] Create `PartnerAuditLogger` service and call it from credential, partner, order, and event workflows.
- [ ] Add data retention policy documentation for partner, order, event, credential, idempotency, and audit data.
- [ ] Add threat model document `docs/security/threat-model.md`.
- [ ] Add dependency vulnerability scanning to CI.
- [ ] Add static analysis and secret scanning to CI.

## 9. Testing and quality gates

- [ ] Add unit tests for each command handler success path and failure path.
- [ ] Add unit tests for each query handler filter and ordering rule.
- [ ] Add unit tests for every repository implementation.
- [ ] Add integration tests for all HTTP endpoints and status codes.
- [ ] Add contract tests that verify OpenAPI examples match real responses.
- [ ] Add authorization tests proving partners cannot access another partner's orders or shipment events.
- [ ] Add migration tests that apply the database schema from an empty database.
- [ ] Add performance tests for event ingestion throughput.
- [ ] Add load tests for list endpoints with realistic pagination sizes.
- [ ] Add smoke tests for local Docker Compose startup.
- [ ] Add CI steps for restore, build, unit tests, integration tests, formatting, vulnerability scan, and package publish validation.

## 10. Deployment and release readiness

- [ ] Create a production Dockerfile for the shipping partner integration API.
- [ ] Create Docker Compose configuration for API, database, and observability dependencies.
- [ ] Create Kubernetes deployment manifests or Helm chart values for staging and production.
- [ ] Add environment-specific configuration files for local, staging, and production.
- [ ] Add deployment documentation `docs/deployment/deploy-shipping-partner-api.md`.
- [ ] Add rollback documentation `docs/deployment/rollback-shipping-partner-api.md`.
- [ ] Add release notes template `docs/release/release-notes-template.md`.
- [ ] Add semantic versioning policy for the API package and OpenAPI document.
- [ ] Add database backup and restore runbook.
- [ ] Add production readiness review checklist covering security, reliability, observability, support, and partner documentation.

## 11. Partner support and market launch

- [ ] Create public quickstart guide `docs/partner-guide/quickstart.md` with authentication, first order, first event, and query examples.
- [ ] Create Postman collection for partner testing.
- [ ] Create sample curl scripts for each endpoint.
- [ ] Create sample C# client class `ShippingPartnerApiClient` for partner developers.
- [ ] Create sample JavaScript client class `ShippingPartnerApiClient` for partner developers.
- [ ] Create sandbox onboarding checklist for internal support.
- [ ] Create production onboarding checklist for internal support.
- [ ] Create support escalation matrix for partner incidents.
- [ ] Create known limitations document for launch candidates.
- [ ] Create go-to-market FAQ covering authentication, rate limits, retries, idempotency, status lifecycle, and support contacts.

## Suggested first milestone

1. Replace fixed API key validation with partner-specific credential storage.
2. Add persistent database storage for partners, orders, events, credentials, and idempotency keys.
3. Add request validators and stable problem-detail error responses.
4. Add pagination and response DTOs for all list endpoints.
5. Add integration tests for authentication, order creation, event ingestion, and cross-partner isolation.
6. Add OpenAPI security definitions, examples, and a partner quickstart guide.
7. Add Docker Compose and CI checks so the API can be demonstrated reliably to early partners.
