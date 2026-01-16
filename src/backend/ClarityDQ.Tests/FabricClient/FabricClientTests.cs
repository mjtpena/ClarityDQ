using ClarityDQ.FabricClient;

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
}
