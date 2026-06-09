using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

namespace Presentation.Extensions;

public static class LoggerConfigurator
{
    public static Logger ConfigureLogger(IConfiguration configuration)
    {
        var serilogConfig = new LoggerConfiguration();
        // 1. Minium Level
        serilogConfig
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning);
        // 2. Filters

        // 3.Enrichers
        serilogConfig
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.FromLogContext();

        // 4. Sinks
        var elasticUri = configuration["Elasticsearch:Uri"] ?? "";
        serilogConfig
            .WriteTo.Console()
            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticUri))
            {
                AutoRegisterTemplate = true,
                IndexFormat = "mfawebapplication-development-{0:yyyy-MM}",
                NumberOfShards = 1,
                NumberOfReplicas = 1,
                TypeName = null
            });

        return serilogConfig.CreateLogger();
    }

}
