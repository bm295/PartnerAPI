namespace Shipping.Partner.Integration.Domain.Entities;

public sealed record PartnerCredential(
    Guid Id,
    Guid PartnerId,
    string HashedSecret,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset ExpiresAtUtc,
    DateTimeOffset? RevokedAtUtc,
    DateTimeOffset? LastUsedAtUtc);
