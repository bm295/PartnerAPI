using Shipping.Partner.Integration.Domain.Enums;

namespace Shipping.Partner.Integration.Domain.Rules;

public static class ShipmentStatusLifecycle
{
    private static readonly IReadOnlyDictionary<ShipmentStatus, ShipmentStatus[]> AllowedTransitions =
        new Dictionary<ShipmentStatus, ShipmentStatus[]>
        {
            [ShipmentStatus.LabelCreated] = [ShipmentStatus.PickedUp, ShipmentStatus.Cancelled, ShipmentStatus.Exception],
            [ShipmentStatus.PickedUp] = [ShipmentStatus.InTransit, ShipmentStatus.Exception, ShipmentStatus.Returned, ShipmentStatus.Cancelled],
            [ShipmentStatus.InTransit] = [ShipmentStatus.OutForDelivery, ShipmentStatus.Exception, ShipmentStatus.Returned],
            [ShipmentStatus.OutForDelivery] = [ShipmentStatus.Delivered, ShipmentStatus.Exception, ShipmentStatus.Returned],
            [ShipmentStatus.Delivered] = [],
            [ShipmentStatus.Exception] = [ShipmentStatus.InTransit, ShipmentStatus.OutForDelivery, ShipmentStatus.Delivered, ShipmentStatus.Returned, ShipmentStatus.Cancelled],
            [ShipmentStatus.Returned] = [],
            [ShipmentStatus.Cancelled] = []
        };

    public static bool CanTransitionTo(ShipmentStatus from, ShipmentStatus to) =>
        AllowedTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
}
