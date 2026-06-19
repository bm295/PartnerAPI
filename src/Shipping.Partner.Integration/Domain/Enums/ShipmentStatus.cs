namespace Shipping.Partner.Integration.Domain.Enums;

public enum ShipmentStatus
{
    LabelCreated = 0,
    PickedUp = 1,
    InTransit = 2,
    OutForDelivery = 3,
    Delivered = 4,
    Exception = 5,
    Returned = 6,
    Cancelled = 7
}
