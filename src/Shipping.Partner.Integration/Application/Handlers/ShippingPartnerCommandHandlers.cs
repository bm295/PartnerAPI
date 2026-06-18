using Shipping.Partner.Integration.Application.Abstractions;
using Shipping.Partner.Integration.Application.Commands;
using Shipping.Partner.Integration.Application.Cqrs;
using Shipping.Partner.Integration.Application.Requests;
using Shipping.Partner.Integration.Domain;

namespace Shipping.Partner.Integration.Application.Handlers;

public sealed class ConnectShippingPartnerCommandHandler(
    IShippingPartnerRepository repository) : ICommandHandler<ConnectShippingPartnerCommand, ShippingPartnerConnection>
{
    public ShippingPartnerConnection Handle(ConnectShippingPartnerCommand command) =>
        repository.Connect(new ConnectShippingPartnerRequest(command.Name, command.ExternalReference));
}
