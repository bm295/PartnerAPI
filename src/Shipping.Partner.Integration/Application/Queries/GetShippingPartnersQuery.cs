using Shipping.Partner.Integration.Application.Cqrs;
using Shipping.Partner.Integration.Domain.Entities;

namespace Shipping.Partner.Integration.Application.Queries;

public sealed record GetShippingPartnersQuery : IQuery<IReadOnlyCollection<ShippingPartnerConnection>>;
