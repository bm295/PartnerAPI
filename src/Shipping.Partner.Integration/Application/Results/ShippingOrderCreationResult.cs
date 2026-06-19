using Shipping.Partner.Integration.Domain.Entities;

namespace Shipping.Partner.Integration.Application.Results;

public sealed record ShippingOrderCreationResult(ShippingOrder Order, bool Created);
