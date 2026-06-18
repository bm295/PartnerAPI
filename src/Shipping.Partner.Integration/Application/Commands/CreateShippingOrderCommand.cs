using Shipping.Partner.Integration.Application.Cqrs;
using Shipping.Partner.Integration.Domain;

namespace Shipping.Partner.Integration.Application.Commands;

public sealed record CreateShippingOrderCommand(
    Guid PartnerId,
    string OrderNumber,
    string DestinationName,
    string DestinationAddress,
    string ServiceLevel,
    decimal TotalWeightKg) : ICommand<CommandResult<ShippingOrder>>;
