using Amazon.Lambda.Annotations;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using SQSEncryption.Common;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SQSEncryption.Lambda.Processor;

public class Functions
{
    IMessageEncryption _messageEncryption;
    public Functions(IMessageEncryption messageEncryption)
    {
        _messageEncryption = messageEncryption;
    }


    [LambdaFunction(Name = "SQSProcessor", MemorySize = 256, Timeout = 30)]
    public SQSBatchResponse Processor(SQSEvent evnt, ILambdaContext context)
    {
        var response = new SQSBatchResponse();

        foreach (var record in evnt.Records)
        {
            try
            {
                var encryptedMessage = record.Body;
                var decryptedMessage = _messageEncryption.Decrypt(encryptedMessage);

                // Log messages for demo purposes. In a real world scenario if we are encrypting messages
                // then we don't want to be logging decrypted messages.
                context.Logger.LogInformation($"Encrypted message: {encryptedMessage}");
                context.Logger.LogInformation($"Decrypted message: {decryptedMessage}");

                ProcessMessage(decryptedMessage);
            }
            catch(Exception e)
            {
                response.BatchItemFailures.Add(new SQSBatchResponse.BatchItemFailure
                {
                    ItemIdentifier = record.MessageId
                });

                context.Logger.LogError($"Failed to process message {record.MessageId}: {e.Message}");
                context.Logger.LogError(e.StackTrace);
            }
        }

        return response;
    }


    private void ProcessMessage(string message)
    {
        // TODO: Add some important business logic
    }
}