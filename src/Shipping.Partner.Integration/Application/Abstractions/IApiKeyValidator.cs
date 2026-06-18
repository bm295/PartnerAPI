namespace Shipping.Partner.Integration.Application.Abstractions;

public interface IApiKeyValidator
{
    bool IsValid(string apiKey);
}
