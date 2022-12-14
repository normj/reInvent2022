using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using AWSSDK.BuildSystem.Common;
using System.Text.Json;
using System.Text.Json.Serialization;
using AWSSDK.BuildSystem.LambdaFailedBuildNotifier;
using OpenTelemetry.Contrib.Instrumentation.AWSLambda.Implementation;
using OpenTelemetry.Trace;
using OpenTelemetry;
using OpenTelemetry.Contrib.Extensions.AWSXRay.Trace;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.SourceGeneratorLambdaJsonSerializer<LambdaFunctionJsonSerializerContext>))]

namespace AWSSDK.BuildSystem.LambdaFailedBuildNotifier;

public class Functions
{
    TracerProvider _traceProvider;

    public Functions()
    {
        _traceProvider = Sdk.CreateTracerProviderBuilder()
            .AddXRayTraceId()
            .AddSource(Telemetry.ServiceName)
            .AddAWSInstrumentation()
            .AddAWSLambdaConfigurations()
            .AddOtlpExporter()
            .Build();

        Sdk.SetDefaultTextMapPropagator(new AWSXRayPropagator());
    }

    public Task ProcessFailedBuild(SNSEvent evnt, ILambdaContext context)
        => AWSLambdaWrapper.Trace(_traceProvider, InstrumentedProcessFailedBuild, evnt, context);

    public async Task InstrumentedProcessFailedBuild(SNSEvent evnt, ILambdaContext context)
    {
        using var activity = Telemetry.RootActivitySource.StartActivity("Start processing SNS event");
        foreach(var message in evnt.Records)
        {
            var buildStatusMessage = JsonSerializer.Deserialize<BuildStatusMessage>(message.Sns.Message);
            using var buildActivity = Telemetry.RootActivitySource.StartActivity("Notifiy for build failure");
            buildActivity.AddTag("BuildId", buildStatusMessage.BuildId);

            context.Logger.LogError($"Build {buildStatusMessage.BuildId} failed: {buildStatusMessage.BuildException}");

            // TODO: Page .NET on-call engineer for build failure.
            await Task.Delay(1000);
        }
        await Task.CompletedTask;
    }
}

/// <summary>
/// This class is used to register the input event and return type for the FunctionHandler method with the System.Text.Json source generator.
/// There must be a JsonSerializable attribute for each type used as the input and return type or a runtime error will occur 
/// from the JSON serializer unable to find the serialziation information for unknown types.
/// </summary>
[JsonSerializable(typeof(SNSEvent))]
public partial class LambdaFunctionJsonSerializerContext : JsonSerializerContext
{
    // By using this partial class derived from JsonSerializerContext, we can generate reflection free JSON Serializer code at compile time
    // which can deserialize our class and properties. However, we must attribute this class to tell it what types to generate serialization code for.
    // See https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-source-generation
}