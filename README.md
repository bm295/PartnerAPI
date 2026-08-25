# PartnerAPI

A .NET 10 solution containing a partner CRUD API and a shipping-partner integration API.

## Projects
- `src/Partner.Api`: ASP.NET Core minimal API with in-memory repository.
- `tests/Partner.Api.Tests`: xUnit unit tests for repository behavior.
- `src/Shipping.Partner.Integration`: layered ASP.NET Core minimal API for partner connections, shipping orders, shipment events, and partner credentials.
- `tests/Shipping.Partner.Integration.Tests`: xUnit behavioral and architecture-boundary tests for the shipping integration.

See [`docs/component-relationships.md`](docs/component-relationships.md) for the original partner API and
[`docs/shipping-partner-integration-flow.md`](docs/shipping-partner-integration-flow.md) for the shipping integration architecture.

## Run locally
```bash
dotnet restore PartnerAPI.sln
dotnet run --project src/Partner.Api/Partner.Api.csproj
```

To run the shipping integration instead:

```bash
dotnet run --project src/Shipping.Partner.Integration/Shipping.Partner.Integration.csproj
```

Swagger UI is available at `http://localhost:5000/swagger` by default.

## Keycloak JWT integration
- Protected endpoints: all `/partners` routes now require a valid Bearer token.
- Public endpoint: `/health`.
- Configure Keycloak in `src/Partner.Api/appsettings.json` under `Keycloak`:
  - `Authority`: realm issuer URL, e.g. `http://localhost:8080/realms/partner-realm`
  - `Realm`: realm name
  - `ClientId`: API client/audience (e.g. `partner-api`)
  - `RequireHttpsMetadata`: set `false` for local HTTP development

Example token request for client credentials:
```bash
curl -X POST \
  "http://localhost:8080/realms/partner-realm/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials&client_id=partner-api&client_secret=<secret>"
```

## Shipping partner market readiness

A detailed implementation checklist for preparing the shipping partner integration API for market launch is available in [`docs/shipping-partner-market-readiness-todo.md`](docs/shipping-partner-market-readiness-todo.md).

## Test
```bash
dotnet test PartnerAPI.sln
```
