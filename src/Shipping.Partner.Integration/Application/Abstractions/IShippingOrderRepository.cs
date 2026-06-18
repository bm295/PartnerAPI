using Shipping.Partner.Integration.Domain;
using Shipping.Partner.Integration.Application.Requests;

namespace Shipping.Partner.Integration.Application.Abstractions;

public interface IShippingOrderRepository
{
    IReadOnlyCollection<ShippingOrder> GetAll();
    IReadOnlyCollection<ShippingOrder> GetByPartnerId(Guid partnerId);
    ShippingOrder Create(CreateShippingOrderRequest request);
}
