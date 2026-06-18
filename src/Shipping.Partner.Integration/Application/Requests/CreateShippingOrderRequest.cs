namespace Shipping.Partner.Integration.Application.Requests;

public sealed record CreateShippingOrderRequest(
    Guid PartnerId,
    string OrderNumber,
    string DestinationName,
    string DestinationAddress,
    string ServiceLevel,
    decimal TotalWeightKg);
