using Shipping.Partner.Integration.Application.Cqrs;
using Shipping.Partner.Integration.Domain.Entities;

namespace Shipping.Partner.Integration.Application.Queries;

public sealed record GetShippingOrdersQuery(Guid? PartnerId) : IQuery<IReadOnlyCollection<ShippingOrder>>;
