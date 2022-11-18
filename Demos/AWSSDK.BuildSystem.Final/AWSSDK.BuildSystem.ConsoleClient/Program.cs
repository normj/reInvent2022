    using Amazon.S3;
using Amazon.S3.Model;
using AWSSDK.BuildSystem.Common;
using AWSSDK.BuildSystem.ConsoleClient;

var region = Amazon.RegionEndpoint.USWest1;
var s3Client = new AmazonS3Client(region);
var buildMessagePublisher = new BuildMessagePublisher(region);


var modelUrl = s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
{
    BucketName = "normj-sdkbuild",
    Key = "dynamodb.zip",
    Expires = DateTime.UtcNow.AddHours(5)
});

var previewMessage = new PreviewMessage(buildId: Guid.NewGuid().ToString(), serviceName: "DynamoDB", modelUrl: modelUrl);
await buildMessagePublisher.SendPreviewBuildMessage(previewMessage);

//var services = new List<ServiceModel>
//{
//    new ServiceModel("DynamoDB", modelUrl),
//};
//var releaseMessage = new ReleaseMessage(buildId: Guid.NewGuid().ToString(), services);
//await buildMessagePublisher.SendReleaseBuildMessage(releaseMessage);