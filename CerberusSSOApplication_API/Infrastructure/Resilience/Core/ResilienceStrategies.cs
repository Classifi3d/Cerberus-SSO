using Infrastructure.Resilience.Keys;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using Polly.Timeout;

namespace Infrastructure.Resilience.Core;


public static class ResilienceStrategies
{
    public static TimeoutStrategyOptions TimeoutStrategy(ResilienceOptions options, ILogger logger)
    {
        return new TimeoutStrategyOptions
        {
            TimeoutGenerator = args => new ValueTask<TimeSpan>(TimeSpan.FromSeconds(options.TimeoutPerTry)),
            OnTimeout = args =>
            {
                args.Context.Properties.TryGetValue(ResiliencePropertyKeys.RequestUrl, out var requestUri);
                // logging
                return ValueTask.CompletedTask;
            }
        };
    }

    public static RetryStrategyOptions<HttpResponseMessage> RetryStrategy(ResilienceOptions options, ILogger logger)
    {
        return new RetryStrategyOptions<HttpResponseMessage>
        {
            ShouldHandle = options.ResultHandlingPredicate,
            BackoffType = DelayBackoffType.Constant,
            MaxRetryAttempts = options.RetryCount,
            DelayGenerator = args => new ValueTask<TimeSpan?>(TimeSpan.FromSeconds(options.RetryWaitTime)),
            OnRetry = args =>
            {
                args.Context.Properties.TryGetValue(ResiliencePropertyKeys.RequestUrl, out var requestUri);

                // logging
                return ValueTask.CompletedTask;
            }
        };
    }
}