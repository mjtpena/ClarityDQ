using ClarityDQ.FabricClient;
using Moq;

namespace ClarityDQ.Tests.FabricClient;

public class FabricRuleDataSourceTests
{
    private readonly Mock<IFabricClient> _fabricClientMock;
    private readonly HttpClient _httpClient;
    private readonly FabricRuleDataSource _dataSource;

    public FabricRuleDataSourceTests()
    {
        _fabricClientMock = new Mock<IFabricClient>();
        _httpClient = new HttpClient();
        _dataSource = new FabricRuleDataSource(_fabricClientMock.Object, _httpClient);
    }

    [Fact]
    public async Task GetDataAsync_ReturnsMockData()
    {
        var result = await _dataSource.GetDataAsync("workspace1", "dataset1", "table1");

        Assert.NotNull(result);
        Assert.Equal(100, result.TotalRecords);
        Assert.Equal(100, result.Rows.Count());
    }

    [Fact]
    public async Task GetDataAsync_ContainsExpectedColumns()
    {
        var result = await _dataSource.GetDataAsync("workspace1", "dataset1", "table1");

        var firstRow = result.Rows.First();
        Assert.Contains("Id", firstRow.Keys);
        Assert.Contains("Name", firstRow.Keys);
        Assert.Contains("Status", firstRow.Keys);
        Assert.Contains("Value", firstRow.Keys);
        Assert.Contains("CreatedDate", firstRow.Keys);
    }

    [Fact]
    public async Task GetDataAsync_ContainsNullValues()
    {
        var result = await _dataSource.GetDataAsync("workspace1", "dataset1", "table1");

        var nullRows = result.Rows.Where(r => r["Name"] == null).ToList();
        Assert.NotEmpty(nullRows);
    }

    [Fact]
    public async Task GetDataAsync_WithColumnFilter_ReturnsData()
    {
        var result = await _dataSource.GetDataAsync("workspace1", "dataset1", "table1", "Name");

        Assert.NotNull(result);
        Assert.True(result.TotalRecords > 0);
    }
}
