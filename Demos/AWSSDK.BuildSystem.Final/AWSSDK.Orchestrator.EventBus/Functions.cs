using Amazon.Lambda.Core;
using Amazon.Lambda.CloudWatchEvents;
using AWSSDK.BuildSystem.Common;
using Amazon.Lambda.Annotations;
using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;
using OpenTelemetry.Contrib.Extensions.AWSXRay.Trace;
using OpenTelemetry.Trace;
using OpenTelemetry;
using OpenTelemetry.Contrib.Instrumentation.AWSLambda.Implementation;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWSSDK.Orchestrator.EventBus;

public class Functions
{
    private OrchestratorManager _orchestratorManager;
    private IAmazonSQS _sqsClient;
    private TracerProvider _tracerProvider;  

    public Functions(OrchestratorManager orchestratorManager, IAmazonSQS sqsClient, TracerProvider tracerProvider)
    {
        _orchestratorManager = orchestratorManager;
        _sqsClient = sqsClient;
        _tracerProvider = tracerProvider;
    }

    // Using Amazon.Lambda.Annotations to define Lambda Functions
    //   * Syncs with CloudFormation
    //   * Integrates Dependency Injection
    [LambdaFunction(Name = nameof(PreviewBuildHandler))]
    public async Task PreviewBuildHandler(CloudWatchEvent<PreviewMessage> evnt, ILambdaContext context) =>
        await AWSLambdaWrapper.Trace(_tracerProvider, InstrumentedPreviewBuildHandler, evnt, context);

    public async Task InstrumentedPreviewBuildHandler(CloudWatchEvent<PreviewMessage> evnt, ILambdaContext context)
    {
        context.Logger.LogInformation("Preparing preview build: " + evnt.Detail.ServiceName);

        var previewMessage = evnt.Detail;
        var message = JsonSerializer.Serialize(previewMessage);

        var queueUrls = await _orchestratorManager.GetBuildQueuesAsync(BuildType.PreviewBuild);
        foreach (var queueUrl in queueUrls)
        {
            var sendRequest = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = message,
                MessageGroupId = previewMessage.ServiceName,

                // Use attributes so processor can decide what action to take without having to parse message body
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {Constants.BUILD_TYPE_MESSAGE_ATTRIBUTE_KEY, new MessageAttributeValue{StringValue = BuildType.PreviewBuild.ToString(), DataType = "String"} },
                    {Constants.BUILD_ID_MESSAGE_ATTRIBUTE_KEY, new MessageAttributeValue{StringValue = previewMessage.BuildId, DataType = "String"} }
                }
            };

            context.Logger.LogDebug($"Sending preview message to {queueUrl}");
            await _sqsClient.SendMessageAsync(sendRequest);
        }
    }

    [LambdaFunction(Name = nameof(ReleaseBuildHandler))]
    public async Task ReleaseBuildHandler(CloudWatchEvent<ReleaseMessage> evnt, ILambdaContext context) =>
        await AWSLambdaWrapper.Trace(_tracerProvider, InstrumentedReleaseBuildHandler, evnt, context);

    public async Task InstrumentedReleaseBuildHandler(CloudWatchEvent<ReleaseMessage> evnt, ILambdaContext context)
    {
        context.Logger.LogInformation($"Preparing release build for {evnt.Detail.Services.Count} services");

        // Make sure all approvals have been given for the service being released
        await _orchestratorManager.VerifyBuildApprovalAsync(evnt.Detail.BuildId);

        var releaseMessage = evnt.Detail;
        var message = JsonSerializer.Serialize(releaseMessage);

        var queueUrls = await _orchestratorManager.GetBuildQueuesAsync(BuildType.ReleaseBuild);
        foreach (var queueUrl in queueUrls)
        {
            var sendRequest = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = message,
                MessageGroupId = Constants.RELEASE_MESSAGE_GROUP_ID,

                // Use attributes so processor can decide what action to take without having to parse message body
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {Constants.BUILD_TYPE_MESSAGE_ATTRIBUTE_KEY, new MessageAttributeValue{StringValue = BuildType.ReleaseBuild.ToString(), DataType = "String"} },
                    {Constants.BUILD_ID_MESSAGE_ATTRIBUTE_KEY, new MessageAttributeValue{StringValue = releaseMessage.BuildId, DataType = "String"} }
                }
            };

            await _sqsClient.SendMessageAsync(sendRequest);
        }
    }
}