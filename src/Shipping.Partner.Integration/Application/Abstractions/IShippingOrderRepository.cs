using Shipping.Partner.Integration.Domain.Entities;
using Shipping.Partner.Integration.Application.Requests;
using Shipping.Partner.Integration.Application.Results;

namespace Shipping.Partner.Integration.Application.Abstractions;

public interface IShippingOrderRepository
{
    IReadOnlyCollection<ShippingOrder> GetAll();
    IReadOnlyCollection<ShippingOrder> GetByPartnerId(Guid partnerId);
    ShippingOrderCreationResult Create(CreateShippingOrderRequest request);
}
