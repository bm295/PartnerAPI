using Shipping.Partner.Sdk.Models;

namespace Shipping.Partner.Sdk;

public interface IPartnerApiClient
{
    Task<ShippingOrder> CreateOrderAsync(CreateShippingOrderRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ShippingOrder>> GetOrdersAsync(Guid partnerId, CancellationToken cancellationToken = default);
}
