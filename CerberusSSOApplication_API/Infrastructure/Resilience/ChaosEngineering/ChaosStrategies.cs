using Infrastructure.Resilience.Keys;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Simmy.Fault;
using Polly.Simmy.Latency;
using Polly.Simmy.Outcomes;

namespace Infrastructure.Resilience.ChaosEngineering;

public static class ChaosStrategies
{
    public static ChaosLatencyStrategyOptions LatencyStrategy(ILogger logger)
    {
        return new ChaosLatencyStrategyOptions
        {
            EnabledGenerator = args =>
            {
                var result = new ValueTask<bool>(
                    args.Context.Properties.TryGetValue(ResiliencePropertyKeys.EnabledByUrl, out var isUrlEnabled) && isUrlEnabled);
                logger.LogWarning("ChaosPipeline with AddChaosLatency has " +
                    "EnabledGenerator with enabled set as {IsUrlEnabled}", isUrlEnabled);
                return result;
            },
            InjectionRateGenerator = args =>
            {
                var result = new ValueTask<double>(
                    args.Context.Properties.TryGetValue(ResiliencePropertyKeys.LatencyInjectionRate, out var rate)
                        ? rate : 0);
                logger.LogWarning("ChaosPipeline with AddChaosLatency has " +
                    "InjectionRateGenerator with enabled set as {Rate}", rate);
                return result;
            },
            LatencyGenerator = args =>
            {
                var result = new ValueTask<TimeSpan>(
                    args.Context.Properties.TryGetValue(ResiliencePropertyKeys.LatencyDuration, out var latency)
                        ? latency : TimeSpan.Zero);
                logger.LogWarning("ChaosPipeline with AddChaosLatency has " +
                    "LatencyGenerator with enabled set as {Latency}", latency);
                return result;
            }
        };
    }

    public static ChaosFaultStrategyOptions FaultStrategy(ILogger logger)
    {
        return new ChaosFaultStrategyOptions
        {
            EnabledGenerator = args =>
            {
                var result = new ValueTask<bool>(
                    args.Context.Properties.TryGetValue(ResiliencePropertyKeys.EnabledByUrl, out var isUrlEnabled) && isUrlEnabled);
                logger.LogWarning("ChaosPipeline with AddChaosFault has " +
                    "EnabledGenerator with enabled set as {IsUrlEnabled}", isUrlEnabled);
                return result;
            },
            InjectionRateGenerator = args =>
            {
                var result = new ValueTask<double>(
                    args.Context.Properties.TryGetValue(ResiliencePropertyKeys.FaultInjectionRate, out var rate)
                        ? rate : 0);
                logger.LogWarning("ChaosPipeline with AddChaosFault has " +
                    "InjectionRateGenerator with enabled set as {Rate}", rate);
                return result;
            },
            FaultGenerator = args =>
            {
                args.Context.Properties.TryGetValue(ResiliencePropertyKeys.FaultException, out var exception);
                var result = new ValueTask<Exception?>(exception);
                logger.LogWarning("ChaosPipeline with AddChaosFault has " +
                    "LatencyGenerator with enabled set as {Latency}", exception!.GetType().Name);
                return result;
            }
        };
    }

    public static ChaosOutcomeStrategyOptions<HttpResponseMessage> OutcomeStrategy(ILogger logger)
    {
        return new ChaosOutcomeStrategyOptions<HttpResponseMessage>
        {
            EnabledGenerator = args =>
            {
                var result = new ValueTask<bool>(
                    args.Context.Properties.TryGetValue(ResiliencePropertyKeys.EnabledByUrl, out var isUrlEnabled) && isUrlEnabled);
                logger.LogWarning("ChaosPipeline with AddChaosOutcome has " +
                    "EnabledGenerator with enabled set as {IsUrlEnabled}", isUrlEnabled);
                return result;
            },
            InjectionRateGenerator = args =>
            {
                var result = new ValueTask<double>(
                    args.Context.Properties.TryGetValue(ResiliencePropertyKeys.FaultInjectionRate, out var rate)
                        ? rate : 0);
                logger.LogWarning("ChaosPipeline with AddChaosOutcome has " +
                    "InjectionRateGenerator with enabled set as {Rate}", rate);
                return result;
            },
            OutcomeGenerator = args =>
            {
                if(!args.Context.Properties.TryGetValue(ResiliencePropertyKeys.OutcomeHttpStatusCode, out var status)){
                    logger.LogWarning("ChaosPipeline with AddChaosOutcome has " +
                    "LatencyGenerator with enabled set as {Status}", status);
                    return new ValueTask<Outcome<HttpResponseMessage>?>();
                }

                var response = new HttpResponseMessage(status);
                logger.LogWarning("ChaosPipeline with AddChaosOutcome has " +
                  "OutcomeGenerator with enabled set as {Response}", response);
                return new ValueTask<Outcome<HttpResponseMessage>?>(Outcome.FromResult(response));
            }
        };
    }
}