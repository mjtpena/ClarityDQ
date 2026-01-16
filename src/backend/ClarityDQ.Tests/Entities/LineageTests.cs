using ClarityDQ.Core.Entities;

namespace ClarityDQ.Tests.Entities;

public class LineageNodeTests
{
    [Fact]
    public void LineageNode_CanBeCreated()
    {
        var node = new LineageNode
        {
            Id = Guid.NewGuid(),
            WorkspaceId = "workspace1",
            NodeName = "TestTable",
            NodeType = LineageNodeType.Table,
            DatasetName = "dataset1",
            TableName = "table1",
            Description = "Test description",
            DiscoveredAt = DateTime.UtcNow,
            LastSeenAt = DateTime.UtcNow,
            CreatedBy = "testuser"
        };

        Assert.NotEqual(Guid.Empty, node.Id);
        Assert.Equal("workspace1", node.WorkspaceId);
        Assert.Equal("TestTable", node.NodeName);
        Assert.Equal(LineageNodeType.Table, node.NodeType);
    }

    [Fact]
    public void LineageNode_AllNodeTypes_AreValid()
    {
        var types = new[]
        {
            LineageNodeType.Table,
            LineageNodeType.Column,
            LineageNodeType.View,
            LineageNodeType.Procedure,
            LineageNodeType.Report,
            LineageNodeType.Pipeline
        };

        foreach (var type in types)
        {
            var node = new LineageNode
            {
                WorkspaceId = "ws",
                NodeName = $"Node_{type}",
                NodeType = type
            };

            Assert.Equal(type, node.NodeType);
        }
    }
}

public class LineageEdgeTests
{
    [Fact]
    public void LineageEdge_CanBeCreated()
    {
        var sourceId = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        var edge = new LineageEdge
        {
            Id = Guid.NewGuid(),
            SourceNodeId = sourceId,
            TargetNodeId = targetId,
            TransformationType = "Filter",
            TransformationLogic = "WHERE status = 'active'",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "testuser"
        };

        Assert.NotEqual(Guid.Empty, edge.Id);
        Assert.Equal(sourceId, edge.SourceNodeId);
        Assert.Equal(targetId, edge.TargetNodeId);
        Assert.Equal("Filter", edge.TransformationType);
    }

    [Fact]
    public void LineageEdge_CanHaveNavigationProperties()
    {
        var sourceNode = new LineageNode
        {
            Id = Guid.NewGuid(),
            WorkspaceId = "ws",
            NodeName = "Source"
        };

        var targetNode = new LineageNode
        {
            Id = Guid.NewGuid(),
            WorkspaceId = "ws",
            NodeName = "Target"
        };

        var edge = new LineageEdge
        {
            SourceNodeId = sourceNode.Id,
            TargetNodeId = targetNode.Id,
            TransformationType = "Join",
            SourceNode = sourceNode,
            TargetNode = targetNode
        };

        Assert.NotNull(edge.SourceNode);
        Assert.NotNull(edge.TargetNode);
        Assert.Equal("Source", edge.SourceNode.NodeName);
        Assert.Equal("Target", edge.TargetNode.NodeName);
    }
}

public class LineageGraphTests
{
    [Fact]
    public void LineageGraph_CanAddNodes()
    {
        var graph = new LineageGraph();
        var node1 = new LineageNode { Id = Guid.NewGuid(), WorkspaceId = "ws", NodeName = "N1" };
        var node2 = new LineageNode { Id = Guid.NewGuid(), WorkspaceId = "ws", NodeName = "N2" };

        graph.AddNode(node1);
        graph.AddNode(node2);

        Assert.Equal(2, graph.Nodes.Count);
        Assert.Contains(node1, graph.Nodes);
        Assert.Contains(node2, graph.Nodes);
    }

    [Fact]
    public void LineageGraph_AddNode_PreventsAddingSameNodeTwice()
    {
        var graph = new LineageGraph();
        var node = new LineageNode { Id = Guid.NewGuid(), WorkspaceId = "ws", NodeName = "N1" };

        graph.AddNode(node);
        graph.AddNode(node);

        Assert.Single(graph.Nodes);
    }

    [Fact]
    public void LineageGraph_CanAddEdges()
    {
        var graph = new LineageGraph();
        var edge1 = new LineageEdge { Id = Guid.NewGuid(), SourceNodeId = Guid.NewGuid(), TargetNodeId = Guid.NewGuid(), TransformationType = "T1" };
        var edge2 = new LineageEdge { Id = Guid.NewGuid(), SourceNodeId = Guid.NewGuid(), TargetNodeId = Guid.NewGuid(), TransformationType = "T2" };

        graph.AddEdge(edge1);
        graph.AddEdge(edge2);

        Assert.Equal(2, graph.Edges.Count);
        Assert.Contains(edge1, graph.Edges);
        Assert.Contains(edge2, graph.Edges);
    }

    [Fact]
    public void LineageGraph_AddEdge_PreventsAddingSameEdgeTwice()
    {
        var graph = new LineageGraph();
        var edge = new LineageEdge { Id = Guid.NewGuid(), SourceNodeId = Guid.NewGuid(), TargetNodeId = Guid.NewGuid(), TransformationType = "T" };

        graph.AddEdge(edge);
        graph.AddEdge(edge);

        Assert.Single(graph.Edges);
    }

    [Fact]
    public void LineageGraph_GetUpstreamNodes_ReturnsCorrectNodes()
    {
        var graph = new LineageGraph();
        var node1 = new LineageNode { Id = Guid.NewGuid(), WorkspaceId = "ws", NodeName = "Source1" };
        var node2 = new LineageNode { Id = Guid.NewGuid(), WorkspaceId = "ws", NodeName = "Source2" };
        var node3 = new LineageNode { Id = Guid.NewGuid(), WorkspaceId = "ws", NodeName = "Target" };
        
        graph.AddNode(node1);
        graph.AddNode(node2);
        graph.AddNode(node3);
        
        graph.AddEdge(new LineageEdge { SourceNodeId = node1.Id, TargetNodeId = node3.Id, TransformationType = "T1" });
        graph.AddEdge(new LineageEdge { SourceNodeId = node2.Id, TargetNodeId = node3.Id, TransformationType = "T2" });
        
        var upstreamNodes = graph.GetUpstreamNodes(node3.Id);
        
        Assert.Equal(2, upstreamNodes.Count);
        Assert.Contains(node1, upstreamNodes);
        Assert.Contains(node2, upstreamNodes);
    }

    [Fact]
    public void LineageGraph_GetDownstreamNodes_ReturnsCorrectNodes()
    {
        var graph = new LineageGraph();
        var node1 = new LineageNode { Id = Guid.NewGuid(), WorkspaceId = "ws", NodeName = "Source" };
        var node2 = new LineageNode { Id = Guid.NewGuid(), WorkspaceId = "ws", NodeName = "Target1" };
        var node3 = new LineageNode { Id = Guid.NewGuid(), WorkspaceId = "ws", NodeName = "Target2" };
        
        graph.AddNode(node1);
        graph.AddNode(node2);
        graph.AddNode(node3);
        
        graph.AddEdge(new LineageEdge { SourceNodeId = node1.Id, TargetNodeId = node2.Id, TransformationType = "T1" });
        graph.AddEdge(new LineageEdge { SourceNodeId = node1.Id, TargetNodeId = node3.Id, TransformationType = "T2" });
        
        var downstreamNodes = graph.GetDownstreamNodes(node1.Id);
        
        Assert.Equal(2, downstreamNodes.Count);
        Assert.Contains(node2, downstreamNodes);
        Assert.Contains(node3, downstreamNodes);
    }

    [Fact]
    public void LineageGraph_GetUpstreamNodes_WithNoUpstream_ReturnsEmpty()
    {
        var graph = new LineageGraph();
        var node = new LineageNode { Id = Guid.NewGuid(), WorkspaceId = "ws", NodeName = "Source" };
        graph.AddNode(node);
        
        var upstreamNodes = graph.GetUpstreamNodes(node.Id);
        
        Assert.Empty(upstreamNodes);
    }

    [Fact]
    public void LineageGraph_GetDownstreamNodes_WithNoDownstream_ReturnsEmpty()
    {
        var graph = new LineageGraph();
        var node = new LineageNode { Id = Guid.NewGuid(), WorkspaceId = "ws", NodeName = "Terminal" };
        graph.AddNode(node);
        
        var downstreamNodes = graph.GetDownstreamNodes(node.Id);
        
        Assert.Empty(downstreamNodes);
    }
}

public class ScheduleTests
{
    [Fact]
    public void Schedule_CanBeCreated()
    {
        var schedule = new Schedule
        {
            Id = Guid.NewGuid(),
            Name = "Daily Report",
            Type = ScheduleType.DataProfiling,
            WorkspaceId = "ws1",
            DatasetName = "ds1",
            TableName = "table1",
            CronExpression = "0 0 * * *",
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "user1"
        };

        Assert.NotEqual(Guid.Empty, schedule.Id);
        Assert.Equal("Daily Report", schedule.Name);
        Assert.Equal(ScheduleType.DataProfiling, schedule.Type);
    }

    [Fact]
    public void Schedule_AllScheduleTypes_AreValid()
    {
        var types = new[]
        {
            ScheduleType.RuleExecution,
            ScheduleType.DataProfiling
        };

        foreach (var type in types)
        {
            var schedule = new Schedule
            {
                Name = $"Schedule_{type}",
                Type = type,
                CronExpression = "0 0 * * *"
            };

            Assert.Equal(type, schedule.Type);
        }
    }
}

public class ScheduleExecutionTests
{
    [Fact]
    public void ScheduleExecution_CanBeCreated()
    {
        var scheduleId = Guid.NewGuid();
        var execution = new ScheduleExecution
        {
            Id = Guid.NewGuid(),
            ScheduleId = scheduleId,
            StartedAt = DateTime.UtcNow,
            Status = ScheduleExecutionStatus.Running,
            ResultSummary = "Processing..."
        };

        Assert.NotEqual(Guid.Empty, execution.Id);
        Assert.Equal(scheduleId, execution.ScheduleId);
        Assert.Equal(ScheduleExecutionStatus.Running, execution.Status);
    }

    [Fact]
    public void ScheduleExecution_AllStatuses_AreValid()
    {
        var statuses = new[]
        {
            ScheduleExecutionStatus.Running,
            ScheduleExecutionStatus.Completed,
            ScheduleExecutionStatus.Failed
        };

        foreach (var status in statuses)
        {
            var execution = new ScheduleExecution
            {
                ScheduleId = Guid.NewGuid(),
                Status = status,
                StartedAt = DateTime.UtcNow
            };

            Assert.Equal(status, execution.Status);
        }
    }

    [Fact]
    public void ScheduleExecution_CanHaveErrorDetails()
    {
        var execution = new ScheduleExecution
        {
            Id = Guid.NewGuid(),
            ScheduleId = Guid.NewGuid(),
            StartedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow.AddMinutes(5),
            Status = ScheduleExecutionStatus.Failed,
            ErrorMessage = "Connection timeout"
        };

        Assert.Equal(ScheduleExecutionStatus.Failed, execution.Status);
        Assert.Equal("Connection timeout", execution.ErrorMessage);
        Assert.NotNull(execution.CompletedAt);
    }

    [Fact]
    public void Schedule_CanHaveRuleNavigation()
    {
        var rule = new Rule
        {
            Id = Guid.NewGuid(),
            Name = "Test Rule",
            Type = RuleType.Completeness
        };

        var schedule = new Schedule
        {
            Id = Guid.NewGuid(),
            Name = "Rule Schedule",
            Type = ScheduleType.RuleExecution,
            RuleId = rule.Id,
            Rule = rule,
            CronExpression = "0 0 * * *"
        };

        Assert.NotNull(schedule.Rule);
        Assert.Equal(rule.Id, schedule.RuleId);
        Assert.Equal("Test Rule", schedule.Rule.Name);
    }

    [Fact]
    public void ScheduleExecution_CanHaveScheduleNavigation()
    {
        var schedule = new Schedule
        {
            Id = Guid.NewGuid(),
            Name = "Test Schedule",
            Type = ScheduleType.DataProfiling,
            CronExpression = "0 0 * * *"
        };

        var execution = new ScheduleExecution
        {
            Id = Guid.NewGuid(),
            ScheduleId = schedule.Id,
            Schedule = schedule,
            StartedAt = DateTime.UtcNow,
            Status = ScheduleExecutionStatus.Running
        };

        Assert.NotNull(execution.Schedule);
        Assert.Equal(schedule.Id, execution.ScheduleId);
        Assert.Equal("Test Schedule", execution.Schedule.Name);
    }

    [Fact]
    public void Schedule_CanHaveLastAndNextRunDates()
    {
        var lastRun = DateTime.UtcNow.AddDays(-1);
        var nextRun = DateTime.UtcNow.AddDays(1);

        var schedule = new Schedule
        {
            Id = Guid.NewGuid(),
            Name = "Recurring Schedule",
            Type = ScheduleType.DataProfiling,
            CronExpression = "0 0 * * *",
            LastRunAt = lastRun,
            NextRunAt = nextRun,
            IsEnabled = true
        };

        Assert.Equal(lastRun, schedule.LastRunAt);
        Assert.Equal(nextRun, schedule.NextRunAt);
        Assert.True(schedule.IsEnabled);
    }

    [Fact]
    public void ScheduleExecution_CompletedHasResultSummary()
    {
        var execution = new ScheduleExecution
        {
            Id = Guid.NewGuid(),
            ScheduleId = Guid.NewGuid(),
            StartedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow.AddMinutes(10),
            Status = ScheduleExecutionStatus.Completed,
            ResultSummary = "Processed 1000 records successfully"
        };

        Assert.Equal(ScheduleExecutionStatus.Completed, execution.Status);
        Assert.NotNull(execution.ResultSummary);
        Assert.Contains("1000 records", execution.ResultSummary);
    }
}
