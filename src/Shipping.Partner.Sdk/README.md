# Shipping.Partner.Sdk

.NET 10 client for the Shipping Partner Integration API. Register with `services.AddPartnerApi(options => { options.BaseUrl = new Uri("https://partner.example/"); options.ApiKey = "..."; });`, then inject `IPartnerApiClient` to call `CreateOrderAsync` or `GetOrdersAsync`. Store the API key in the consuming application's secret configuration.
