using Amazon;
using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using AWSSDK.BuildSystem.Common;
using System.Text.Json;

var eventBridgeClient = new AmazonEventBridgeClient(RegionEndpoint.USWest1);

var services = new string[] { "S3", "DynamoDB", "SQS", "SNS", "EC2", "ECS", "AppRunner", "Beanstalk", "RDS", "Lambda", "EKS" };

var random = new Random();

int counter = 0;

while(true)
{
    var buildId = (counter++).ToString();
    

    PutEventsRequest putEventRequest;
    if (counter % 10 == 0)
    {
        var releaseMessage = new ReleaseMessage(buildId);
        releaseMessage.Services.Add(new ServiceModel("S3", "http://foobar/foo"));
        releaseMessage.Services.Add(new ServiceModel("DynamoDB", "http://foobar/foo"));

        putEventRequest = new PutEventsRequest
        {
            Entries = new List<PutEventsRequestEntry>
             {
                 new PutEventsRequestEntry
                 {
                     DetailType = "ReleaseBuild",
                     Detail = JsonSerializer.Serialize(releaseMessage),
                     EventBusName = "aws-sdk-builds-bus",
                     Source = "SDK.Orchestrator"
                 }
             }
        };
    }
    else
    {
        var previewMessage = new PreviewMessage(buildId, services[random.Next(0, services.Length)], "http://foobar/foo");

        putEventRequest = new PutEventsRequest
        {
            Entries = new List<PutEventsRequestEntry>
             {
                 new PutEventsRequestEntry
                 {
                     DetailType = "PreviewBuild",
                     Detail = JsonSerializer.Serialize(previewMessage),
                     EventBusName = "aws-sdk-builds-bus",
                     Source = "SDK.Orchestrator"
                 }
             }
        };
    }

    if(buildId == "99999")
    {
        counter = 0;
    }



    await eventBridgeClient.PutEventsAsync(putEventRequest);

    await Task.Delay(TimeSpan.FromSeconds(10));
}


