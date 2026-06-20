# Shipping Partner Go-Live Checklist

Partner success teams should complete this checklist before production credentials are enabled.

## Partner details

- [ ] Partner legal name and display name recorded.
- [ ] Production `externalReference` recorded.
- [ ] Primary technical contact confirmed.
- [ ] Secondary technical contact confirmed.
- [ ] 24/7 incident or escalation contact confirmed when required by contract.
- [ ] Expected launch date and time window documented.
- [ ] Expected daily order volume and shipment event volume documented.

## Contract and documentation

- [ ] Partner received endpoint contract and examples.
- [ ] Partner received authentication instructions and API key header name.
- [ ] Partner received idempotency rules.
- [ ] Partner received supported service-level catalog.
- [ ] Partner received shipment status lifecycle rules.
- [ ] Partner received stable error catalog and support escalation process.
- [ ] Partner acknowledged deprecation and versioning policy.

## Sandbox certification

- [ ] Partner authenticated successfully in sandbox.
- [ ] Missing API key test produced `401`.
- [ ] Invalid API key test produced `403`.
- [ ] Partner connection lookup succeeded.
- [ ] Valid order creation returned `201 Created`.
- [ ] Duplicate order retry returned the documented idempotent response.
- [ ] Invalid weight test returned the documented validation error.
- [ ] Valid shipment lifecycle sequence was submitted successfully.
- [ ] Invalid status transition behavior was reviewed with partner.
- [ ] Order query by `partnerId` returned expected records.
- [ ] Shipment event query by `partnerId` returned expected records.

## Production readiness

- [ ] Production credentials created or scheduled for creation.
- [ ] Secret delivery channel approved.
- [ ] Partner confirmed credentials will be stored in a secret manager.
- [ ] Partner confirmed retry and timeout settings.
- [ ] Partner confirmed idempotency key generation for retryable writes.
- [ ] Service levels requested by partner are enabled and mapped.
- [ ] Monitoring dashboard reviewed for the partner.
- [ ] Alert routing and support ownership confirmed.
- [ ] Rollback or disablement procedure reviewed.

## Launch execution

- [ ] Production credentials enabled at the agreed time.
- [ ] Partner performed authentication smoke test.
- [ ] Partner created a production test or low-risk order.
- [ ] Partner submitted at least one production shipment event.
- [ ] Platform team confirmed logs show successful authentication and ingestion.
- [ ] Partner success confirmed partner-side success.
- [ ] Hypercare monitoring window started.

## Post-launch

- [ ] First-day error rate reviewed.
- [ ] Authentication failures reviewed.
- [ ] Duplicate order and duplicate event signals reviewed.
- [ ] Partner feedback captured.
- [ ] Any launch issues converted into tracked follow-up work.
- [ ] Partner moved to standard support after hypercare sign-off.
