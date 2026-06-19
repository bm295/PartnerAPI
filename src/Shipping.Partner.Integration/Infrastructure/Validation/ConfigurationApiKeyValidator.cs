using Shipping.Partner.Integration.Application.Abstractions;

namespace Shipping.Partner.Integration.Infrastructure.Validation;

public sealed class ConfigurationApiKeyValidator : IApiKeyValidator
{
    private const string ExpectedApiKey = "change-me";

    public bool IsValid(string apiKey) => string.Equals(apiKey, ExpectedApiKey, StringComparison.Ordinal);
}
