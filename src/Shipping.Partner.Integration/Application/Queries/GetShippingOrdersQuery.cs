using Shipping.Partner.Integration.Application.Cqrs;
using Shipping.Partner.Integration.Domain;

namespace Shipping.Partner.Integration.Application.Queries;

public sealed record GetShippingOrdersQuery(Guid? PartnerId) : IQuery<IReadOnlyCollection<ShippingOrder>>;
