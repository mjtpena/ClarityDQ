using ClarityDQ.RuleEngine;

namespace ClarityDQ.Tests.RuleEngine;

public class MockRuleDataSourceTests
{
    [Fact]
    public async Task GetDataAsync_ReturnsConsistentData()
    {
        var dataSource = new MockRuleDataSource();

        var result = await dataSource.GetDataAsync("ws1", "dataset1", "table1");

        Assert.Equal(100, result.TotalRecords);
        Assert.Equal(100, result.Rows.Count());
        Assert.Equal(6, result.Schema.Count);
    }

    [Fact]
    public async Task GetDataAsync_SchemaContainsExpectedColumns()
    {
        var dataSource = new MockRuleDataSource();

        var result = await dataSource.GetDataAsync("ws1", "dataset1", "table1");

        Assert.True(result.Schema.ContainsKey("Id"));
        Assert.True(result.Schema.ContainsKey("Name"));
        Assert.True(result.Schema.ContainsKey("Email"));
        Assert.True(result.Schema.ContainsKey("Age"));
        Assert.True(result.Schema.ContainsKey("Score"));
        Assert.True(result.Schema.ContainsKey("Status"));
    }

    [Fact]
    public async Task GetDataAsync_SchemaHasCorrectTypes()
    {
        var dataSource = new MockRuleDataSource();

        var result = await dataSource.GetDataAsync("ws1", "dataset1", "table1");

        Assert.Equal(typeof(int), result.Schema["Id"]);
        Assert.Equal(typeof(string), result.Schema["Name"]);
        Assert.Equal(typeof(string), result.Schema["Email"]);
        Assert.Equal(typeof(int), result.Schema["Age"]);
        Assert.Equal(typeof(double), result.Schema["Score"]);
        Assert.Equal(typeof(string), result.Schema["Status"]);
    }

    [Fact]
    public async Task GetDataAsync_RowsContainExpectedData()
    {
        var dataSource = new MockRuleDataSource();

        var result = await dataSource.GetDataAsync("ws1", "dataset1", "table1");

        var firstRow = result.Rows.First();
        Assert.Equal(1, firstRow["Id"]);
        Assert.Equal("User1", firstRow["Name"]);
        Assert.Equal("user1@example.com", firstRow["Email"]);
        Assert.NotNull(firstRow["Age"]);
        Assert.NotNull(firstRow["Score"]);
        Assert.Equal("Inactive", firstRow["Status"]);
    }

    [Fact]
    public async Task GetDataAsync_ContainsNullValues()
    {
        var dataSource = new MockRuleDataSource();

        var result = await dataSource.GetDataAsync("ws1", "dataset1", "table1");

        var row10 = result.Rows.ElementAt(9);
        Assert.Null(row10["Name"]);

        var row15 = result.Rows.ElementAt(14);
        Assert.Null(row15["Email"]);
    }

    [Fact]
    public async Task GetDataAsync_StatusPatternIsCorrect()
    {
        var dataSource = new MockRuleDataSource();

        var result = await dataSource.GetDataAsync("ws1", "dataset1", "table1");

        var row5 = result.Rows.ElementAt(4);
        Assert.Equal("Active", row5["Status"]);

        var row1 = result.Rows.ElementAt(0);
        Assert.Equal("Inactive", row1["Status"]);
    }

    [Fact]
    public async Task GetDataAsync_WithColumnName_ReturnsData()
    {
        var dataSource = new MockRuleDataSource();

        var result = await dataSource.GetDataAsync("ws1", "dataset1", "table1", "Name");

        Assert.Equal(100, result.TotalRecords);
        Assert.Equal(100, result.Rows.Count());
    }

    [Fact]
    public async Task GetDataAsync_IsDeterministic()
    {
        var dataSource1 = new MockRuleDataSource();
        var dataSource2 = new MockRuleDataSource();

        var result1 = await dataSource1.GetDataAsync("ws1", "dataset1", "table1");
        var result2 = await dataSource2.GetDataAsync("ws1", "dataset1", "table1");

        Assert.Equal(result1.TotalRecords, result2.TotalRecords);
        Assert.Equal(result1.Rows.First()["Age"], result2.Rows.First()["Age"]);
        Assert.Equal(result1.Rows.First()["Score"], result2.Rows.First()["Score"]);
    }
}
