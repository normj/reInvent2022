using Amazon.S3;
using Amazon.S3.Model;
using AWSSDK.BuildSystem.Common;
using AWSSDK.BuildSystem.ConsoleClient;
using System.Collections.Generic;
using System;

var region = Amazon.RegionEndpoint.USWest2;
var s3Client = new AmazonS3Client(region);
var buildMessagePublisher = new BuildMessagePublisher(region);


var modelUrl = s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
{
    BucketName = "normj-sdkbuild",
    Key = "dynamodb.zip",
    Expires = DateTime.UtcNow.AddHours(5)
});

while (true)
{
    Console.WriteLine("Select Build Message Type");
    Console.WriteLine("-------------------------");
    Console.WriteLine("1: Preview Message");
    Console.WriteLine("2: Release Message");

    var input = Console.ReadLine()?.Trim();
    if (input == "1")
    {
        var previewMessage = new PreviewMessage(buildId: Guid.NewGuid().ToString(), serviceName: "DynamoDB", modelUrl: modelUrl);
        await buildMessagePublisher.SendPreviewBuildMessage(previewMessage);
        Console.WriteLine("Preview build message sent");
        break;
    }
    else if (input == "2")
    {
        var services = new List<ServiceModel>
        {
            new ServiceModel("DynamoDB", modelUrl),
        };
        var releaseMessage = new ReleaseMessage(buildId: Guid.NewGuid().ToString(), services);
        await buildMessagePublisher.SendReleaseBuildMessage(releaseMessage);
        Console.WriteLine("Release build message sent");
        break;
    }
    else
    {
        Console.Error.WriteLine("Invalid option: " + input);
    }
}