using Shipping.Partner.Integration.Application.Cqrs;
using Shipping.Partner.Integration.Domain.Entities;

namespace Shipping.Partner.Integration.Application.Commands;

public sealed record RecordShipmentEventCommand(
    Guid PartnerId,
    string TrackingNumber,
    string Status,
    string? Location,
    DateTimeOffset OccurredAtUtc) : ICommand<CommandResult<ShipmentEventRecord>>;
