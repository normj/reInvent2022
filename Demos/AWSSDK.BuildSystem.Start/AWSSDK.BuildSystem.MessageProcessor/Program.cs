using AWSSDK.BuildSystem.MessageProcessor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


await Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.AddConsole();
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddOptions<AppSettings>().Bind(hostContext.Configuration.GetSection("AppSettings"));

        services.AddAWSService<Amazon.SQS.IAmazonSQS>();
        services.AddAWSService<Amazon.SimpleNotificationService.IAmazonSimpleNotificationService>();
        services.AddHostedService<BuildQueueProcessor>();
    })
    .Build()
    .RunAsync();