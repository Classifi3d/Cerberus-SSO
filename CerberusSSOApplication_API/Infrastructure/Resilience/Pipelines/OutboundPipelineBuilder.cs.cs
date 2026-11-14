using Infrastructure.Resilience.ChaosEngineering;
using Infrastructure.Resilience.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Simmy;

namespace Infrastructure.Resilience.Pipelines;

public interface IOutboundPipelineBuilder<HttpResponseMessage>
{
    void Configure(ResiliencePipelineBuilder<HttpResponseMessage> builder);
}

public sealed class OutboundPipelineBuilder : IOutboundPipelineBuilder<HttpResponseMessage>
{

    private readonly ResilienceOptions _options;
    private readonly ChaosEngineeringOptions _chaosOptions;
    private readonly ILogger<OutboundPipelineBuilder> _logger;

    public OutboundPipelineBuilder(
        IOptions<ResilienceOptions> options,
        IOptionsMonitor<ChaosEngineeringOptions> optionsMonitor,
        ILogger<OutboundPipelineBuilder> logger)
    {
        _options = options.Value;
        _chaosOptions = optionsMonitor.CurrentValue;
        _logger = logger;
    }

    public void Configure(ResiliencePipelineBuilder<HttpResponseMessage> builder)
    {
        builder
            .AddRetry(ResilienceStrategies.RetryStrategy(_options, _logger))
            .AddTimeout(ResilienceStrategies.TimeoutStrategy(_options, _logger));
        if (_chaosOptions.Enabled && _chaosOptions.Outbound.Enabled)
        {
            builder
                .AddChaosLatency(ChaosStrategies.LatencyStrategy(_logger))
                .AddChaosFault(ChaosStrategies.FaultStrategy(_logger))
                .AddChaosOutcome(ChaosStrategies.OutcomeStrategy(_logger));
        }
    }
}
