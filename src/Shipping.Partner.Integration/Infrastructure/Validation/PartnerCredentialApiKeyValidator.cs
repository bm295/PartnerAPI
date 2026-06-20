using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Shipping.Partner.Integration.Application.Abstractions;
using Shipping.Partner.Integration.Application.Configuration;

namespace Shipping.Partner.Integration.Infrastructure.Validation;

public sealed class PartnerCredentialApiKeyValidator : IApiKeyValidator
{
    private readonly IPartnerCredentialRepository _credentials;
    private readonly IOptions<PartnerCredentialOptions> _options;

    public PartnerCredentialApiKeyValidator(
        IPartnerCredentialRepository credentials,
        IOptions<PartnerCredentialOptions> options)
    {
        _credentials = credentials;
        _options = options;
    }

    public bool IsValid(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return false;
        }

        var nowUtc = DateTimeOffset.UtcNow;
        var hashedSecret = HashSecret(apiKey);
        var credential = _credentials.GetActiveByHashedSecret(
            hashedSecret,
            nowUtc.Subtract(_options.Value.AllowedClockSkew));

        if (credential is null || credential.CreatedAtUtc > nowUtc.Add(_options.Value.AllowedClockSkew))
        {
            return false;
        }

        _credentials.RecordLastUsed(credential.Id, nowUtc);
        return true;
    }

    private static string HashSecret(string secret)
    {
        var secretBytes = Encoding.UTF8.GetBytes(secret);
        var hashBytes = SHA256.HashData(secretBytes);
        return Convert.ToBase64String(hashBytes);
    }
}
