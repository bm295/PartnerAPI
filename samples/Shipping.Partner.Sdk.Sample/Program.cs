using Microsoft.Extensions.DependencyInjection;
using Shipping.Partner.Sdk;

var baseUrl = Environment.GetEnvironmentVariable("PARTNER_API_URL")
    ?? throw new InvalidOperationException("Set PARTNER_API_URL.");
var apiKey = Environment.GetEnvironmentVariable("PARTNER_API_KEY")
    ?? throw new InvalidOperationException("Set PARTNER_API_KEY.");
var partnerIdText = Environment.GetEnvironmentVariable("PARTNER_ID")
    ?? throw new InvalidOperationException("Set PARTNER_ID.");
if (!Guid.TryParse(partnerIdText, out var partnerId))
    throw new InvalidOperationException("PARTNER_ID must be a GUID.");

var services = new ServiceCollection();
services.AddPartnerApi(options =>
{
    options.BaseUrl = new Uri(baseUrl);
    options.ApiKey = apiKey;
});

using var provider = services.BuildServiceProvider();
var client = provider.GetRequiredService<IPartnerApiClient>();
var orders = await client.GetOrdersAsync(partnerId).ConfigureAwait(false);
foreach (var order in orders)
    Console.WriteLine($"{order.OrderNumber}: {order.Id}");
