using Shipping.Partner.Integration.Domain;

namespace Shipping.Partner.Integration.Application.Results;

public sealed record ShippingOrderCreationResult(ShippingOrder Order, bool Created);
