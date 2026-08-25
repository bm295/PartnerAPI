using Shipping.Partner.Integration.Domain.Entities;
using Shipping.Partner.Integration.Application.Models;

namespace Shipping.Partner.Integration.Application.Abstractions;

public interface IShippingPartnerRepository
{
    IReadOnlyCollection<ShippingPartnerConnection> GetAll();
    ShippingPartnerConnection? GetById(Guid id);
    ShippingPartnerConnection Connect(NewShippingPartner partner);
    bool Exists(Guid id);
}
