using Elastic.Apm.SerilogEnricher;
using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;
using System.Globalization;

namespace Learning.InternalGateway.Shared;

public static class LoggerConfig
{
    public static IServiceCollection ConfigureLogger(this IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration configuration,
        string serviceName)
    {
        var env = environment.EnvironmentName;

        var loggerConfiguration = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithElasticApmCorrelationInfo()
            .Enrich.WithEnvironmentName()
            .Enrich.WithSpan(new SpanOptions
            {
                IncludeBaggage = true,
                IncludeOperationName = true,
                IncludeTags = true,
                LogEventPropertiesNames = new SpanLogEventPropertiesNames
                {
                    SpanId = "span.id",
                    ParentId = "parent.id",
                    TraceId = "trace.id"
                }
            })
            .WriteTo.Async(x =>
            {
                x.Elasticsearch(
                    nodes: [new Uri("http://localhost:9200")], configureOptions: opts =>
                    {
                        opts.BootstrapMethod = BootstrapMethod.Silent;
                        opts.DataStream = new DataStreamName($"dotnet-{env.ToLower(CultureInfo.InvariantCulture)}-{serviceName}");
                    },
                    transport =>
                    {
                        //transport.Authentication(new BasicAuthentication("", ""));
                        transport.ServerCertificateValidationCallback((o, certificate, arg3, arg4) => true);
                    },
                    restrictedToMinimumLevel: LogEventLevel.Information);
            });

        Log.Logger = loggerConfiguration.CreateLogger();


        return services;
    }
}
