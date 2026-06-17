using Shipping.Partner.Integration.Domain;

namespace Shipping.Partner.Integration.Application;

public interface IShippingOrderRepository
{
    IReadOnlyCollection<ShippingOrder> GetAll();
    IReadOnlyCollection<ShippingOrder> GetByPartnerId(Guid partnerId);
    ShippingOrder Create(CreateShippingOrderRequest request);
}
