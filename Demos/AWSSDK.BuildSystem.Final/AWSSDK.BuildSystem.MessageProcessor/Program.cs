using AWSSDK.BuildSystem.MessageProcessor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Contrib.Extensions.AWSXRay.Trace;
using OpenTelemetry.Contrib.Extensions.AWSXRay.Resources;

var traceProviderBuilder = Sdk.CreateTracerProviderBuilder();

#if DEBUG
traceProviderBuilder
    .AddSource(Telemetry.ServiceName)
    .AddAWSInstrumentation()
    .AddConsoleExporter();
#else
traceProviderBuilder
    .AddXRayTraceId()
    .AddSource(Telemetry.ServiceName)
    .AddAWSInstrumentation()
    .SetResourceBuilder(ResourceBuilder
                            .CreateDefault()
                            .AddDetector(new AWSECSResourceDetector())
                            .AddTelemetrySdk())
    .AddOtlpExporter(options =>
    {
        options.Endpoint = new Uri(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT"));
    });

Sdk.SetDefaultTextMapPropagator(new AWSXRayPropagator());
# endif

using var tracerProvider = traceProviderBuilder.Build();


await Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.AddConsole();
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddOptions<AppSettings>().Bind(hostContext.Configuration.GetSection("AppSettings"));

        services.AddAWSService<Amazon.SQS.IAmazonSQS>();
        services.AddAWSService<Amazon.SimpleNotificationService.IAmazonSimpleNotificationService>();
        services.AddHostedService<BuildQueueProcessor>();
    })
    .Build()
    .RunAsync();