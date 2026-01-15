namespace ClarityDQ.Core.Entities;

public class DataProfile
{
    public Guid Id { get; set; }
    public string WorkspaceId { get; set; } = string.Empty;
    public string DatasetName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public DateTime ProfiledAt { get; set; }
    public long RowCount { get; set; }
    public int ColumnCount { get; set; }
    public long SizeInBytes { get; set; }
    public string ProfileData { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public ProfileStatus Status { get; set; }
}

public enum ProfileStatus
{
    Pending,
    InProgress,
    Completed,
    Failed
}
