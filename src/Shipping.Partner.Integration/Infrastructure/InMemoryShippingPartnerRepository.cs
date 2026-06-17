using System.Collections.Concurrent;
using Shipping.Partner.Integration.Application;
using Shipping.Partner.Integration.Domain;

namespace Shipping.Partner.Integration.Infrastructure;

public sealed class InMemoryShippingPartnerRepository : IShippingPartnerRepository
{
    private readonly ConcurrentDictionary<Guid, ShippingPartnerConnection> _partners = new();

    public IReadOnlyCollection<ShippingPartnerConnection> GetAll() =>
        _partners.Values.OrderBy(partner => partner.Name).ToArray();

    public ShippingPartnerConnection? GetById(Guid id) => _partners.GetValueOrDefault(id);

    public ShippingPartnerConnection Connect(ConnectShippingPartnerRequest request)
    {
        var partner = new ShippingPartnerConnection(
            Guid.NewGuid(),
            request.Name.Trim(),
            request.ExternalReference.Trim(),
            Convert.ToHexString(Guid.NewGuid().ToByteArray()),
            DateTimeOffset.UtcNow);

        _partners[partner.Id] = partner;
        return partner;
    }

    public bool Exists(Guid id) => _partners.ContainsKey(id);
}
