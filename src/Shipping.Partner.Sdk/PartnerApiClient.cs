using System.Net.Http.Json;
using System.Text.Json;
using Shipping.Partner.Sdk.Models;

namespace Shipping.Partner.Sdk;

public sealed class PartnerApiClient(HttpClient httpClient) : IPartnerApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<ShippingOrder> CreateOrderAsync(CreateShippingOrderRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        using var response = await httpClient.PostAsJsonAsync("shipping-orders", request, JsonOptions, cancellationToken).ConfigureAwait(false);
        return await ReadAsync<ShippingOrder>(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<ShippingOrder>> GetOrdersAsync(Guid partnerId, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync($"shipping-orders?partnerId={partnerId:D}", cancellationToken).ConfigureAwait(false);
        return await ReadAsync<List<ShippingOrder>>(response, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            string? error = null;
            try
            {
                using var json = JsonDocument.Parse(body);
                if (json.RootElement.TryGetProperty("error", out var property) && property.ValueKind == JsonValueKind.String)
                    error = property.GetString();
            }
            catch (JsonException) { }

            var requestId = response.Headers.TryGetValues("X-Request-Id", out var values) ? values.FirstOrDefault() : null;
            throw new PartnerApiException(response.StatusCode, error ?? $"Partner API returned {(int)response.StatusCode}.", error, requestId);
        }

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken).ConfigureAwait(false)
            ?? throw new JsonException("Partner API returned an empty response.");
    }
}
