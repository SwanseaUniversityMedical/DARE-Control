using static Google.Apis.Requests.BatchRequest;
using System.Text.Json;
using Tre_Camunda.Models;
using Zeebe.Client;

namespace Tre_Camunda.Services
{
    public class ZeebeDmnService : IZeebeDmnService
    {
        private readonly IZeebeClient _zeebeClient;

        public ZeebeDmnService(IZeebeClient zeebeClient)
        {
            _zeebeClient = zeebeClient;
        }

        //Generic for all inputs including json
        public async Task<DmnResponse> EvaluateDecisionModelAsync(DmnRequest input)
        {
            var zeebeClient = ZeebeClient.Builder()
            .UseGatewayAddress("127.0.0.1:26500") 
            .UsePlainText()                     
            .Build();

            var topology = await zeebeClient.TopologyRequest().Send();
            Console.WriteLine($"Connected to cluster with version");
            var json = JsonSerializer.Serialize(input.Variables);

            var result = await _zeebeClient.NewEvaluateDecisionCommand()
                .DecisionId(input.DecisionId)
                .Variables(json)
                .Send();

            var decisionResult = result.DecisionOutput;
            Dictionary<string, object> outputDict;

            var jsonDoc = JsonDocument.Parse(decisionResult);
            var root = jsonDoc.RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                outputDict = JsonSerializer.Deserialize<Dictionary<string, object>>(decisionResult);
            }
            else
            {
                outputDict = new Dictionary<string, object>
                {
                    { "result", root.GetString() }
                };
            }

            return new DmnResponse
            {
                DecisionId = input.DecisionId,
                Result = outputDict
            };


        }
    }
}
