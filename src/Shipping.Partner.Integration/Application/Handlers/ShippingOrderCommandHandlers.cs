using Shipping.Partner.Integration.Application.Abstractions;
using Shipping.Partner.Integration.Application.Commands;
using Shipping.Partner.Integration.Application.Cqrs;
using Shipping.Partner.Integration.Application.Requests;
using Shipping.Partner.Integration.Application.Results;
using Shipping.Partner.Integration.Domain.Entities;

namespace Shipping.Partner.Integration.Application.Handlers;

public sealed class CreateShippingOrderCommandHandler(
    IShippingPartnerRepository partnerRepository,
    IShippingOrderRepository orderRepository) : ICommandHandler<CreateShippingOrderCommand, CommandResult<ShippingOrderCreationResult>>
{
    public CommandResult<ShippingOrderCreationResult> Handle(CreateShippingOrderCommand command)
    {
        if (!partnerRepository.Exists(command.PartnerId))
        {
            return CommandResult<ShippingOrderCreationResult>.Failure("Unknown partner.");
        }

        if (command.TotalWeightKg <= 0)
        {
            return CommandResult<ShippingOrderCreationResult>.Failure("TotalWeightKg must be greater than zero.");
        }

        var order = orderRepository.Create(new CreateShippingOrderRequest(
            command.PartnerId,
            command.OrderNumber,
            command.DestinationName,
            command.DestinationAddress,
            command.ServiceLevel,
            command.TotalWeightKg));

        return CommandResult<ShippingOrderCreationResult>.Success(order);
    }
}
