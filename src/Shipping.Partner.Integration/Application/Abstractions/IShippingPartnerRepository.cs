using Shipping.Partner.Integration.Domain;
using Shipping.Partner.Integration.Application.Requests;

namespace Shipping.Partner.Integration.Application.Abstractions;

public interface IShippingPartnerRepository
{
    IReadOnlyCollection<ShippingPartnerConnection> GetAll();
    ShippingPartnerConnection? GetById(Guid id);
    ShippingPartnerConnection Connect(ConnectShippingPartnerRequest request);
    bool Exists(Guid id);
}
