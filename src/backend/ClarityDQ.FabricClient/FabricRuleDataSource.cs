using ClarityDQ.RuleEngine;
using System.Data;
using System.Text.Json;

namespace ClarityDQ.FabricClient;

public class FabricRuleDataSource : IRuleDataSource
{
    private readonly IFabricClient _fabricClient;
    private readonly HttpClient _httpClient;

    public FabricRuleDataSource(IFabricClient fabricClient, HttpClient httpClient)
    {
        _fabricClient = fabricClient;
        _httpClient = httpClient;
    }

    public async Task<RuleDataSourceResult> GetDataAsync(
        string workspaceId,
        string datasetName,
        string tableName,
        string? columnName = null,
        CancellationToken cancellationToken = default)
    {
        // For now, return mock data
        // In production, this would query Fabric's SQL endpoint or OneLake
        var rows = new List<Dictionary<string, object?>>();

        // Generate sample data
        for (int i = 0; i < 100; i++)
        {
            var row = new Dictionary<string, object?>
            {
                ["Id"] = i + 1,
                ["Name"] = i % 10 == 0 ? null : $"Record {i}",
                ["Status"] = i % 3 == 0 ? "Active" : "Inactive",
                ["Value"] = i * 10.5,
                ["CreatedDate"] = DateTime.UtcNow.AddDays(-i)
            };

            rows.Add(row);
        }

        var result = new RuleDataSourceResult
        {
            TotalRecords = 100,
            Rows = rows
        };

        return result;
    }
}
