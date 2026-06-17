using Shipping.Partner.Integration.Domain;

namespace Shipping.Partner.Integration.Application;

public interface IShippingPartnerRepository
{
    IReadOnlyCollection<ShippingPartnerConnection> GetAll();
    ShippingPartnerConnection? GetById(Guid id);
    ShippingPartnerConnection Connect(ConnectShippingPartnerRequest request);
    bool Exists(Guid id);
}
