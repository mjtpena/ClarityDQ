using ClarityDQ.FabricClient;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace ClarityDQ.Tests.FabricClient;

public class FabricClientTests
{
    [Fact]
    public void FabricClientOptions_DefaultBaseUrl()
    {
        var options = new FabricClientOptions();
        Assert.Equal("https://api.fabric.microsoft.com/v1", options.FabricApiBaseUrl);
    }

    [Fact]
    public void FabricClientOptions_SetProperties()
    {
        var options = new FabricClientOptions
        {
            TenantId = "tenant-123",
            ClientId = "client-456",
            ClientSecret = "secret-789",
            FabricApiBaseUrl = "https://custom.api.com"
        };

        Assert.Equal("tenant-123", options.TenantId);
        Assert.Equal("client-456", options.ClientId);
        Assert.Equal("secret-789", options.ClientSecret);
        Assert.Equal("https://custom.api.com", options.FabricApiBaseUrl);
    }

    [Fact]
    public void FabricWorkspace_DefaultValues()
    {
        var workspace = new FabricWorkspace();
        Assert.Equal(string.Empty, workspace.Id);
        Assert.Equal(string.Empty, workspace.DisplayName);
        Assert.Equal(string.Empty, workspace.Description);
        Assert.Equal(string.Empty, workspace.Type);
    }

    [Fact]
    public void FabricWorkspace_SetProperties()
    {
        var workspace = new FabricWorkspace
        {
            Id = "ws1",
            DisplayName = "My Workspace",
            Description = "Test description",
            Type = "Workspace"
        };

        Assert.Equal("ws1", workspace.Id);
        Assert.Equal("My Workspace", workspace.DisplayName);
        Assert.Equal("Test description", workspace.Description);
        Assert.Equal("Workspace", workspace.Type);
    }

    [Fact]
    public void FabricItem_DefaultValues()
    {
        var item = new FabricItem();
        Assert.Equal(string.Empty, item.Id);
        Assert.Equal(string.Empty, item.DisplayName);
        Assert.Equal(string.Empty, item.Type);
        Assert.Equal(string.Empty, item.Description);
    }

    [Fact]
    public void FabricItem_SetProperties()
    {
        var item = new FabricItem
        {
            Id = "item1",
            DisplayName = "Lakehouse 1",
            Type = "Lakehouse",
            Description = "Test lakehouse"
        };

        Assert.Equal("item1", item.Id);
        Assert.Equal("Lakehouse 1", item.DisplayName);
        Assert.Equal("Lakehouse", item.Type);
        Assert.Equal("Test lakehouse", item.Description);
    }

    [Fact]
    public void FabricTableSchema_DefaultValues()
    {
        var schema = new FabricTableSchema();
        Assert.Equal(string.Empty, schema.Name);
        Assert.Empty(schema.Columns);
    }

    [Fact]
    public void FabricTableSchema_SetProperties()
    {
        var schema = new FabricTableSchema
        {
            Name = "TestTable",
            Columns = new[]
            {
                new FabricColumn { Name = "Id", DataType = "int", IsNullable = false }
            }
        };

        Assert.Equal("TestTable", schema.Name);
        Assert.Single(schema.Columns);
        Assert.Equal("Id", schema.Columns[0].Name);
    }

    [Fact]
    public void FabricColumn_DefaultValues()
    {
        var column = new FabricColumn();
        Assert.Equal(string.Empty, column.Name);
        Assert.Equal(string.Empty, column.DataType);
        Assert.False(column.IsNullable);
    }

    [Fact]
    public void FabricColumn_SetProperties()
    {
        var column = new FabricColumn
        {
            Name = "TestColumn",
            DataType = "string",
            IsNullable = true
        };

        Assert.Equal("TestColumn", column.Name);
        Assert.Equal("string", column.DataType);
        Assert.True(column.IsNullable);
    }

    [Fact]
    public void FabricWorkspacesResponse_DefaultValue()
    {
        var response = new FabricWorkspacesResponse();
        Assert.NotNull(response.Value);
        Assert.Empty(response.Value);
    }

    [Fact]
    public void FabricWorkspacesResponse_SetValue()
    {
        var response = new FabricWorkspacesResponse
        {
            Value = new[] { new FabricWorkspace { Id = "ws1" } }
        };

        Assert.Single(response.Value);
        Assert.Equal("ws1", response.Value[0].Id);
    }

    [Fact]
    public void FabricItemsResponse_DefaultValue()
    {
        var response = new FabricItemsResponse();
        Assert.NotNull(response.Value);
        Assert.Empty(response.Value);
    }

    [Fact]
    public void FabricItemsResponse_SetValue()
    {
        var response = new FabricItemsResponse
        {
            Value = new[] { new FabricItem { Id = "item1" } }
        };

        Assert.Single(response.Value);
        Assert.Equal("item1", response.Value[0].Id);
    }

    [Fact]
    public void FabricClient_CanBeInstantiated()
    {
        var options = new FabricClientOptions
        {
            TenantId = "test-tenant",
            ClientId = "test-client",
            ClientSecret = "test-secret"
        };

        var httpClient = new HttpClient();
        var client = new ClarityDQ.FabricClient.FabricClient(httpClient, options);
        
        Assert.NotNull(client);
    }

    [Fact]
    public async Task GetWorkspacesAsync_ReturnsWorkspaces()
    {
        var workspaces = new FabricWorkspacesResponse
        {
            Value = new[]
            {
                new FabricWorkspace { Id = "ws1", DisplayName = "Workspace 1" },
                new FabricWorkspace { Id = "ws2", DisplayName = "Workspace 2" }
            }
        };

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(workspaces))
            });

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.fabric.microsoft.com/v1/") };
        var options = new FabricClientOptions
        {
            TenantId = "test-tenant",
            ClientId = "test-client",
            ClientSecret = "test-secret"
        };

        var mockCredential = new Mock<Azure.Core.TokenCredential>();
        mockCredential
            .Setup(c => c.GetTokenAsync(It.IsAny<Azure.Core.TokenRequestContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Azure.Core.AccessToken("mock-token", DateTimeOffset.UtcNow.AddHours(1)));

        var client = new ClarityDQ.FabricClient.FabricClient(httpClient, options, mockCredential.Object);
        var result = await client.GetWorkspacesAsync();
        
        Assert.NotNull(result);
        Assert.Equal(2, result.Length);
        Assert.Equal("ws1", result[0].Id);
    }

    [Fact]
    public async Task GetWorkspaceItemsAsync_ReturnsItems()
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        var items = new FabricItemsResponse
        {
            Value = new[]
            {
                new FabricItem { Id = "item1", DisplayName = "Item 1", Type = "Lakehouse" },
                new FabricItem { Id = "item2", DisplayName = "Item 2", Type = "Warehouse" }
            }
        };

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(items))
            });

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.fabric.microsoft.com/v1/") };
        var options = new FabricClientOptions
        {
            TenantId = "test-tenant",
            ClientId = "test-client",
            ClientSecret = "test-secret"
        };

        var mockCredential = new Mock<Azure.Core.TokenCredential>();
        mockCredential
            .Setup(c => c.GetTokenAsync(It.IsAny<Azure.Core.TokenRequestContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Azure.Core.AccessToken("mock-token", DateTimeOffset.UtcNow.AddHours(1)));

        var client = new ClarityDQ.FabricClient.FabricClient(httpClient, options, mockCredential.Object);
        var result = await client.GetWorkspaceItemsAsync("ws1");
        
        Assert.NotNull(result);
        Assert.Equal(2, result.Length);
    }

    [Fact]
    public async Task GetTableSchemaAsync_ReturnsSchema()
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        var schema = new FabricTableSchema
        {
            Name = "TestTable",
            Columns = new[]
            {
                new FabricColumn { Name = "Id", DataType = "int", IsNullable = false },
                new FabricColumn { Name = "Name", DataType = "string", IsNullable = true }
            }
        };

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(schema))
            });

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://api.fabric.microsoft.com/v1/") };
        var options = new FabricClientOptions
        {
            TenantId = "test-tenant",
            ClientId = "test-client",
            ClientSecret = "test-secret"
        };

        var mockCredential = new Mock<Azure.Core.TokenCredential>();
        mockCredential
            .Setup(c => c.GetTokenAsync(It.IsAny<Azure.Core.TokenRequestContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Azure.Core.AccessToken("mock-token", DateTimeOffset.UtcNow.AddHours(1)));

        var client = new ClarityDQ.FabricClient.FabricClient(httpClient, options, mockCredential.Object);
        var result = await client.GetTableSchemaAsync("ws1", "lh1", "TestTable");
        
        Assert.NotNull(result);
        Assert.Equal("TestTable", result.Name);
    }

    [Fact]
    public void FabricColumn_MultipleColumns()
    {
        var columns = new[]
        {
            new FabricColumn { Name = "Col1", DataType = "int", IsNullable = false },
            new FabricColumn { Name = "Col2", DataType = "string", IsNullable = true },
            new FabricColumn { Name = "Col3", DataType = "datetime", IsNullable = false }
        };

        Assert.Equal(3, columns.Length);
        Assert.All(columns, c => Assert.NotEmpty(c.Name));
    }

    [Fact]
    public void FabricWorkspace_MultipleProperties()
    {
        var ws = new FabricWorkspace
        {
            Id = "ws-123",
            DisplayName = "Production",
            Description = "Prod workspace",
            Type = "Premium"
        };

        Assert.Equal("ws-123", ws.Id);
        Assert.Equal("Production", ws.DisplayName);
        Assert.Equal("Prod workspace", ws.Description);
        Assert.Equal("Premium", ws.Type);
    }

    [Fact]
    public void FabricItem_AllTypes()
    {
        var items = new[]
        {
            new FabricItem { Type = "Lakehouse" },
            new FabricItem { Type = "Warehouse" },
            new FabricItem { Type = "Dataset" },
            new FabricItem { Type = "Pipeline" }
        };

        Assert.Equal(4, items.Length);
        Assert.Contains(items, i => i.Type == "Lakehouse");
        Assert.Contains(items, i => i.Type == "Warehouse");
    }
}
