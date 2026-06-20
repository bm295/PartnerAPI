using Shipping.Partner.Integration.Application.Abstractions;
using Shipping.Partner.Integration.Domain.Entities;

namespace Shipping.Partner.Integration.Infrastructure.Repositories;

public sealed class InMemoryPartnerCredentialRepository : IPartnerCredentialRepository
{
    private readonly List<PartnerCredential> _credentials = [];
    private readonly object _syncRoot = new();

    public PartnerCredential Create(Guid partnerId, string hashedSecret, DateTimeOffset expiresAtUtc)
    {
        var credential = new PartnerCredential(
            Guid.NewGuid(),
            partnerId,
            hashedSecret,
            DateTimeOffset.UtcNow,
            expiresAtUtc,
            null,
            null);

        lock (_syncRoot)
        {
            _credentials.Add(credential);
        }

        return credential;
    }

    public PartnerCredential? Rotate(Guid partnerId, Guid credentialId, string hashedSecret, DateTimeOffset expiresAtUtc)
    {
        lock (_syncRoot)
        {
            var existingIndex = _credentials.FindIndex(credential => credential.Id == credentialId && credential.PartnerId == partnerId);
            if (existingIndex < 0)
            {
                return null;
            }

            var nowUtc = DateTimeOffset.UtcNow;
            _credentials[existingIndex] = _credentials[existingIndex] with { RevokedAtUtc = nowUtc };

            var replacement = new PartnerCredential(
                Guid.NewGuid(),
                partnerId,
                hashedSecret,
                nowUtc,
                expiresAtUtc,
                null,
                null);

            _credentials.Add(replacement);
            return replacement;
        }
    }

    public bool Revoke(Guid partnerId, Guid credentialId, DateTimeOffset revokedAtUtc)
    {
        lock (_syncRoot)
        {
            var existingIndex = _credentials.FindIndex(credential => credential.Id == credentialId && credential.PartnerId == partnerId);
            if (existingIndex < 0)
            {
                return false;
            }

            _credentials[existingIndex] = _credentials[existingIndex] with { RevokedAtUtc = revokedAtUtc };
            return true;
        }
    }

    public PartnerCredential? GetById(Guid id)
    {
        lock (_syncRoot)
        {
            return _credentials.FirstOrDefault(credential => credential.Id == id);
        }
    }

    public IReadOnlyCollection<PartnerCredential> GetByPartnerId(Guid partnerId)
    {
        lock (_syncRoot)
        {
            return _credentials.Where(credential => credential.PartnerId == partnerId).ToArray();
        }
    }

    public PartnerCredential? GetActiveByHashedSecret(string hashedSecret, DateTimeOffset nowUtc)
    {
        lock (_syncRoot)
        {
            return _credentials.FirstOrDefault(credential =>
                string.Equals(credential.HashedSecret, hashedSecret, StringComparison.Ordinal) &&
                credential.RevokedAtUtc is null &&
                credential.ExpiresAtUtc > nowUtc);
        }
    }

    public bool RecordLastUsed(Guid credentialId, DateTimeOffset lastUsedAtUtc)
    {
        lock (_syncRoot)
        {
            var existingIndex = _credentials.FindIndex(credential => credential.Id == credentialId);
            if (existingIndex < 0)
            {
                return false;
            }

            _credentials[existingIndex] = _credentials[existingIndex] with { LastUsedAtUtc = lastUsedAtUtc };
            return true;
        }
    }
}
