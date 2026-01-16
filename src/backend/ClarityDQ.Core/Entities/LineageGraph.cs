namespace ClarityDQ.Core.Entities;

public class LineageGraph
{
    public List<LineageNode> Nodes { get; set; } = new();
    public List<LineageEdge> Edges { get; set; } = new();
    
    public void AddNode(LineageNode node)
    {
        if (!Nodes.Any(n => n.Id == node.Id))
        {
            Nodes.Add(node);
        }
    }
    
    public void AddEdge(LineageEdge edge)
    {
        if (!Edges.Any(e => e.SourceNodeId == edge.SourceNodeId && e.TargetNodeId == edge.TargetNodeId))
        {
            Edges.Add(edge);
        }
    }
    
    public List<LineageNode> GetUpstreamNodes(Guid nodeId)
    {
        var upstreamIds = Edges.Where(e => e.TargetNodeId == nodeId).Select(e => e.SourceNodeId).ToList();
        return Nodes.Where(n => upstreamIds.Contains(n.Id)).ToList();
    }
    
    public List<LineageNode> GetDownstreamNodes(Guid nodeId)
    {
        var downstreamIds = Edges.Where(e => e.SourceNodeId == nodeId).Select(e => e.TargetNodeId).ToList();
        return Nodes.Where(n => downstreamIds.Contains(n.Id)).ToList();
    }
}
