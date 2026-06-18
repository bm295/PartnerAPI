using Shipping.Partner.Integration.Application.Abstractions;
using Shipping.Partner.Integration.Application.Cqrs;
using Shipping.Partner.Integration.Application.Queries;
using Shipping.Partner.Integration.Domain;

namespace Shipping.Partner.Integration.Application.Handlers;

public sealed class GetShippingOrdersQueryHandler(
    IShippingOrderRepository orderRepository) : IQueryHandler<GetShippingOrdersQuery, IReadOnlyCollection<ShippingOrder>>
{
    public IReadOnlyCollection<ShippingOrder> Handle(GetShippingOrdersQuery query) =>
        query.PartnerId is null ? orderRepository.GetAll() : orderRepository.GetByPartnerId(query.PartnerId.Value);
}
