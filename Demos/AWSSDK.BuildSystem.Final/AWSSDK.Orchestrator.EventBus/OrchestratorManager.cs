using Amazon.DynamoDBv2;
using Amazon.SQS;
using AWSSDK.BuildSystem.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSSDK.Orchestrator.EventBus
{
    public class OrchestratorManager
    {
        private IAmazonSQS _sqsClient;
        private IAmazonDynamoDB _ddbClient;

        public OrchestratorManager(IAmazonSQS sqsClient, IAmazonDynamoDB ddbClient)
        {
            _sqsClient = sqsClient;
            _ddbClient = ddbClient;
        }

        public Task<List<string>> GetBuildQueuesAsync(BuildType buildType)
        {
            // TODO: Fetch queues
            return Task.FromResult(new List<string> { "https://sqs.us-west-1.amazonaws.com/626492997873/SdkBuild.fifo" });
        }


        public Task VerifyBuildApprovalAsync(string buildId)
        {
            // TODO: Check with orchestrator backend that all approvals have been meet.
            return Task.CompletedTask;
        }
    }
}
