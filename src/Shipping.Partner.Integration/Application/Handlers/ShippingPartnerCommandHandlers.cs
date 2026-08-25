using Shipping.Partner.Integration.Application.Abstractions;
using Shipping.Partner.Integration.Application.Commands;
using Shipping.Partner.Integration.Application.Cqrs;
using Shipping.Partner.Integration.Application.Models;
using Shipping.Partner.Integration.Domain.Entities;

namespace Shipping.Partner.Integration.Application.Handlers;

public sealed class ConnectShippingPartnerCommandHandler(
    IShippingPartnerRepository repository) : ICommandHandler<ConnectShippingPartnerCommand, ShippingPartnerConnection>
{
    public ShippingPartnerConnection Handle(ConnectShippingPartnerCommand command) =>
        repository.Connect(new NewShippingPartner(command.Name, command.ExternalReference));
}
