namespace Shipping.Partner.Integration.Application;

public sealed record CreateShippingOrderRequest(
    Guid PartnerId,
    string OrderNumber,
    string DestinationName,
    string DestinationAddress,
    string ServiceLevel,
    decimal TotalWeightKg);
