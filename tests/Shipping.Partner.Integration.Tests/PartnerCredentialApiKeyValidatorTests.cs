using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Shipping.Partner.Integration.Application.Abstractions;
using Shipping.Partner.Integration.Application.Configuration;
using Shipping.Partner.Integration.Domain.Entities;
using Shipping.Partner.Integration.Infrastructure.Validation;
using Xunit;

namespace Shipping.Partner.Integration.Tests;

public class PartnerCredentialApiKeyValidatorTests
{
    [Fact]
    public void IsValid_ShouldHashSecretAndRecordCredentialUse()
    {
        var now = DateTimeOffset.UtcNow;
        var credential = new PartnerCredential(
            Guid.NewGuid(), Guid.NewGuid(), Hash("secret"), now.AddMinutes(-1), now.AddHours(1), null, null);
        var repository = new CredentialRepositoryStub(credential);
        var validator = new PartnerCredentialApiKeyValidator(
            repository,
            Options.Create(new PartnerCredentialOptions { AllowedClockSkew = TimeSpan.FromMinutes(5) }),
            TimeProvider.System);

        var valid = validator.IsValid("secret");

        Assert.True(valid);
        Assert.Equal(Hash("secret"), repository.RequestedHash);
        Assert.Equal(credential.Id, repository.RecordedCredentialId);
        Assert.NotNull(repository.RecordedAtUtc);
    }

    [Fact]
    public void IsValid_ShouldNotRecordUseWhenCredentialDoesNotMatch()
    {
        var repository = new CredentialRepositoryStub(null);
        var validator = new PartnerCredentialApiKeyValidator(
            repository,
            Options.Create(new PartnerCredentialOptions()),
            TimeProvider.System);

        var valid = validator.IsValid("wrong-secret");

        Assert.False(valid);
        Assert.Null(repository.RecordedCredentialId);
    }

    [Fact]
    public void IsValid_ShouldUseInjectedTimeForExpiryQueryAndLastUsedTimestamp()
    {
        var now = new DateTimeOffset(2026, 8, 25, 9, 0, 0, TimeSpan.Zero);
        var credential = new PartnerCredential(
            Guid.NewGuid(), Guid.NewGuid(), Hash("secret"), now.AddMinutes(-1), now.AddHours(1), null, null);
        var repository = new CredentialRepositoryStub(credential);
        var validator = new PartnerCredentialApiKeyValidator(
            repository,
            Options.Create(new PartnerCredentialOptions { AllowedClockSkew = TimeSpan.FromMinutes(5) }),
            new FixedTimeProvider(now));

        var valid = validator.IsValid("secret");

        Assert.True(valid);
        Assert.Equal(now.AddMinutes(-5), repository.RequestedAtUtc);
        Assert.Equal(now, repository.RecordedAtUtc);
    }

    [Fact]
    public void IsValid_ShouldRejectCredentialCreatedBeyondAllowedClockSkew()
    {
        var now = new DateTimeOffset(2026, 8, 25, 9, 0, 0, TimeSpan.Zero);
        var credential = new PartnerCredential(
            Guid.NewGuid(), Guid.NewGuid(), Hash("secret"), now.AddMinutes(6), now.AddHours(1), null, null);
        var repository = new CredentialRepositoryStub(credential);
        var validator = new PartnerCredentialApiKeyValidator(
            repository,
            Options.Create(new PartnerCredentialOptions { AllowedClockSkew = TimeSpan.FromMinutes(5) }),
            new FixedTimeProvider(now));

        var valid = validator.IsValid("secret");

        Assert.False(valid);
        Assert.Null(repository.RecordedCredentialId);
    }

    private static string Hash(string value) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private sealed class CredentialRepositoryStub(PartnerCredential? credential) : IPartnerCredentialRepository
    {
        public string? RequestedHash { get; private set; }
        public Guid? RecordedCredentialId { get; private set; }
        public DateTimeOffset? RecordedAtUtc { get; private set; }
        public DateTimeOffset? RequestedAtUtc { get; private set; }

        public PartnerCredential Create(Guid partnerId, string hashedSecret, DateTimeOffset expiresAtUtc) =>
            throw new NotSupportedException();

        public PartnerCredential? Rotate(
            Guid partnerId,
            Guid credentialId,
            string hashedSecret,
            DateTimeOffset expiresAtUtc) => throw new NotSupportedException();

        public bool Revoke(Guid partnerId, Guid credentialId, DateTimeOffset revokedAtUtc) =>
            throw new NotSupportedException();

        public PartnerCredential? GetById(Guid id) => throw new NotSupportedException();

        public IReadOnlyCollection<PartnerCredential> GetByPartnerId(Guid partnerId) =>
            throw new NotSupportedException();

        public PartnerCredential? GetActiveByHashedSecret(string hashedSecret, DateTimeOffset nowUtc)
        {
            RequestedHash = hashedSecret;
            RequestedAtUtc = nowUtc;
            return credential;
        }

        public bool RecordLastUsed(Guid credentialId, DateTimeOffset lastUsedAtUtc)
        {
            RecordedCredentialId = credentialId;
            RecordedAtUtc = lastUsedAtUtc;
            return true;
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
