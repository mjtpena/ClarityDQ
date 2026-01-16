namespace ClarityDQ.Core.Entities;

public class Schedule
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ScheduleType Type { get; set; }
    public Guid? RuleId { get; set; }
    public Rule? Rule { get; set; }
    public string? WorkspaceId { get; set; }
    public string? DatasetName { get; set; }
    public string? TableName { get; set; }
    public string CronExpression { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public DateTime? LastRunAt { get; set; }
    public DateTime? NextRunAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public enum ScheduleType
{
    RuleExecution,
    DataProfiling
}

public class ScheduleExecution
{
    public Guid Id { get; set; }
    public Guid ScheduleId { get; set; }
    public Schedule? Schedule { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public ScheduleExecutionStatus Status { get; set; }
    public string? ResultSummary { get; set; }
    public string? ErrorMessage { get; set; }
}

public enum ScheduleExecutionStatus
{
    Running,
    Completed,
    Failed
}
