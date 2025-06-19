using Amazon.Util.Internal;
using TRE_API.Models;
using Zeebe.Client;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;
using System.Text.Json;



namespace TRE_API.Services
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
