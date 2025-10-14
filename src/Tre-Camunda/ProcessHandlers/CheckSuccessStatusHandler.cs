using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Tre_Credentials.DbContexts;
using Tre_Credentials.Models;
using Zeebe.Client.Accelerator.Abstractions;
using Zeebe.Client.Accelerator.Attributes;




namespace Tre_Camunda.ProcessHandlers
{
    [JobType("set-success-status")]
    public class SetSuccessStatusHandler : IAsyncZeebeWorker
    {
        private readonly CredentialsDbContext _credDb;
        private readonly ILogger<SetSuccessStatusHandler> _logger;

        public SetSuccessStatusHandler(CredentialsDbContext credsDb, ILogger<SetSuccessStatusHandler> logger)
        {
            _credDb = credsDb;
            _logger = logger;
        }

        public async Task HandleJob(ZeebeJob job, CancellationToken ct)
        {
            var vars = JsonSerializer.Deserialize<Dictionary<string, object>>(job.Variables) ?? new();

            if (!vars.TryGetValue("parentProcessKey", out var parentObj) || parentObj is null)
            {
                _logger.LogWarning("parentProcessKey missing.");
                return; 
            }

            long parentProcessKey = parentObj is JsonElement el && el.ValueKind == JsonValueKind.Number && el.TryGetInt64(out var parsed)
                ? parsed
                : long.Parse(parentObj.ToString());

            var rows = await _credDb.EphemeralCredentials.Where(e => e.ParentProcessInstanceKey == parentProcessKey).ToListAsync(ct);

            if (rows.Count == 0)
            {
                _logger.LogInformation("No rows found for ParentProcessInstanceKey={Parent}", parentProcessKey);
                return;
            }

            foreach (var r in rows)
                r.SuccessStatus = SuccessStatus.Success;

            await _credDb.SaveChangesAsync(ct);

            _logger.LogInformation("Marked {Count} row(s) Success for ParentProcessInstanceKey={Parent}", rows.Count, parentProcessKey);
        }
    }
}
