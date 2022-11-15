using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using AWSSDK.BuildSystem.Common;
using System.Text.Json;

namespace AWSSDK.BuildSystem.ConsoleClient
{
    public class BuildMessagePublisher
    {
        string _kmsKeyArn = "arn:aws:kms:us-west-2:626492997873:key/7a739407-ac6d-4483-9f2c-74dc25ebde74";
        string _queueUrl = "https://sqs.us-west-2.amazonaws.com/626492997873/SdkBuild";
        private IAmazonSQS _sqsClient;
        private AmazonS3Client _s3Client;

        public BuildMessagePublisher(RegionEndpoint region)
        {
            _sqsClient = new AmazonSQSClient(region);
            _s3Client = new AmazonS3Client(region);
        }

        public async Task SendPreviewBuildMessage(string buildId, string service, string bucketName, string objectKey)
        {
            var modelUrl = _s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = objectKey,
                Expires = DateTime.UtcNow.AddHours(5)
            });

            var previewMessage = new PreviewMessage(buildId: buildId, serviceName: service, modelUrl: modelUrl);
            var message = JsonSerializer.Serialize(previewMessage);

            var sendRequest = new SendMessageRequest
            {
                QueueUrl = _queueUrl,
                MessageBody = message,

                // Use attributes so processor can decide what action to take without having to parse message body
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {Constants.BUILD_TYPE_MESSAGE_ATTRIBUTE_KEY, new MessageAttributeValue{StringValue = BuildType.PreviewBuild.ToString(), DataType = "String"} }
                }
            };

            await _sqsClient.SendMessageAsync(sendRequest);
        }

        public Task SendReleaseBuildMessage()
        {
            throw new NotImplementedException();
        }
    }
}
