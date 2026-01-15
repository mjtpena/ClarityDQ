namespace ClarityDQ.Core.Entities;

public class Rule
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RuleType Type { get; set; }
    public string WorkspaceId { get; set; } = string.Empty;
    public string DatasetName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string ColumnName { get; set; } = string.Empty;
    public string Expression { get; set; } = string.Empty;
    public double Threshold { get; set; }
    public RuleSeverity Severity { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public enum RuleType
{
    Completeness,
    Accuracy,
    Consistency,
    Uniqueness,
    Validity,
    Custom
}

public enum RuleSeverity
{
    Low,
    Medium,
    High,
    Critical
}

public class RuleExecution
{
    public Guid Id { get; set; }
    public Guid RuleId { get; set; }
    public Rule? Rule { get; set; }
    public DateTime ExecutedAt { get; set; }
    public RuleExecutionStatus Status { get; set; }
    public long RecordsChecked { get; set; }
    public long RecordsPassed { get; set; }
    public long RecordsFailed { get; set; }
    public double SuccessRate { get; set; }
    public string? ResultDetails { get; set; }
    public string? ErrorMessage { get; set; }
    public int DurationMs { get; set; }
}

public enum RuleExecutionStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Skipped
}
