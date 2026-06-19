using Shipping.Partner.Integration.Application.Abstractions;
using Shipping.Partner.Integration.Application.Commands;
using Shipping.Partner.Integration.Application.Cqrs;
using Shipping.Partner.Integration.Application.Requests;
using Shipping.Partner.Integration.Domain.Entities;
using Shipping.Partner.Integration.Domain.Enums;

namespace Shipping.Partner.Integration.Application.Handlers;

public sealed class RecordShipmentEventCommandHandler(
    IShipmentEventStore store,
    IShippingPartnerRepository repository) : ICommandHandler<RecordShipmentEventCommand, CommandResult<ShipmentEventRecord>>
{
    public CommandResult<ShipmentEventRecord> Handle(RecordShipmentEventCommand command)
    {
        if (!repository.Exists(command.PartnerId))
        {
            return CommandResult<ShipmentEventRecord>.Failure("Unknown partner.");
        }

        if (!Enum.TryParse<ShipmentStatus>(command.Status.Trim(), true, out var status))
        {
            return CommandResult<ShipmentEventRecord>.Failure("Invalid shipment status.");
        }

        var eventRecord = store.Append(new ShipmentEventRequest(
            command.PartnerId,
            command.TrackingNumber,
            status,
            command.Location,
            command.OccurredAtUtc));

        return CommandResult<ShipmentEventRecord>.Success(eventRecord);
    }
}
