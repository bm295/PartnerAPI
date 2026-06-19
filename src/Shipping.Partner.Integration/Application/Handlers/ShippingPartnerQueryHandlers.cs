using Shipping.Partner.Integration.Application.Abstractions;
using Shipping.Partner.Integration.Application.Cqrs;
using Shipping.Partner.Integration.Application.Queries;
using Shipping.Partner.Integration.Domain.Entities;

namespace Shipping.Partner.Integration.Application.Handlers;

public sealed class GetShippingPartnersQueryHandler(
    IShippingPartnerRepository repository) : IQueryHandler<GetShippingPartnersQuery, IReadOnlyCollection<ShippingPartnerConnection>>
{
    public IReadOnlyCollection<ShippingPartnerConnection> Handle(GetShippingPartnersQuery query) => repository.GetAll();
}

public sealed class GetShippingPartnerByIdQueryHandler(
    IShippingPartnerRepository repository) : IQueryHandler<GetShippingPartnerByIdQuery, ShippingPartnerConnection?>
{
    public ShippingPartnerConnection? Handle(GetShippingPartnerByIdQuery query) => repository.GetById(query.Id);
}
