namespace Shipping.Partner.Sdk.Models;

public sealed record CreateShippingOrderRequest(
    Guid PartnerId,
    string OrderNumber,
    string DestinationName,
    string DestinationAddress,
    string ServiceLevel,
    decimal TotalWeightKg);
