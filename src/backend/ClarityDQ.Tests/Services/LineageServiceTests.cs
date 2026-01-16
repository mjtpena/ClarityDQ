using ClarityDQ.Core.Entities;
using ClarityDQ.Infrastructure.Data;
using ClarityDQ.Lineage.Services;
using Microsoft.EntityFrameworkCore;

namespace ClarityDQ.Tests.Services;

public class LineageServiceTests : IDisposable
{
    private readonly ClarityDbContext _context;
    private readonly LineageService _service;

    public LineageServiceTests()
    {
        var options = new DbContextOptionsBuilder<ClarityDbContext>()
            .UseInMemoryDatabase(databaseName: $"LineageTest_{Guid.NewGuid()}")
            .Options;
        
        _context = new ClarityDbContext(options);
        _service = new LineageService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task CreateNodeAsync_CreatesNode()
    {
        var node = new LineageNode
        {
            WorkspaceId = "workspace1",
            NodeName = "Test Table",
            DatasetName = "dataset1",
            TableName = "table1",
            NodeType = LineageNodeType.Table
        };

        var nodeId = await _service.CreateNodeAsync(node);

        Assert.NotEqual(Guid.Empty, nodeId);
        
        var saved = await _context.LineageNodes.FindAsync(nodeId);
        Assert.NotNull(saved);
        Assert.Equal("workspace1", saved.WorkspaceId);
    }

    [Fact]
    public async Task CreateEdgeAsync_CreatesEdge()
    {
        var sourceNode = new LineageNode
        {
            Id = Guid.NewGuid(),
            WorkspaceId = "ws1",
            DatasetName = "ds1",
            TableName = "source",
            NodeType = LineageNodeType.Table,
            NodeName = "Source"
        };
        var targetNode = new LineageNode
        {
            Id = Guid.NewGuid(),
            WorkspaceId = "ws1",
            DatasetName = "ds1",
            TableName = "target",
            NodeType = LineageNodeType.Table,
            NodeName = "Target"
        };
        _context.LineageNodes.AddRange(sourceNode, targetNode);
        await _context.SaveChangesAsync();

        var edge = new LineageEdge
        {
            SourceNodeId = sourceNode.Id,
            TargetNodeId = targetNode.Id,
            TransformationType = "Copy"
        };

        var edgeId = await _service.CreateEdgeAsync(edge);

        Assert.NotEqual(Guid.Empty, edgeId);
        
        
        var saved = await _context.LineageEdges.FindAsync(edgeId);
        Assert.NotNull(saved);
        Assert.Equal(sourceNode.Id, saved.SourceNodeId);
        Assert.Equal(targetNode.Id, saved.TargetNodeId);
    }

    [Fact]
    public async Task GetLineageGraphAsync_ReturnsGraphForWorkspace()
    {
        var nodes = new[]
        {
            new LineageNode { Id = Guid.NewGuid(), WorkspaceId = "ws1", DatasetName = "ds1", TableName = "t1", NodeType = LineageNodeType.Table, NodeName = "T1" },
            new LineageNode { Id = Guid.NewGuid(), WorkspaceId = "ws1", DatasetName = "ds1", TableName = "t2", NodeType = LineageNodeType.Table, NodeName = "T2" },
            new LineageNode { Id = Guid.NewGuid(), WorkspaceId = "ws2", DatasetName = "ds2", TableName = "t3", NodeType = LineageNodeType.Table, NodeName = "T3" }
        };
        _context.LineageNodes.AddRange(nodes);
        await _context.SaveChangesAsync();

        var edge = new LineageEdge
        {
            Id = Guid.NewGuid(),
            SourceNodeId = nodes[0].Id,
            TargetNodeId = nodes[1].Id,
            TransformationType = "Transform"
        };
        _context.LineageEdges.Add(edge);
        await _context.SaveChangesAsync();

        var graph = await _service.GetLineageGraphAsync("ws1");

        Assert.Equal(2, graph.Nodes.Count);
        Assert.Single(graph.Edges);
    }

    [Fact]
    public async Task GetLineageGraphAsync_WithDatasetFilter_ReturnsFilteredGraph()
    {
        var nodes = new[]
        {
            new LineageNode { Id = Guid.NewGuid(), WorkspaceId = "ws1", DatasetName = "ds1", TableName = "t1", NodeType = LineageNodeType.Table, NodeName = "T1" },
            new LineageNode { Id = Guid.NewGuid(), WorkspaceId = "ws1", DatasetName = "ds2", TableName = "t2", NodeType = LineageNodeType.Table, NodeName = "T2" }
        };
        _context.LineageNodes.AddRange(nodes);
        await _context.SaveChangesAsync();

        var graph = await _service.GetLineageGraphAsync("ws1", "ds1");

        Assert.Single(graph.Nodes);
        Assert.Equal("ds1", graph.Nodes[0].DatasetName);
    }

    [Fact]
    public async Task GetUpstreamLineageAsync_ReturnsUpstreamNodes()
    {
        var nodes = new[]
        {
            new LineageNode { Id = Guid.NewGuid(), WorkspaceId = "ws1", DatasetName = "ds", TableName = "source", NodeType = LineageNodeType.Table, NodeName = "Source" },
            new LineageNode { Id = Guid.NewGuid(), WorkspaceId = "ws1", DatasetName = "ds", TableName = "intermediate", NodeType = LineageNodeType.Table, NodeName = "Intermediate" },
            new LineageNode { Id = Guid.NewGuid(), WorkspaceId = "ws1", DatasetName = "ds", TableName = "target", NodeType = LineageNodeType.Table, NodeName = "Target" }
        };
        _context.LineageNodes.AddRange(nodes);
        await _context.SaveChangesAsync();

        var edges = new[]
        {
            new LineageEdge { Id = Guid.NewGuid(), SourceNodeId = nodes[0].Id, TargetNodeId = nodes[1].Id, TransformationType = "T1", SourceNode = nodes[0], TargetNode = nodes[1] },
            new LineageEdge { Id = Guid.NewGuid(), SourceNodeId = nodes[1].Id, TargetNodeId = nodes[2].Id, TransformationType = "T2", SourceNode = nodes[1], TargetNode = nodes[2] }
        };
        _context.LineageEdges.AddRange(edges);
        await _context.SaveChangesAsync();

        var graph = await _service.GetUpstreamLineageAsync(nodes[2].Id);

        Assert.Equal(3, graph.Nodes.Count);
        Assert.Equal(2, graph.Edges.Count);
    }

    [Fact]
    public async Task GetDownstreamLineageAsync_ReturnsDownstreamNodes()
    {
        var nodes = new[]
        {
            new LineageNode { Id = Guid.NewGuid(), WorkspaceId = "ws1", DatasetName = "ds", TableName = "source", NodeType = LineageNodeType.Table, NodeName = "Source" },
            new LineageNode { Id = Guid.NewGuid(), WorkspaceId = "ws1", DatasetName = "ds", TableName = "intermediate", NodeType = LineageNodeType.Table, NodeName = "Intermediate" },
            new LineageNode { Id = Guid.NewGuid(), WorkspaceId = "ws1", DatasetName = "ds", TableName = "target", NodeType = LineageNodeType.Table, NodeName = "Target" }
        };
        _context.LineageNodes.AddRange(nodes);
        await _context.SaveChangesAsync();

        var edges = new[]
        {
            new LineageEdge { Id = Guid.NewGuid(), SourceNodeId = nodes[0].Id, TargetNodeId = nodes[1].Id, TransformationType = "T1", SourceNode = nodes[0], TargetNode = nodes[1] },
            new LineageEdge { Id = Guid.NewGuid(), SourceNodeId = nodes[1].Id, TargetNodeId = nodes[2].Id, TransformationType = "T2", SourceNode = nodes[1], TargetNode = nodes[2] }
        };
        _context.LineageEdges.AddRange(edges);
        await _context.SaveChangesAsync();

        var graph = await _service.GetDownstreamLineageAsync(nodes[0].Id);

        Assert.Equal(3, graph.Nodes.Count);
        Assert.Equal(2, graph.Edges.Count);
    }

    [Fact]
    public async Task GetUpstreamLineageAsync_RespectsDepthLimit()
    {
        var nodes = Enumerable.Range(0, 5)
            .Select(i => new LineageNode
            {
                Id = Guid.NewGuid(),
                WorkspaceId = "ws1",
                DatasetName = "ds",
                TableName = $"table{i}",
                NodeType = LineageNodeType.Table,
                NodeName = $"Table {i}"
            })
            .ToArray();
        _context.LineageNodes.AddRange(nodes);
        await _context.SaveChangesAsync();

        var edges = new[]
        {
            new LineageEdge { Id = Guid.NewGuid(), SourceNodeId = nodes[0].Id, TargetNodeId = nodes[1].Id, TransformationType = "T", SourceNode = nodes[0], TargetNode = nodes[1] },
            new LineageEdge { Id = Guid.NewGuid(), SourceNodeId = nodes[1].Id, TargetNodeId = nodes[2].Id, TransformationType = "T", SourceNode = nodes[1], TargetNode = nodes[2] },
            new LineageEdge { Id = Guid.NewGuid(), SourceNodeId = nodes[2].Id, TargetNodeId = nodes[3].Id, TransformationType = "T", SourceNode = nodes[2], TargetNode = nodes[3] },
            new LineageEdge { Id = Guid.NewGuid(), SourceNodeId = nodes[3].Id, TargetNodeId = nodes[4].Id, TransformationType = "T", SourceNode = nodes[3], TargetNode = nodes[4] }
        };
        _context.LineageEdges.AddRange(edges);
        await _context.SaveChangesAsync();

        var graph = await _service.GetUpstreamLineageAsync(nodes[4].Id, depth: 2);

        Assert.True(graph.Nodes.Count <= 3); // Target + 2 levels up
    }

    [Fact]
    public async Task GetNodeAsync_ReturnsNode()
    {
        var node = new LineageNode
        {
            Id = Guid.NewGuid(),
            WorkspaceId = "ws1",
            DatasetName = "ds1",
            TableName = "t1",
            NodeType = LineageNodeType.Table,
            NodeName = "Test Table"
        };
        _context.LineageNodes.Add(node);
        await _context.SaveChangesAsync();

        var result = await _service.GetNodeAsync(node.Id);

        Assert.NotNull(result);
        Assert.Equal(node.Id, result.Id);
        Assert.Equal("Test Table", result.NodeName);
    }

    [Fact]
    public async Task GetNodeAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _service.GetNodeAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetNodesAsync_ReturnsNodesForWorkspace()
    {
        var nodes = new[]
        {
            new LineageNode { Id = Guid.NewGuid(), WorkspaceId = "ws1", DatasetName = "ds", TableName = "t1", NodeType = LineageNodeType.Table, NodeName = "T1", LastSeenAt = DateTime.UtcNow },
            new LineageNode { Id = Guid.NewGuid(), WorkspaceId = "ws1", DatasetName = "ds", TableName = "t2", NodeType = LineageNodeType.Pipeline, NodeName = "T2", LastSeenAt = DateTime.UtcNow.AddHours(-1) },
            new LineageNode { Id = Guid.NewGuid(), WorkspaceId = "ws2", DatasetName = "ds", TableName = "t3", NodeType = LineageNodeType.Table, NodeName = "T3", LastSeenAt = DateTime.UtcNow }
        };
        _context.LineageNodes.AddRange(nodes);
        await _context.SaveChangesAsync();

        var result = await _service.GetNodesAsync("ws1");

        Assert.Equal(2, result.Count);
        Assert.True(result[0].LastSeenAt >= result[1].LastSeenAt); // Ordered by LastSeenAt desc
    }

    [Fact]
    public async Task GetNodesAsync_WithTypeFilter_ReturnsFilteredNodes()
    {
        var nodes = new[]
        {
            new LineageNode { Id = Guid.NewGuid(), WorkspaceId = "ws1", DatasetName = "ds", TableName = "t1", NodeType = LineageNodeType.Table, NodeName = "T1", LastSeenAt = DateTime.UtcNow },
            new LineageNode { Id = Guid.NewGuid(), WorkspaceId = "ws1", DatasetName = "ds", TableName = "p1", NodeType = LineageNodeType.Pipeline, NodeName = "P1", LastSeenAt = DateTime.UtcNow }
        };
        _context.LineageNodes.AddRange(nodes);
        await _context.SaveChangesAsync();

        var result = await _service.GetNodesAsync("ws1", LineageNodeType.Table);

        Assert.Single(result);
        Assert.Equal(LineageNodeType.Table, result[0].NodeType);
    }

    [Fact]
    public async Task DeleteNodeAsync_DeletesNodeAndEdges()
    {
        var nodes = new[]
        {
            new LineageNode { Id = Guid.NewGuid(), WorkspaceId = "ws1", DatasetName = "ds", TableName = "t1", NodeType = LineageNodeType.Table, NodeName = "T1" },
            new LineageNode { Id = Guid.NewGuid(), WorkspaceId = "ws1", DatasetName = "ds", TableName = "t2", NodeType = LineageNodeType.Table, NodeName = "T2" }
        };
        _context.LineageNodes.AddRange(nodes);
        await _context.SaveChangesAsync();

        var edge = new LineageEdge
        {
            Id = Guid.NewGuid(),
            SourceNodeId = nodes[0].Id,
            TargetNodeId = nodes[1].Id,
            TransformationType = "T"
        };
        _context.LineageEdges.Add(edge);
        await _context.SaveChangesAsync();

        await _service.DeleteNodeAsync(nodes[0].Id);

        var deletedNode = await _context.LineageNodes.FindAsync(nodes[0].Id);
        var deletedEdge = await _context.LineageEdges.FindAsync(edge.Id);
        
        Assert.Null(deletedNode);
        Assert.Null(deletedEdge);
    }

    [Fact]
    public async Task DeleteNodeAsync_DoesNothing_WhenNodeNotFound()
    {
        await _service.DeleteNodeAsync(Guid.NewGuid());
        // Should not throw
        Assert.True(true);
    }
}
