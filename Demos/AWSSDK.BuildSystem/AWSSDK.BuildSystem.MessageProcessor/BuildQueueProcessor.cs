using Amazon.SQS;
using Amazon.SQS.Model;
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

namespace AWSSDK.BuildSystem.MessageProcessor
{
    public class BuildQueueProcessor : BackgroundService
    {
        protected const int VISIBLITY_TIMEOUT = 60;

        private readonly IAmazonSQS _sqsClient;
        private readonly ILogger _logger;
        private readonly AppSettings _appSettings;


        public BuildQueueProcessor(
            IAmazonSQS sqsClient,
            ILogger<BuildQueueProcessor> logger,
            IOptions<AppSettings> appSettings)
        {
            _sqsClient = sqsClient;
            _logger = logger;
            _appSettings = appSettings.Value;
        }


        protected override async Task ExecuteAsync(CancellationToken token)
        {
            var readRequest = new ReceiveMessageRequest
            {
                QueueUrl = _appSettings.QueueUrl,
                VisibilityTimeout = VISIBLITY_TIMEOUT,
                WaitTimeSeconds = 20,
                MaxNumberOfMessages = 1,
                MessageAttributeNames = new List<string> { ".*" }
            };

            _logger.LogInformation($"Starting to watch Queue {_appSettings.QueueUrl}");

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
                                            QueueUrl = _appSettings.QueueUrl,
                                            ReceiptHandle = message.ReceiptHandle,
                                            VisibilityTimeout = VISIBLITY_TIMEOUT
                                        };
                                        await _sqsClient.ChangeMessageVisibilityAsync(request);
                                    }
                                }

                                if (processMessageTask.GetAwaiter().GetResult())
                                {
                                    await _sqsClient.DeleteMessageAsync(_appSettings.QueueUrl, message.ReceiptHandle);
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

            switch (buildType)
            {
                case BuildType.PreviewBuild:
                    await PreviewBuildAsync(messageBody);
                    break;
                case BuildType.ReleaseBuild:
                    await ReleaseAsync(messageBody);
                    break;

            }

            return true;
        }

        private Task PreviewBuildAsync(string messageBody)
        {
            var previewMessage = JsonSerializer.Deserialize<PreviewMessage>(messageBody);
            _logger.LogInformation($"Running preview build for {previewMessage.ServiceName}");
            return Task.CompletedTask;
        }

        private Task ReleaseAsync(string messageBody)
        {
            _logger.LogInformation("Running release build");
            return Task.CompletedTask;
        }
    }
}
