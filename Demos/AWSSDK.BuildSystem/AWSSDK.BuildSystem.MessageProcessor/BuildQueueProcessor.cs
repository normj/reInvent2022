using Amazon.SQS;
using Amazon.SQS.Model;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using AWSSDK.BuildSystem.Common;
using static AWSSDK.BuildSystem.Common.Constants;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using AWSSDK.BuildSystem.MessageProcessor.BuildTasks;

namespace AWSSDK.BuildSystem.MessageProcessor
{
    public class BuildQueueProcessor : BackgroundService
    {
        protected const int VISIBLITY_TIMEOUT = 60;

        private readonly IAmazonSQS _sqsClient;
        private readonly IAmazonSimpleNotificationService _snsClient;
        private readonly ILogger _logger;
        private readonly AppSettings _appSettings;


        public BuildQueueProcessor(
            IAmazonSQS sqsClient,
            IAmazonSimpleNotificationService snsClient
            ILogger<BuildQueueProcessor> logger,
            IOptions<AppSettings> appSettings)
        {
            _sqsClient = sqsClient;
            _snsClient = snsClient;
            _logger = logger;
            _appSettings = appSettings.Value;
        }


        protected override async Task ExecuteAsync(CancellationToken token)
        {
            var readRequest = new ReceiveMessageRequest
            {
                QueueUrl = _appSettings.BuildMessagesQueueUrl,
                VisibilityTimeout = VISIBLITY_TIMEOUT,
                WaitTimeSeconds = 20,
                MaxNumberOfMessages = 1,
                MessageAttributeNames = new List<string> { ".*" }
            };

            _logger.LogInformation($"Starting to watch Queue {_appSettings.BuildMessagesQueueUrl}");

            while (!token.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Pinging SQS for new messages.");
                    var response = await _sqsClient.ReceiveMessageAsync(readRequest, token);
                    _logger.LogDebug("SQS response" + response.Messages.Count);
                    if (response.Messages.Count == 0)
                    {
                        _logger.LogDebug($"No messages received queue.");
                    }
                    else
                    {
                        _logger.LogDebug("Processing Queue messages");
                        foreach (var message in response.Messages)
                        {
                            _logger.LogInformation("Message Attribute Count: " + message.MessageAttributes.Count);
                            ActivityContext parentContext;
                            //if(message.MessageAttributes.ContainsKey(Constants.TRACE_ID_MESSAGE_ATTRIBUTE_KEY))
                            //{
                            //    _logger.LogInformation("Found parent trace");
                            //    var traceId = message.MessageAttributes[Constants.TRACE_ID_MESSAGE_ATTRIBUTE_KEY].StringValue;
                            //    var spanId = message.MessageAttributes[Constants.SPAN_ID_MESSAGE_ATTRIBUTE_KEY].StringValue;

                            //    string traceState = null;
                            //    if(message.MessageAttributes.ContainsKey(Constants.TRACE_STATE_MESSAGE_ATTRIBUTE_KEY))
                            //    {
                            //        traceState = message.MessageAttributes[Constants.TRACE_STATE_MESSAGE_ATTRIBUTE_KEY].StringValue;
                            //    }

                            //    parentContext = new ActivityContext(ActivityTraceId.CreateFromString(traceId), ActivitySpanId.CreateFromString(spanId), ActivityTraceFlags.Recorded, traceState, true);
                            //}
                            using var activity = Telemetry.RootActivitySource.StartActivity("Reading Message", ActivityKind.Consumer, parentContext);
                            if (activity == null)
                            {
                                _logger.LogInformation("Failed to start activity");
                                continue;
                            }
                            activity.AddBaggage("MessageId", message.MessageId);

                            using (_logger.BeginScope($"Message: {message.MessageId}"))
                            {
                                var logData = new
                                {
                                    Message = message,
                                    MessageId = message.MessageId,
                                    MessageBody = JsonSerializer.Serialize(message.Body)
                                };
                                _logger.LogInformation($"Processing message {message.MessageId} from queue");

                                var processMessageTask = ProcessMessageAsync(message);

                                // Monitor processing task and if it is taken a long time to process
                                // update the message visiblity to avoid it getting processed again.
                                while (!processMessageTask.IsCompleted)
                                {
                                    await Task.WhenAny(processMessageTask, Task.Delay(TimeSpan.FromSeconds(30)));

                                    if (!processMessageTask.IsCompleted)
                                    {
                                        var request = new ChangeMessageVisibilityRequest
                                        {
                                            QueueUrl = _appSettings.BuildMessagesQueueUrl,
                                            ReceiptHandle = message.ReceiptHandle,
                                            VisibilityTimeout = VISIBLITY_TIMEOUT
                                        };
                                        await _sqsClient.ChangeMessageVisibilityAsync(request);
                                    }
                                }

                                if (processMessageTask.GetAwaiter().GetResult())
                                {
                                    await _sqsClient.DeleteMessageAsync(_appSettings.BuildMessagesQueueUrl, message.ReceiptHandle);
                                }
                            }
                        }
                    }
                }
                catch (TaskCanceledException) when (token.IsCancellationRequested)
                {
                    _logger.LogError($"Stop polling messages because processor has been request to cancelled.");
                }
                catch (Exception e)
                {
                    _logger.LogError($"Unknown error while processing queue: {e}");

                    // Add a delay before continuing processing message in case there was a transient networking issue.
                    await Task.Delay(1000, token);
                }
            }
        }


        protected async Task<bool> ProcessMessageAsync(Message message)
        {
            // Get task 
            var buildTypeStr = message.MessageAttributes[BUILD_TYPE_MESSAGE_ATTRIBUTE_KEY].StringValue;
            if(string.IsNullOrEmpty(buildTypeStr))
            {
                throw new ApplicationException($"Message attribute {BUILD_TYPE_MESSAGE_ATTRIBUTE_KEY} not set");
            }

            if(!Enum.TryParse<BuildType>(buildTypeStr, out var buildType))
            {
                throw new ApplicationException($"Unknown build type: {buildTypeStr}");
            }

            var messageBody = message.Body;

            try
            {
                switch (buildType)
                {
                    case BuildType.PreviewBuild:
                        await PreviewBuildAsync(messageBody);
                        break;
                    case BuildType.ReleaseBuild:
                        await ReleaseAsync(messageBody);
                        break;

                }

            }
            catch(Exception e)
            {
                var snsRequest = new PublishRequest
                {
                    TopicArn = _appSettings.BuildStatusTopicArn,
                    Message = 
                }
                return false;
            }

            return true;
        }

        private async Task PreviewBuildAsync(string messageBody)
        {
            using var activity = Telemetry.RootActivitySource.StartActivity("Starting preview build");

            var previewMessage = JsonSerializer.Deserialize<PreviewMessage>(messageBody);
            activity.AddTag("AWSServiceName", previewMessage.ServiceName);

            _logger.LogInformation($"Running preview build for {previewMessage.ServiceName}");

            var command = new PreviewBuildCommand(previewMessage);
            await command.ExecuteAsync();
        }

        private async Task ReleaseAsync(string messageBody)
        {
            using var activity = Telemetry.RootActivitySource.StartActivity("Starting release build");

            var releaseMessage = JsonSerializer.Deserialize<ReleaseMessage>(messageBody);


            _logger.LogInformation("Running release build");

            var command = new ReleaseBuildCommand(releaseMessage);
            await command.ExecuteAsync();
        }
    }
}
