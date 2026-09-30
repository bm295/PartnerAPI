namespace Shipping.Partner.Integration.Api.Contracts;

public sealed record CreateShippingOrderRequest(
    Guid PartnerId,
    string OrderNumber,
    string DestinationName,
    string DestinationAddress,
    string ServiceLevel,
    decimal TotalWeightKg);
