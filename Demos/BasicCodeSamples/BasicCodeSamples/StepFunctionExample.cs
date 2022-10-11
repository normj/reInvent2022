using System.Text.Json;

using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;

namespace BasicCodeSamples
{
    public class StepFunctionExample
    {
        IAmazonStepFunctions _stepFunctionsClient;
        private string _stateMachineArn;

        public StepFunctionExample(IAmazonStepFunctions stepFunctionsClient, string stateMachineArn)
        {
            _stepFunctionsClient = stepFunctionsClient;
            _stateMachineArn = stateMachineArn;
        }

        public async Task InitiateOrderWorkflowAsync(ProductOrder order)
        {
            // Serialize .NET type to JSON
            var state = JsonSerializer.Serialize(order);

            var startRequest = new StartExecutionRequest
            {
                StateMachineArn = _stateMachineArn,
                Input = state,
            
                // Optionally set the name of the execution. Since our system use unique order id
                // we can use the id for the name to make it easier link execution to order.
                Name = order.Id,
            };

            await _stepFunctionsClient.StartExecutionAsync(startRequest);
        }


        public class ProductOrder
        {
            public string Id { get; set; } = Guid.NewGuid().ToString();
            public DateTimeOffset CreateDate { get; set; } = DateTimeOffset.UtcNow;
        }
    }



}
