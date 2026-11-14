using Polly;
using System.Net;

namespace Infrastructure.Resilience.Keys;
public static class ResiliencePropertyKeys
{
    // Resilience 
    public static readonly ResiliencePropertyKey<string> RequestUrl = new("Uri");

    // Chaos
    public static readonly ResiliencePropertyKey<bool> ChaosEnabled = new("ChaosEnabled");

    public static readonly ResiliencePropertyKey<double> LatencyInjectionRate = new("LatencyInjectionRate");

    public static readonly ResiliencePropertyKey<TimeSpan> LatencyDuration = new("LatencyDuration");

    public static readonly ResiliencePropertyKey<double> FaultInjectionRate = new("FaultInjectionRate");

    public static readonly ResiliencePropertyKey<Exception> FaultException = new("FaultException");

    public static readonly ResiliencePropertyKey<double> OutcomeInjectionRate = new("OutcomeInjectionRate");

    public static readonly ResiliencePropertyKey<HttpStatusCode> OutcomeHttpStatusCode = new("OutcomeHttpStatusCode");

    public static readonly ResiliencePropertyKey<bool> EnabledByUrl = new("EnabledByUrl");
}
