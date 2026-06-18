using Shipping.Partner.Integration.Application.Cqrs;
using Shipping.Partner.Integration.Domain;

namespace Shipping.Partner.Integration.Application.Commands;

public sealed record ConnectShippingPartnerCommand(string Name, string ExternalReference)
    : ICommand<ShippingPartnerConnection>;
