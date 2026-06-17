using Shipping.Partner.Integration.Domain;

namespace Shipping.Partner.Integration.Application;

public interface IShipmentEventStore
{
    IReadOnlyCollection<ShipmentEventRecord> GetAll();
    IReadOnlyCollection<ShipmentEventRecord> GetByPartnerId(Guid partnerId);
    ShipmentEventRecord Append(ShipmentEventRequest request);
}
