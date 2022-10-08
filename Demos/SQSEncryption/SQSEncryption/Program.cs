using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;

using SQSEncryption.Common;

var queueUrl = "https://sqs.us-west-2.amazonaws.com/626492997873/ClientSideEncryptionExample";
string kmsKeyArn = "arn:aws:kms:us-west-2:626492997873:key/7a739407-ac6d-4483-9f2c-74dc25ebde74";

var client = new AmazonSQSClient(RegionEndpoint.USWest2);

var messageEncryption = new MessageEncryption(kmsKeyArn);

var message = $"Important business message to be processed: {DateTime.Now}";
var encryptedMessage = messageEncryption.Encrypt(message);

await client.SendMessageAsync(new SendMessageRequest
{
    MessageBody = encryptedMessage,
    QueueUrl = queueUrl
});
