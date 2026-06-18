using Shipping.Partner.Integration.Application.Abstractions;
using Shipping.Partner.Integration.Application.Cqrs;
using Shipping.Partner.Integration.Application.Queries;
using Shipping.Partner.Integration.Domain;

namespace Shipping.Partner.Integration.Application.Handlers;

public sealed class GetShipmentEventsQueryHandler(
    IShipmentEventStore store) : IQueryHandler<GetShipmentEventsQuery, IReadOnlyCollection<ShipmentEventRecord>>
{
    public IReadOnlyCollection<ShipmentEventRecord> Handle(GetShipmentEventsQuery query) =>
        query.PartnerId is null ? store.GetAll() : store.GetByPartnerId(query.PartnerId.Value);
}
