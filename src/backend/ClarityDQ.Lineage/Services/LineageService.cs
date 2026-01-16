using ClarityDQ.Core.Entities;
using ClarityDQ.Core.Interfaces;
using ClarityDQ.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClarityDQ.Lineage.Services;

public class LineageService : ILineageService
{
    private readonly ClarityDbContext _context;

    public LineageService(ClarityDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> CreateNodeAsync(LineageNode node, CancellationToken cancellationToken = default)
    {
        node.Id = Guid.NewGuid();
        node.DiscoveredAt = DateTime.UtcNow;
        node.LastSeenAt = DateTime.UtcNow;

        _context.LineageNodes.Add(node);
        await _context.SaveChangesAsync(cancellationToken);
        return node.Id;
    }

    public async Task<Guid> CreateEdgeAsync(LineageEdge edge, CancellationToken cancellationToken = default)
    {
        edge.Id = Guid.NewGuid();
        edge.CreatedAt = DateTime.UtcNow;

        _context.LineageEdges.Add(edge);
        await _context.SaveChangesAsync(cancellationToken);
        return edge.Id;
    }

    public async Task<LineageGraph> GetLineageGraphAsync(string workspaceId, string? datasetName = null, CancellationToken cancellationToken = default)
    {
        var query = _context.LineageNodes.Where(n => n.WorkspaceId == workspaceId);
        
        if (!string.IsNullOrEmpty(datasetName))
        {
            query = query.Where(n => n.DatasetName == datasetName);
        }

        var nodes = await query.ToListAsync(cancellationToken);
        var nodeIds = nodes.Select(n => n.Id).ToHashSet();
        
        var edges = await _context.LineageEdges
            .Where(e => nodeIds.Contains(e.SourceNodeId) || nodeIds.Contains(e.TargetNodeId))
            .Include(e => e.SourceNode)
            .Include(e => e.TargetNode)
            .ToListAsync(cancellationToken);

        return new LineageGraph { Nodes = nodes, Edges = edges };
    }

    public async Task<LineageGraph> GetUpstreamLineageAsync(Guid nodeId, int depth = 10, CancellationToken cancellationToken = default)
    {
        var graph = new LineageGraph();
        var visited = new HashSet<Guid>();
        await TraverseUpstream(nodeId, depth, graph, visited, cancellationToken);
        return graph;
    }

    public async Task<LineageGraph> GetDownstreamLineageAsync(Guid nodeId, int depth = 10, CancellationToken cancellationToken = default)
    {
        var graph = new LineageGraph();
        var visited = new HashSet<Guid>();
        await TraverseDownstream(nodeId, depth, graph, visited, cancellationToken);
        return graph;
    }

    public async Task<LineageNode?> GetNodeAsync(Guid nodeId, CancellationToken cancellationToken = default)
    {
        return await _context.LineageNodes.FindAsync(new object[] { nodeId }, cancellationToken);
    }

    public async Task<List<LineageNode>> GetNodesAsync(string workspaceId, LineageNodeType? nodeType = null, CancellationToken cancellationToken = default)
    {
        var query = _context.LineageNodes.Where(n => n.WorkspaceId == workspaceId);
        
        if (nodeType.HasValue)
        {
            query = query.Where(n => n.NodeType == nodeType.Value);
        }

        return await query.OrderByDescending(n => n.LastSeenAt).ToListAsync(cancellationToken);
    }

    public async Task DeleteNodeAsync(Guid nodeId, CancellationToken cancellationToken = default)
    {
        var node = await _context.LineageNodes.FindAsync(new object[] { nodeId }, cancellationToken);
        if (node != null)
        {
            var edges = await _context.LineageEdges
                .Where(e => e.SourceNodeId == nodeId || e.TargetNodeId == nodeId)
                .ToListAsync(cancellationToken);
            
            _context.LineageEdges.RemoveRange(edges);
            _context.LineageNodes.Remove(node);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task TraverseUpstream(Guid nodeId, int depth, LineageGraph graph, HashSet<Guid> visited, CancellationToken cancellationToken)
    {
        if (depth <= 0 || visited.Contains(nodeId)) return;
        visited.Add(nodeId);

        var node = await GetNodeAsync(nodeId, cancellationToken);
        if (node != null) graph.AddNode(node);

        var upstreamEdges = await _context.LineageEdges
            .Include(e => e.SourceNode)
            .Where(e => e.TargetNodeId == nodeId)
            .ToListAsync(cancellationToken);

        foreach (var edge in upstreamEdges)
        {
            graph.AddEdge(edge);
            if (edge.SourceNode != null) graph.AddNode(edge.SourceNode);
            await TraverseUpstream(edge.SourceNodeId, depth - 1, graph, visited, cancellationToken);
        }
    }

    private async Task TraverseDownstream(Guid nodeId, int depth, LineageGraph graph, HashSet<Guid> visited, CancellationToken cancellationToken)
    {
        if (depth <= 0 || visited.Contains(nodeId)) return;
        visited.Add(nodeId);

        var node = await GetNodeAsync(nodeId, cancellationToken);
        if (node != null) graph.AddNode(node);

        var downstreamEdges = await _context.LineageEdges
            .Include(e => e.TargetNode)
            .Where(e => e.SourceNodeId == nodeId)
            .ToListAsync(cancellationToken);

        foreach (var edge in downstreamEdges)
        {
            graph.AddEdge(edge);
            if (edge.TargetNode != null) graph.AddNode(edge.TargetNode);
            await TraverseDownstream(edge.TargetNodeId, depth - 1, graph, visited, cancellationToken);
        }
    }
}
