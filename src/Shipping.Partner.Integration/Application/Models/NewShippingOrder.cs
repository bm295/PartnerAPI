namespace Shipping.Partner.Integration.Application.Models;

public sealed record NewShippingOrder(
    Guid PartnerId,
    string OrderNumber,
    string DestinationName,
    string DestinationAddress,
    string ServiceLevel,
    decimal TotalWeightKg);
