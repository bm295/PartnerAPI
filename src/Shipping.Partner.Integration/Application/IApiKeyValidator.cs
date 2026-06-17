namespace Shipping.Partner.Integration.Application;

public interface IApiKeyValidator
{
    bool IsValid(string apiKey);
}
