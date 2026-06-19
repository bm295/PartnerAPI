using System.Collections.Concurrent;
using Shipping.Partner.Integration.Application.Abstractions;
using Shipping.Partner.Integration.Application.Requests;
using Shipping.Partner.Integration.Domain.Entities;
using Shipping.Partner.Integration.Domain.Rules;

namespace Shipping.Partner.Integration.Infrastructure.Stores;

public sealed class InMemoryShipmentEventStore : IShipmentEventStore
{
    private readonly ConcurrentQueue<ShipmentEventRecord> _events = new();

    public IReadOnlyCollection<ShipmentEventRecord> GetAll() => _events.ToArray();

    public IReadOnlyCollection<ShipmentEventRecord> GetByPartnerId(Guid partnerId) =>
        _events.Where(@event => @event.PartnerId == partnerId).ToArray();

    public ShipmentEventRecord Append(ShipmentEventRequest request)
    {
        var trackingNumber = request.TrackingNumber.Trim();
        var previous = _events
            .Where(@event => @event.PartnerId == request.PartnerId &&
                             string.Equals(@event.TrackingNumber, trackingNumber, StringComparison.Ordinal))
            .OrderByDescending(@event => @event.OccurredAtUtc)
            .FirstOrDefault();

        if (previous is not null && !ShipmentStatusLifecycle.CanTransitionTo(previous.Status, request.Status))
        {
            throw new InvalidOperationException(
                $"Cannot transition shipment {trackingNumber} from {previous.Status} to {request.Status}.");
        }

        var record = new ShipmentEventRecord(
            Guid.NewGuid(),
            request.PartnerId,
            trackingNumber,
            request.Status,
            string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim(),
            request.OccurredAtUtc,
            DateTimeOffset.UtcNow);

        _events.Enqueue(record);
        return record;
    }
}
