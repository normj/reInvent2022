using System.Net;
using Amazon.Lambda.Core;
using Amazon.Lambda.CloudWatchEvents;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SDK.Orchestrator.EventBus;

public class Functions
{
    public Functions()
    {
    }

    public async Task PreviewBuildHandler(CloudWatchEvent<PreviewBuild> evnt, ILambdaContext context)
    {
        context.Logger.LogLine("Preparing preview build: " + evnt.Detail.ServiceName);
        await Task.Delay(100);
    }

    public async Task ApprovedReleaseBuildHandler(CloudWatchEvent<ReleaseBuild> evnt, ILambdaContext context)
    {
        context.Logger.LogLine($"Preparing release build for {evnt.Detail.Services.Length} services");
        await Task.Delay(100);
    }
}

public class PreviewBuild
{
    public string ServiceName { get; set; }

    public string ModelUrl { get; set; }
}

public class ReleaseBuild
{
    public string ApprovedBy { get; set; }


    public Service[] Services { get; set; }

    public class Service
    {
        public string ServiceName { get; set; }

        public string ModelUrl { get; set; }
    }
}