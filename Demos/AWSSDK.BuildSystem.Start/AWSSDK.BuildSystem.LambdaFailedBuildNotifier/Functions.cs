using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using AWSSDK.BuildSystem.Common;
using System.Text.Json;
using System.Text.Json.Serialization;
using AWSSDK.BuildSystem.LambdaFailedBuildNotifier;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.SourceGeneratorLambdaJsonSerializer<LambdaFunctionJsonSerializerContext>))]

namespace AWSSDK.BuildSystem.LambdaFailedBuildNotifier;

public class Functions
{
    private readonly JsonSerializerOptions _jsonOptions;


    public Functions()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            Converters = {
                    new JsonStringEnumConverter()
                }
        };
    }


    public async Task ProcessFailedBuild(SNSEvent evnt, ILambdaContext context)
    {
        foreach(var message in evnt.Records)
        {
            var buildStatusMessage = JsonSerializer.Deserialize<BuildStatusMessage>(message.Sns.Message, _jsonOptions);

            context.Logger.LogError($"Build {buildStatusMessage.BuildId} failed: {buildStatusMessage.BuildException}");

            // TODO: Page .NET on-call engineer for build failure.
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