namespace ClarityDQ.Core.Entities;

public enum LineageNodeType
{
    Table = 0,
    Column = 1,
    View = 2,
    Procedure = 3,
    Report = 4,
    Pipeline = 5
}

public class LineageNode
{
    public Guid Id { get; set; }
    public required string WorkspaceId { get; set; }
    public required string NodeName { get; set; }
    public LineageNodeType NodeType { get; set; }
    public string? DatasetName { get; set; }
    public string? TableName { get; set; }
    public string? ColumnName { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
    public DateTime DiscoveredAt { get; set; }
    public DateTime LastSeenAt { get; set; }
    public string? CreatedBy { get; set; }
}

public class LineageEdge
{
    public Guid Id { get; set; }
    public required Guid SourceNodeId { get; set; }
    public required Guid TargetNodeId { get; set; }
    public required string TransformationType { get; set; }
    public string? TransformationLogic { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }

    public LineageNode? SourceNode { get; set; }
    public LineageNode? TargetNode { get; set; }
}
