using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;

namespace Infrastructure.Resilience.ChaosEngineering;

public class ChaosInboundMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ResiliencePipeline _pipeline;
    private readonly ChaosEngineeringOptions _chaosOptions;
    private readonly ILogger<ChaosInboundMiddleware> _logger;

    public ChaosInboundMiddleware(RequestDelegate next, ResiliencePipelineProvider<string> pipelineProvider, ChaosEngineeringOptions chaosOptions, ILogger<ChaosInboundMiddleware> logger)
    {
        _next = next;
        _pipeline = pipelineProvider.GetPipeline("InboundChaosPipeline");
        _chaosOptions = chaosOptions;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        var chaosHttpHeadersEnabled = _chaosOptions.Inbound.HTTPHeaderEnabled;
        bool chaosHeaderPresent = httpContext.Request.Headers.ContainsKey(ChaosCommon.CHAOS_ENABLED);

        _logger.LogDebug("ChaosMiddleware entered!");

        if(chaosHttpHeadersEnabled && !chaosHeaderPresent)
        {
            await _next(httpContext);
            return;
        }

        var resilienceContext = ResilienceContextPool.Shared.Get();

        try
        {
            ChaosContext chaosContext;

            if (!chaosHttpHeadersEnabled)
            {
                chaosContext = new ChaosContext(_chaosOptions);
            }
            else
            {
                chaosContext = new ChaosContext(httpContext);
            }
            chaosContext.SetChaosPropertiesToResilienceContext(resilienceContext);

            _logger.LogDebug("ChaosResilience is executed with ...");

            await _pipeline.ExecuteAsync(
                callback: async (ctx) => await _next(httpContext),
                context: resilienceContext
            );
        }
        finally
        {
            ResilienceContextPool.Shared.Return(resilienceContext);
        }
    }

}
