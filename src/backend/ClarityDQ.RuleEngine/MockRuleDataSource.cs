using ClarityDQ.Core.Entities;

namespace ClarityDQ.RuleEngine;

public class MockRuleDataSource : IRuleDataSource
{
    public Task<RuleDataSourceResult> GetDataAsync(string workspaceId, string datasetName, string tableName, string? columnName = null, CancellationToken cancellationToken = default)
    {
        var result = new RuleDataSourceResult
        {
            TotalRecords = 100,
            Schema = new Dictionary<string, Type>
            {
                ["Id"] = typeof(int),
                ["Name"] = typeof(string),
                ["Email"] = typeof(string),
                ["Age"] = typeof(int),
                ["Score"] = typeof(double),
                ["Status"] = typeof(string)
            }
        };

        var rows = new List<Dictionary<string, object?>>();
        var random = new Random(42);

        for (int i = 1; i <= 100; i++)
        {
            rows.Add(new Dictionary<string, object?>
            {
                ["Id"] = i,
                ["Name"] = i % 10 == 0 ? null : $"User{i}",
                ["Email"] = i % 15 == 0 ? null : $"user{i}@example.com",
                ["Age"] = random.Next(18, 80),
                ["Score"] = random.NextDouble() * 100,
                ["Status"] = i % 5 == 0 ? "Active" : "Inactive"
            });
        }

        result.Rows = rows;
        return Task.FromResult(result);
    }
}
