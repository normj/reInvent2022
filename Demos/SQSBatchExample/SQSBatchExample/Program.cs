using Actions_Compile;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using SQSBatchExample;
using SQSBatchExample.Common;

var queueUrl = "https://sqs.us-west-2.amazonaws.com/626492997873/ClientSideEncryptionExample";
string kmsKeyArn = "arn:aws:kms:us-west-2:626492997873:key/7a739407-ac6d-4483-9f2c-74dc25ebde74";

// Application utility stimulating getting application messages. 
var messageReader = new MessageReader();

// Utility for encrypting messages
var messageEncryption = new MessageEncryption(kmsKeyArn);


var client = new AmazonSQSClient(RegionEndpoint.USWest2);


var sendBatchRequest = new SendMessageBatchRequest
{
    QueueUrl = queueUrl,
};

foreach(var messageBody in messageReader.Read())
{
    sendBatchRequest.Entries.Add(new SendMessageBatchRequestEntry
    {
        Id = sendBatchRequest.Entries.Count.ToString(),
        MessageBody = messageEncryption.Encrypt(messageBody)
    });

    if(sendBatchRequest.Entries.Count == 10)
    {
        var response = await client.SendMessageBatchAsync(sendBatchRequest);
        if(response.Failed.Count > 0)
        {
            Console.Error.WriteLine($"There was an error processing {response.Failed.Count} messages");
            foreach(var failure in response.Failed)
            {
                var failedMessage = sendBatchRequest.Entries.First(x => string.Equals(x.Id, failure.Id, StringComparison.InvariantCulture));
                Console.Error.WriteLine($"Error message for following message: {failure.Message}");
                Console.Error.WriteLine(failedMessage.MessageBody);
            }
        }

        sendBatchRequest.Entries.Clear();
    }
}