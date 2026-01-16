using ClarityDQ.Core.Entities;

namespace ClarityDQ.RuleEngine;

public interface IRuleExecutor
{
    Task<RuleExecutionResult> ExecuteAsync(Rule rule, IRuleDataSource dataSource, CancellationToken cancellationToken = default);
}

public interface IRuleDataSource
{
    Task<RuleDataSourceResult> GetDataAsync(string workspaceId, string datasetName, string tableName, string? columnName = null, CancellationToken cancellationToken = default);
}

public class RuleExecutionResult
{
    public long RecordsChecked { get; set; }
    public long RecordsPassed { get; set; }
    public long RecordsFailed { get; set; }
    public double SuccessRate { get; set; }
    public List<RuleViolation> Violations { get; set; } = new();
    public Dictionary<string, object> Metrics { get; set; } = new();
}

public class RuleDataSourceResult
{
    public long TotalRecords { get; set; }
    public IEnumerable<Dictionary<string, object?>> Rows { get; set; } = new List<Dictionary<string, object?>>();
    public Dictionary<string, Type> Schema { get; set; } = new();
}

public class RuleViolation
{
    public int RowIndex { get; set; }
    public Dictionary<string, object?> RowData { get; set; } = new();
    public string ViolationMessage { get; set; } = string.Empty;
}
