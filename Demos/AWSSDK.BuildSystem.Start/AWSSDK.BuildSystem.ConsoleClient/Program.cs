using AWSSDK.BuildSystem.ConsoleClient;

var buildMessagePublisher = new BuildMessagePublisher(Amazon.RegionEndpoint.USWest2);
await buildMessagePublisher.SendPreviewBuildMessage("DynamoDB", "normj-sdkbuild", "dynamodb.zip");