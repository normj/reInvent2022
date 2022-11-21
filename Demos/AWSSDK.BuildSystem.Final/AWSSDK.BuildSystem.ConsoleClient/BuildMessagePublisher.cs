using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using AWSSDK.BuildSystem.Common;
using System.Text.Json;

namespace AWSSDK.BuildSystem.ConsoleClient
{
    public class BuildMessagePublisher
    {
        string _queueUrl = "https://sqs.us-west-1.amazonaws.com/626492997873/SdkBuild.fifo";
        private IAmazonSQS _sqsClient;

        public BuildMessagePublisher(RegionEndpoint region)
        {
            _sqsClient = new AmazonSQSClient(region);
        }

        public async Task SendPreviewBuildMessage(PreviewMessage previewMessage)
        {
            var message = JsonSerializer.Serialize(previewMessage);

            var sendRequest = new SendMessageRequest
            {
                QueueUrl = _queueUrl,
                MessageBody = message,
                MessageGroupId = previewMessage.ServiceName,

                // Use attributes so processor can decide what action to take without having to parse message body
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {Constants.BUILD_TYPE_MESSAGE_ATTRIBUTE_KEY, new MessageAttributeValue{StringValue = BuildType.PreviewBuild.ToString(), DataType = "String"} },
                    {Constants.BUILD_ID_MESSAGE_ATTRIBUTE_KEY, new MessageAttributeValue{StringValue = previewMessage.BuildId, DataType = "String"} }
                }
            };

            await _sqsClient.SendMessageAsync(sendRequest);
        }

        public async Task SendReleaseBuildMessage(ReleaseMessage releaseMessage)
        {
            var message = JsonSerializer.Serialize(releaseMessage);

            var sendRequest = new SendMessageRequest
            {
                QueueUrl = _queueUrl,
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
