using Amazon.Lambda.Annotations;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
using OpenTelemetry;
using Org.BouncyCastle.Asn1.Mozilla;
using SQSEncryption.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTelemetry.Contrib.Extensions.AWSXRay.Trace;

namespace AWSSDK.Orchestrator.EventBus
{
    [LambdaStartup]
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var traceProvider = Sdk.CreateTracerProviderBuilder()
                .AddXRayTraceId()
                .AddSource(Telemetry.ServiceName)
                .AddAWSInstrumentation()
                .AddAWSLambdaConfigurations()
                .AddOtlpExporter()
                .Build();
            Sdk.SetDefaultTextMapPropagator(new AWSXRayPropagator());

            services.AddSingleton<TracerProvider>(traceProvider);

            services.AddAWSService<Amazon.SQS.IAmazonSQS>();
            services.AddAWSService<Amazon.DynamoDBv2.IAmazonDynamoDB>();
            services.AddSingleton<OrchestratorManager, OrchestratorManager>();
        }
    }
}
