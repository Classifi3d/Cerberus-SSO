using Infrastructure.Resilience.ChaosEngineering;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Simmy;

namespace Infrastructure.Resilience.Pipelines;

public interface IInboundPipelineBuilder
{
    void Configure(ResiliencePipelineBuilder builder);
}

public sealed class InboundPipelineBuilder : IInboundPipelineBuilder
{

    private readonly ILogger<InboundPipelineBuilder> _logger;

    public InboundPipelineBuilder(ILogger<InboundPipelineBuilder> logger)
    {
        _logger = logger;
    }

    public void Configure(ResiliencePipelineBuilder builder)
    {
        builder
            .AddChaosLatency(ChaosStrategies.LatencyStrategy(_logger))
            .AddChaosFault(ChaosStrategies.FaultStrategy(_logger));

    }
}
