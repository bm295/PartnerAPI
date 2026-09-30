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

## Shipping Partner SDK

`src/Shipping.Partner.Sdk` is a reusable NuGet library for the shipping integration API. It supports creating orders and listing orders for one partner. The SDK sends the `X-Shipping-Partner-Key` header and throws `PartnerApiException` for unsuccessful HTTP responses. An order with an existing order number may be returned by the API instead of created again. The SDK does not automatically retry POST requests.

Pack it with `dotnet pack src/Shipping.Partner.Sdk/Shipping.Partner.Sdk.csproj -c Release`. A different repo can install the resulting `.nupkg` from a local or private NuGet feed, then configure it like this:

```csharp
using Shipping.Partner.Sdk;
using Shipping.Partner.Sdk.Models;

services.AddPartnerApi(options =>
{
    options.BaseUrl = new Uri(configuration["PartnerApi:BaseUrl"]!);
    options.ApiKey = configuration["PartnerApi:ApiKey"]!;
});

// Inject IPartnerApiClient into a service:
var order = await client.CreateOrderAsync(new CreateShippingOrderRequest(
    partnerId, "ORDER-001", "Customer", "123 Main St", "standard", 1.5m));
var orders = await client.GetOrdersAsync(partnerId);
```

The API key should come from the consuming app's secret configuration. Set `PARTNER_API_URL`, `PARTNER_API_KEY`, and `PARTNER_ID` to run the read-only example in `samples/Shipping.Partner.Sdk.Sample`.
