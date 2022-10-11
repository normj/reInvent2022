
using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.StepFunctions;
using BasicCodeSamples;


var topicArn = "arn:aws:sns:us-west-2:626492997873:DemoTopic";

var snsClient = new AmazonSimpleNotificationServiceClient(RegionEndpoint.USWest2);
//await new SNSProductOrder(snsClient, topicArn).SendAlertAsync(new SNSProductOrder.AlertMessage() {Id = 1, CreateDate = DateTimeOffset.UtcNow, AlertType = "RedAlert" });

var stateMachineArn = "arn:aws:states:us-west-2:626492997873:stateMachine:ProductOrderWorkflow";
var stepFunctionsClient = new AmazonStepFunctionsClient(RegionEndpoint.USWest2);
await new StepFunctionExample(stepFunctionsClient, stateMachineArn).InitiateOrderWorkflowAsync(new StepFunctionExample.ProductOrder());