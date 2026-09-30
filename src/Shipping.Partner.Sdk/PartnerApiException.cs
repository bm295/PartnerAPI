using System.Net;

namespace Shipping.Partner.Sdk;

public sealed class PartnerApiException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string? ErrorCode { get; }
    public string? RequestId { get; }

    public PartnerApiException(HttpStatusCode statusCode, string message, string? errorCode, string? requestId)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        RequestId = requestId;
    }
}
