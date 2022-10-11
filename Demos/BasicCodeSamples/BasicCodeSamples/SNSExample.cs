using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace BasicCodeSamples;

public class SNSProductOrder
{
    private string _topicArn;
    private IAmazonSimpleNotificationService _snsClient;

    public SNSProductOrder(IAmazonSimpleNotificationService snsClient, string topicArn)
    {
        _snsClient = snsClient;
        _topicArn = topicArn;
    }

    public async Task SendAlertAsync(AlertMessage alert)
    {
        // Serialize .NET type to JSON
        var message = JsonSerializer.Serialize(alert);

        var publishRequest = new PublishRequest
        {
            TargetArn = _topicArn,
            Message = message,
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                // Add message type to allow SNS to route message to correct subscribers
                {"MessageType", new MessageAttributeValue{DataType = "String", StringValue = alert.AlertType } }
            }
        };

        await _snsClient.PublishAsync(publishRequest);
    }

    public class AlertMessage
    {
        public int Id { get; set; }
        public DateTimeOffset CreateDate { get; set; }

        public string AlertType { get; set; }
    }

}

