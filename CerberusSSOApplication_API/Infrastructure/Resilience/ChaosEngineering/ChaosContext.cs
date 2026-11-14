using Infrastructure.Resilience.Keys;
using Microsoft.AspNetCore.Http;
using Polly;
using System.Net;

namespace Infrastructure.Resilience.ChaosEngineering;

public class ChaosContext
{
    public bool Enabled { get; init; }
    public double LatencyInjectionRate {  get; init; }
    public TimeSpan LatencyDuration {  get; init; }
    public double FaultInjectionRate {  get; init; }
    public Exception FaultException { get; init;  }
    public  double OutcomeInjectionRate { get; init; }
    public HttpStatusCode OutcomeHttpStatusCode { get; init; } 
    public bool EnabledByUrl { get; init; }

    public ChaosContext(HttpContext httpContext)
    {
        Enabled = ChaosCommon.GetPropertyFromHttpHeader<bool>(
            httpContext, ChaosCommon.CHAOS_ENABLED);
        LatencyInjectionRate = ChaosCommon.GetPropertyFromHttpHeader<double>(
            httpContext, ChaosCommon.CHAOS_LATENCY_INJECTION_RATE);
        LatencyDuration = TimeSpan.FromMilliseconds(ChaosCommon.GetPropertyFromHttpHeader<int>(
            httpContext, ChaosCommon.CHAOS_LATENCY_DURATION_MS));
        FaultInjectionRate = ChaosCommon.GetPropertyFromHttpHeader<double>(
            httpContext, ChaosCommon.CHAOS_FAULT_INJECTION_RATE);
        FaultException = ChaosCommon.GetExceptionFromString(
            ChaosCommon.GetPropertyFromHttpHeader<string>(httpContext, 
            ChaosCommon.CHAOS_FAULT_EXCEPTION));
        OutcomeInjectionRate = default;
        OutcomeHttpStatusCode = default;
        EnabledByUrl = true;
    }

    public ChaosContext(ChaosEngineeringOptions chaosOptions)
    {
        Enabled = chaosOptions.Enabled;
        LatencyInjectionRate = chaosOptions.Inbound.LatencyInjectionRate;
        LatencyDuration = TimeSpan.FromMilliseconds(chaosOptions.Inbound.LatencyDurationMs);
        FaultInjectionRate = chaosOptions.Inbound.LatencyInjectionRate;
        FaultException = ChaosCommon.GetExceptionFromString(chaosOptions.Inbound.FaultException!);
        OutcomeInjectionRate = default;
        OutcomeHttpStatusCode = default;
        EnabledByUrl = true;
    }

    public ChaosContext(ChaosEngineeringOptions chaosOptions, string currentUrl)
    {
        Enabled = chaosOptions.Enabled;
        LatencyInjectionRate = chaosOptions.Outbound.LatencyInjectionRate;
        LatencyDuration = TimeSpan.FromMilliseconds(chaosOptions.Outbound.LatencyDurationMs);
        FaultInjectionRate = chaosOptions.Outbound.LatencyInjectionRate;
        FaultException = ChaosCommon.GetExceptionFromString(chaosOptions.Outbound.FaultException!);
        OutcomeInjectionRate = chaosOptions.Outbound.OutcomeInjectionRate;
        OutcomeHttpStatusCode = ChaosCommon.IntToHttpStatusCode(chaosOptions.Outbound.OutcomeHTTPResponse);
        EnabledByUrl = ChaosCommon.IsInEnabledUrlList(chaosOptions.Outbound.EnabledUrlList, currentUrl);
    }

    public void SetChaosPropertiesToResilienceContext(ResilienceContext resilienceContext) {
        resilienceContext.Properties.Set(ResiliencePropertyKeys.ChaosEnabled, Enabled);
        resilienceContext.Properties.Set(ResiliencePropertyKeys.LatencyInjectionRate, LatencyInjectionRate);
        resilienceContext.Properties.Set(ResiliencePropertyKeys.LatencyDuration, LatencyDuration);
        resilienceContext.Properties.Set(ResiliencePropertyKeys.FaultInjectionRate, FaultInjectionRate);
        resilienceContext.Properties.Set(ResiliencePropertyKeys.FaultException, FaultException);
        resilienceContext.Properties.Set(ResiliencePropertyKeys.OutcomeInjectionRate, OutcomeInjectionRate);
        resilienceContext.Properties.Set(ResiliencePropertyKeys.OutcomeHttpStatusCode, OutcomeHttpStatusCode);
        resilienceContext.Properties.Set(ResiliencePropertyKeys.EnabledByUrl, EnabledByUrl);
    }
}
