using Polly;
using Polly.Timeout;
using System.Net;

namespace Infrastructure.Resilience.Core;

public class ResilienceOptions
{
    private static readonly HttpStatusCode HTTP_STATUS_UNKNOWN_ERROR = (HttpStatusCode)520;
    private static readonly HttpStatusCode HTTP_STATUS_CODE_CONNECTION_TIMEOUT = (HttpStatusCode)522;
    private static readonly HttpStatusCode HTTP_STATUS_CODE_TIMEOUT = (HttpStatusCode)524;
    private static readonly HttpStatusCode HTTP_STATUS_CODE_SSL_ERROR = (HttpStatusCode)525;

    public static readonly HttpStatusCode[] InvalidHttpTransientStatusCodes = [
        HttpStatusCode.RequestTimeout, // 408
        HttpStatusCode.InternalServerError, // 500
        HttpStatusCode.BadGateway, // 502
        HttpStatusCode.ServiceUnavailable, // 503
        HttpStatusCode.GatewayTimeout, // 504
        HTTP_STATUS_UNKNOWN_ERROR,
        HTTP_STATUS_CODE_CONNECTION_TIMEOUT,
        HTTP_STATUS_CODE_TIMEOUT,
        HTTP_STATUS_CODE_SSL_ERROR
        ];

    public PredicateBuilder<HttpResponseMessage> ResultHandlingPredicate { get; } =
        new PredicateBuilder<HttpResponseMessage>()
            .Handle<HttpRequestException>()
            .Handle<TimeoutException>()
            .Handle<TimeoutRejectedException>()
            .HandleResult(result => InvalidHttpTransientStatusCodes
                .Contains(result.StatusCode));

    public int RetryCount { get; init; } = 3;
    public int RetryWaitTime { get; init; } = 1;
    public int TimeoutPerTry { get; init; } = 24;
}
