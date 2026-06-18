using Shipping.Partner.Integration.Application.Cqrs;
using Shipping.Partner.Integration.Domain;

namespace Shipping.Partner.Integration.Application.Queries;

public sealed record GetShipmentEventsQuery(Guid? PartnerId) : IQuery<IReadOnlyCollection<ShipmentEventRecord>>;
