using Shipping.Partner.Integration.Application;

namespace Shipping.Partner.Integration.Infrastructure;

public sealed class ConfigurationApiKeyValidator : IApiKeyValidator
{
    private const string ExpectedApiKey = "change-me";

    public bool IsValid(string apiKey) => string.Equals(apiKey, ExpectedApiKey, StringComparison.Ordinal);
}
