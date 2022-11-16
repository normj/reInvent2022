using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using AWSSDK.BuildSystem.Common;
using SQSEncryption.Common;
using System.Text.Json;

namespace AWSSDK.BuildSystem.ConsoleClient
{
    public class BuildMessagePublisher
    {
        string _queueUrl = "https://sqs.us-west-2.amazonaws.com/626492997873/SdkBuild";
        private IAmazonSQS _sqsClient;
        private AmazonS3Client _s3Client;

        public BuildMessagePublisher(RegionEndpoint region)
        {
            _sqsClient = new AmazonSQSClient(region);
            _s3Client = new AmazonS3Client(region);
        }

        public async Task SendPreviewBuildMessage(PreviewMessage previewMessage)
        {
            var message = JsonSerializer.Serialize(previewMessage);

            var sendRequest = new SendMessageRequest
            {
                QueueUrl = _queueUrl,
                MessageBody = message,

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
