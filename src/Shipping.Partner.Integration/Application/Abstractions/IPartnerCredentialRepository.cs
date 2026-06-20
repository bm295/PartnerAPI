using Shipping.Partner.Integration.Domain.Entities;

namespace Shipping.Partner.Integration.Application.Abstractions;

public interface IPartnerCredentialRepository
{
    PartnerCredential Create(Guid partnerId, string hashedSecret, DateTimeOffset expiresAtUtc);
    PartnerCredential? Rotate(Guid partnerId, Guid credentialId, string hashedSecret, DateTimeOffset expiresAtUtc);
    bool Revoke(Guid partnerId, Guid credentialId, DateTimeOffset revokedAtUtc);
    PartnerCredential? GetById(Guid id);
    IReadOnlyCollection<PartnerCredential> GetByPartnerId(Guid partnerId);
    PartnerCredential? GetActiveByHashedSecret(string hashedSecret, DateTimeOffset nowUtc);
    bool RecordLastUsed(Guid credentialId, DateTimeOffset lastUsedAtUtc);
}
