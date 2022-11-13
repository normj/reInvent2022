using System.Diagnostics;
using Amazon.Lambda.Core;

using AWSSDK.BuildSystem.Common;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;
using OpenTelemetry.Contrib.Instrumentation.AWSLambda.Implementation;
using OpenTelemetry.Trace;
using OpenTelemetry;
using OpenTelemetry.Contrib.Extensions.AWSXRay.Trace;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWSSDK.BuildSystem.LambdaClient;

public class Functions
{
    TracerProvider _traceProvider;

    /// <summary>
    /// Default constructor that Lambda will invoke.
    /// </summary>
    public Functions()
    {
        _traceProvider = Sdk.CreateTracerProviderBuilder()
            .AddXRayTraceId()
            .AddSource("Amazon.AWS.AWSClientInstrumentation")
            .AddAWSInstrumentation()
            .AddAWSLambdaConfigurations()
            .AddOtlpExporter()
            .AddConsoleExporter()
            .Build();

        Sdk.SetDefaultTextMapPropagator(new AWSXRayPropagator());
    }

    public Task FunctionHandler(ILambdaContext context)
        => AWSLambdaWrapper.Trace(_traceProvider, InstrumentedFunctionHandler, context);

    public async Task InstrumentedFunctionHandler(ILambdaContext context)
    {
        var queueUrl = "https://sqs.us-west-2.amazonaws.com/626492997873/SdkBuild";
        var bucketName = "normj-sdkbuild";
        var objectKey = "dynamodb.zip";

        var sqsClient = new AmazonSQSClient();

        // Create a preview message pointing to a service model to generate
        var s3Client = new AmazonS3Client();

        var modelUrl = s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = objectKey,
            Expires = DateTime.UtcNow.AddHours(5)
        });

        var previewMessage = new PreviewMessage(serviceName: "DynamoDB", modelUrl: modelUrl);
        var message = JsonSerializer.Serialize(previewMessage);

        var sendRequest = new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = message
        };

        // Use attributes so processor can decide what action to take without having to parse message body
        sendRequest.MessageAttributes[Constants.BUILD_TYPE_MESSAGE_ATTRIBUTE_KEY] = new MessageAttributeValue { StringValue = BuildType.PreviewBuild.ToString(), DataType = "String" };

        if(Activity.Current != null)
        {
            context.Logger.LogInformation("Adding tracing information");
            sendRequest.MessageAttributes[Constants.TRACE_ID_MESSAGE_ATTRIBUTE_KEY] = new MessageAttributeValue { StringValue = Activity.Current.Context.TraceId.ToHexString(), DataType = "String" };
            sendRequest.MessageAttributes[Constants.SPAN_ID_MESSAGE_ATTRIBUTE_KEY] = new MessageAttributeValue { StringValue = Activity.Current.Context.SpanId.ToHexString(), DataType = "String" };

            if(!string.IsNullOrEmpty(Activity.Current.Context.TraceState))
                sendRequest.MessageAttributes[Constants.TRACE_STATE_MESSAGE_ATTRIBUTE_KEY] = new MessageAttributeValue { StringValue = Activity.Current.Context.TraceState, DataType = "String" };
        }

        await sqsClient.SendMessageAsync(sendRequest);
    }
}