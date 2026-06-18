using Shipping.Partner.Integration.Application.Abstractions;
using Shipping.Partner.Integration.Application.Commands;
using Shipping.Partner.Integration.Application.Cqrs;
using Shipping.Partner.Integration.Application.Requests;
using Shipping.Partner.Integration.Domain;

namespace Shipping.Partner.Integration.Application.Handlers;

public sealed class CreateShippingOrderCommandHandler(
    IShippingPartnerRepository partnerRepository,
    IShippingOrderRepository orderRepository) : ICommandHandler<CreateShippingOrderCommand, CommandResult<ShippingOrder>>
{
    public CommandResult<ShippingOrder> Handle(CreateShippingOrderCommand command)
    {
        if (!partnerRepository.Exists(command.PartnerId))
        {
            return CommandResult<ShippingOrder>.Failure("Unknown partner.");
        }

        if (command.TotalWeightKg <= 0)
        {
            return CommandResult<ShippingOrder>.Failure("TotalWeightKg must be greater than zero.");
        }

        var order = orderRepository.Create(new CreateShippingOrderRequest(
            command.PartnerId,
            command.OrderNumber,
            command.DestinationName,
            command.DestinationAddress,
            command.ServiceLevel,
            command.TotalWeightKg));

        return CommandResult<ShippingOrder>.Success(order);
    }
}
