using Infrastructure.Resilience.ChaosEngineering;
using Microsoft.Extensions.Options;
using Polly.Registry;
using System;
namespace Infrastructure.Resilience.Core;

public class DynamicPipelineHandler : DelegatingHandler
{
    private readonly ResiliencePipelineProvider<string> _provider;
    private readonly Func<HttpRequestMessage, string?> _selector;
    private readonly ChaosEngineeringOptions _chaosOptions;

    public DynamicPipelineHandler(
        ResiliencePipelineProvider<string> provider,
        Func<HttpRequestMessage, string?> selector,
        IOptionsMonitor<ChaosEngineeringOptions> optionsMonitor)
    {
        _provider = provider;
        _selector = selector;
        _chaosOptions = optionsMonitor.CurrentValue;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return null;
    }
}
