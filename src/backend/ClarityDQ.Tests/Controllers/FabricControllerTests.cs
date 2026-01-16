using ClarityDQ.Api.Controllers;
using ClarityDQ.FabricClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace ClarityDQ.Tests.Controllers;

public class FabricControllerTests
{
    private readonly Mock<IFabricClient> _fabricClientMock;
    private readonly Mock<ILogger<FabricController>> _loggerMock;
    private readonly FabricController _controller;

    public FabricControllerTests()
    {
        _fabricClientMock = new Mock<IFabricClient>();
        _loggerMock = new Mock<ILogger<FabricController>>();
        _controller = new FabricController(_fabricClientMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetWorkspaces_ReturnsOk_WithWorkspaces()
    {
        var workspaces = new[]
        {
            new FabricWorkspace { Id = "ws1", DisplayName = "Workspace 1" },
            new FabricWorkspace { Id = "ws2", DisplayName = "Workspace 2" }
        };

        _fabricClientMock
            .Setup(x => x.GetWorkspacesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(workspaces);

        var result = await _controller.GetWorkspaces(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedWorkspaces = Assert.IsAssignableFrom<FabricWorkspace[]>(okResult.Value);
        Assert.Equal(2, returnedWorkspaces.Length);
    }

    [Fact]
    public async Task GetWorkspaces_ReturnsServerError_OnException()
    {
        _fabricClientMock
            .Setup(x => x.GetWorkspacesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test error"));

        var result = await _controller.GetWorkspaces(CancellationToken.None);

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetWorkspaceItems_ReturnsOk_WithItems()
    {
        var items = new[]
        {
            new FabricItem { Id = "item1", DisplayName = "Item 1", Type = "Lakehouse" },
            new FabricItem { Id = "item2", DisplayName = "Item 2", Type = "Warehouse" }
        };

        _fabricClientMock
            .Setup(x => x.GetWorkspaceItemsAsync("ws1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);

        var result = await _controller.GetWorkspaceItems("ws1", CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedItems = Assert.IsAssignableFrom<FabricItem[]>(okResult.Value);
        Assert.Equal(2, returnedItems.Length);
    }

    [Fact]
    public async Task GetWorkspaceItems_ReturnsBadRequest_WhenWorkspaceIdEmpty()
    {
        var result = await _controller.GetWorkspaceItems("", CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Workspace ID is required", badRequestResult.Value);
    }

    [Fact]
    public async Task GetWorkspaceItems_ReturnsServerError_OnException()
    {
        _fabricClientMock
            .Setup(x => x.GetWorkspaceItemsAsync("ws1", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test error"));

        var result = await _controller.GetWorkspaceItems("ws1", CancellationToken.None);

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetTableSchema_ReturnsOk_WithSchema()
    {
        var schema = new FabricTableSchema
        {
            Name = "TestTable",
            Columns = new[]
            {
                new FabricColumn { Name = "Id", DataType = "int", IsNullable = false }
            }
        };

        _fabricClientMock
            .Setup(x => x.GetTableSchemaAsync("ws1", "lh1", "table1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(schema);

        var result = await _controller.GetTableSchema("ws1", "lh1", "table1", CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedSchema = Assert.IsType<FabricTableSchema>(okResult.Value);
        Assert.Equal("TestTable", returnedSchema.Name);
    }

    [Fact]
    public async Task GetTableSchema_ReturnsBadRequest_WhenWorkspaceIdEmpty()
    {
        var result = await _controller.GetTableSchema("", "lh1", "table1", CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Workspace ID is required", badRequestResult.Value);
    }

    [Fact]
    public async Task GetTableSchema_ReturnsBadRequest_WhenLakehouseIdEmpty()
    {
        var result = await _controller.GetTableSchema("ws1", "", "table1", CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Lakehouse ID is required", badRequestResult.Value);
    }

    [Fact]
    public async Task GetTableSchema_ReturnsBadRequest_WhenTableNameEmpty()
    {
        var result = await _controller.GetTableSchema("ws1", "lh1", "", CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Table name is required", badRequestResult.Value);
    }

    [Fact]
    public async Task GetTableSchema_ReturnsServerError_OnException()
    {
        _fabricClientMock
            .Setup(x => x.GetTableSchemaAsync("ws1", "lh1", "table1", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test error"));

        var result = await _controller.GetTableSchema("ws1", "lh1", "table1", CancellationToken.None);

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public void HealthCheck_ReturnsOk()
    {
        var result = _controller.HealthCheck();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }
}
