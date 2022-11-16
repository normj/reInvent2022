using Amazon.Lambda.Annotations;
using Microsoft.Extensions.DependencyInjection;
using SQSEncryption.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSSDK.Orchestrator.EventBus
{
    [LambdaStartup]
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAWSService<Amazon.SQS.IAmazonSQS>();
            services.AddAWSService<Amazon.DynamoDBv2.IAmazonDynamoDB>();
            services.AddSingleton<OrchestratorManager, OrchestratorManager>();
        }
    }
}
