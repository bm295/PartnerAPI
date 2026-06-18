using Shipping.Partner.Integration.Domain;
using Shipping.Partner.Integration.Application.Requests;

namespace Shipping.Partner.Integration.Application.Abstractions;

public interface IShipmentEventStore
{
    IReadOnlyCollection<ShipmentEventRecord> GetAll();
    IReadOnlyCollection<ShipmentEventRecord> GetByPartnerId(Guid partnerId);
    ShipmentEventRecord Append(ShipmentEventRequest request);
}
