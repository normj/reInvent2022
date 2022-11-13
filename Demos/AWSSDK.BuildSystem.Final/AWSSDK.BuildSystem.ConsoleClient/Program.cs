using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;
using Amazon.SQS.Model;

using AWSSDK.BuildSystem.Common;
using SQSEncryption.Common;
using System.Text.Json;

string kmsKeyArn = "arn:aws:kms:us-west-2:626492997873:key/7a739407-ac6d-4483-9f2c-74dc25ebde74";
var queueUrl = "https://sqs.us-west-2.amazonaws.com/626492997873/SdkBuild.fifo";
var bucketName = "normj-sdkbuild";
var objectKey = "dynamodb.zip";


var sqsClient = new AmazonSQSClient(RegionEndpoint.USWest2);

// Create a preview message pointing to a service model to generate
var s3Client = new AmazonS3Client(RegionEndpoint.USWest2);

var modelUrl = s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
{
    BucketName = bucketName,
    Key = objectKey,
    Expires = DateTime.UtcNow.AddHours(5)
});

var messageEncryption = new MessageEncryption(kmsKeyArn);

for (int i = 0; i < 1; i++)
{
    var previewMessage = new PreviewMessage(serviceName: "DynamoDB", modelUrl: modelUrl);
    var message = JsonSerializer.Serialize(previewMessage);

    var sendRequest = new SendMessageRequest
    {
        QueueUrl = queueUrl,
        MessageBody = messageEncryption.Encrypt(message),
        MessageGroupId = "DynamoDB",

        // Use attributes so processor can decide what action to take without having to parse message body
        MessageAttributes = new Dictionary<string, MessageAttributeValue>
        {
            {Constants.BUILD_TYPE_MESSAGE_ATTRIBUTE_KEY, new MessageAttributeValue{StringValue = BuildType.PreviewBuild.ToString(), DataType = "String"} }
        }
    };

    await sqsClient.SendMessageAsync(sendRequest);
}
