using ClarityDQ.Api.Controllers;
using ClarityDQ.Core.Entities;
using ClarityDQ.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace ClarityDQ.Tests.Controllers;

public class LineageControllerTests
{
    private readonly Mock<ILineageService> _lineageServiceMock;
    private readonly Mock<ILogger<LineageController>> _loggerMock;
    private readonly LineageController _controller;

    public LineageControllerTests()
    {
        _lineageServiceMock = new Mock<ILineageService>();
        _loggerMock = new Mock<ILogger<LineageController>>();
        _controller = new LineageController(_lineageServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetWorkspaceLineage_ReturnsOk_WithGraph()
    {
        var graph = new LineageGraph();
        graph.AddNode(new LineageNode { Id = Guid.NewGuid(), WorkspaceId = "ws1", NodeName = "Table1" });

        _lineageServiceMock
            .Setup(x => x.GetLineageGraphAsync("ws1", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(graph);

        var result = await _controller.GetWorkspaceLineage("ws1", CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedGraph = Assert.IsType<LineageGraph>(okResult.Value);
        Assert.Single(returnedGraph.Nodes);
    }

    [Fact]
    public async Task GetWorkspaceLineage_ReturnsBadRequest_WhenWorkspaceIdEmpty()
    {
        var result = await _controller.GetWorkspaceLineage("", CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Workspace ID is required", badRequestResult.Value);
    }

    [Fact]
    public async Task GetWorkspaceLineage_ReturnsServerError_OnException()
    {
        _lineageServiceMock
            .Setup(x => x.GetLineageGraphAsync("ws1", null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test error"));

        var result = await _controller.GetWorkspaceLineage("ws1", CancellationToken.None);

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetTableLineage_ReturnsOk_WithGraph()
    {
        var graph = new LineageGraph();

        _lineageServiceMock
            .Setup(x => x.GetLineageGraphAsync("ws1", "ds1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(graph);

        var result = await _controller.GetTableLineage("ws1", "ds1", "table1", CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<LineageGraph>(okResult.Value);
    }

    [Fact]
    public async Task GetTableLineage_ReturnsBadRequest_WhenWorkspaceIdEmpty()
    {
        var result = await _controller.GetTableLineage("", "ds1", "table1", CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Workspace ID is required", badRequestResult.Value);
    }

    [Fact]
    public async Task GetTableLineage_ReturnsBadRequest_WhenDatasetNameEmpty()
    {
        var result = await _controller.GetTableLineage("ws1", "", "table1", CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Dataset name is required", badRequestResult.Value);
    }

    [Fact]
    public async Task GetTableLineage_ReturnsBadRequest_WhenTableNameEmpty()
    {
        var result = await _controller.GetTableLineage("ws1", "ds1", "", CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Table name is required", badRequestResult.Value);
    }

    [Fact]
    public async Task AddNode_CallsServiceCorrectly()
    {
        var nodeId = Guid.NewGuid();
        var request = new CreateLineageNodeRequest("ws1", "NewTable", LineageNodeType.Table, "ds1", "table1", "desc");

        _lineageServiceMock
            .Setup(x => x.CreateNodeAsync(It.IsAny<LineageNode>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(nodeId);

        var result = await _controller.AddNode(request, CancellationToken.None);

        Assert.NotNull(result);
        _lineageServiceMock.Verify(x => x.CreateNodeAsync(It.IsAny<LineageNode>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddNode_ReturnsServerError_OnException()
    {
        var request = new CreateLineageNodeRequest("ws1", "NewTable", LineageNodeType.Table, "ds1", "table1", "desc");

        _lineageServiceMock
            .Setup(x => x.CreateNodeAsync(It.IsAny<LineageNode>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test error"));

        var result = await _controller.AddNode(request, CancellationToken.None);

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task AddNode_ReturnsBadRequest_WhenWorkspaceIdEmpty()
    {
        var request = new CreateLineageNodeRequest("", "NodeName", LineageNodeType.Table, null, null, null);

        var result = await _controller.AddNode(request, CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Workspace ID is required", badRequestResult.Value);
    }

    [Fact]
    public async Task AddNode_ReturnsBadRequest_WhenNodeNameEmpty()
    {
        var request = new CreateLineageNodeRequest("ws1", "", LineageNodeType.Table, null, null, null);

        var result = await _controller.AddNode(request, CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Node name is required", badRequestResult.Value);
    }

    [Fact]
    public async Task AddEdge_CallsServiceCorrectly()
    {
        var sourceId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var edgeId = Guid.NewGuid();
        var request = new CreateLineageEdgeRequest(sourceId, targetId, "Join", "INNER JOIN");

        _lineageServiceMock
            .Setup(x => x.CreateEdgeAsync(It.IsAny<LineageEdge>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(edgeId);

        var result = await _controller.AddEdge(request, CancellationToken.None);

        Assert.NotNull(result);
        _lineageServiceMock.Verify(x => x.CreateEdgeAsync(It.IsAny<LineageEdge>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddEdge_ReturnsServerError_OnException()
    {
        var request = new CreateLineageEdgeRequest(Guid.NewGuid(), Guid.NewGuid(), "Transform", null);

        _lineageServiceMock
            .Setup(x => x.CreateEdgeAsync(It.IsAny<LineageEdge>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test error"));

        var result = await _controller.AddEdge(request, CancellationToken.None);

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task AddEdge_ReturnsBadRequest_WhenSourceNodeIdEmpty()
    {
        var request = new CreateLineageEdgeRequest(Guid.Empty, Guid.NewGuid(), "Transform", null);

        var result = await _controller.AddEdge(request, CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Source node ID is required", badRequestResult.Value);
    }

    [Fact]
    public async Task AddEdge_ReturnsBadRequest_WhenTargetNodeIdEmpty()
    {
        var request = new CreateLineageEdgeRequest(Guid.NewGuid(), Guid.Empty, "Transform", null);

        var result = await _controller.AddEdge(request, CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Target node ID is required", badRequestResult.Value);
    }
}
