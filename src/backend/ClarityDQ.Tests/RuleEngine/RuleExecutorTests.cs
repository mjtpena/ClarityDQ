using ClarityDQ.Core.Entities;
using ClarityDQ.RuleEngine;
using FluentAssertions;

namespace ClarityDQ.Tests.RuleEngine;

public class RuleExecutorTests
{
    private readonly RuleExecutor _executor;
    private readonly MockRuleDataSource _dataSource;

    public RuleExecutorTests()
    {
        _executor = new RuleExecutor();
        _dataSource = new MockRuleDataSource();
    }

    [Fact]
    public async Task ExecuteCompletenessRule_DetectsNullValues()
    {
        var rule = new Rule
        {
            Type = RuleType.Completeness,
            WorkspaceId = "ws-1",
            DatasetName = "ds",
            TableName = "t",
            ColumnName = "Name",
            Threshold = 95.0
        };

        var result = await _executor.ExecuteAsync(rule, _dataSource);

        result.RecordsChecked.Should().Be(100);
        result.RecordsFailed.Should().Be(10);
        result.SuccessRate.Should().Be(90.0);
    }

    [Fact]
    public async Task ExecuteUniquenessRule_DetectsDuplicates()
    {
        var rule = new Rule
        {
            Type = RuleType.Uniqueness,
            WorkspaceId = "ws-1",
            DatasetName = "ds",
            TableName = "t",
            ColumnName = "Id"
        };

        var result = await _executor.ExecuteAsync(rule, _dataSource);

        result.RecordsChecked.Should().Be(100);
        result.RecordsFailed.Should().Be(0);
        result.Metrics.Should().ContainKey("UniqueCount");
    }

    [Fact]
    public async Task ExecuteValidityRule_WithRegex()
    {
        var rule = new Rule
        {
            Type = RuleType.Validity,
            WorkspaceId = "ws-1",
            DatasetName = "ds",
            TableName = "t",
            ColumnName = "Email",
            Expression = "regex:^user\\d+@example\\.com$"
        };

        var result = await _executor.ExecuteAsync(rule, _dataSource);

        result.RecordsChecked.Should().Be(100);
        result.RecordsFailed.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task ExecuteValidityRule_WithRange()
    {
        var rule = new Rule
        {
            Type = RuleType.Validity,
            WorkspaceId = "ws-1",
            DatasetName = "ds",
            TableName = "t",
            ColumnName = "Age",
            Expression = "range:18,65"
        };

        var result = await _executor.ExecuteAsync(rule, _dataSource);

        result.RecordsChecked.Should().Be(100);
        result.Metrics.Should().ContainKey("ValidationRule");
    }

    [Fact]
    public async Task ExecuteConsistencyRule()
    {
        var rule = new Rule
        {
            Type = RuleType.Consistency,
            WorkspaceId = "ws-1",
            DatasetName = "ds",
            TableName = "t",
            Expression = "Status==Status"
        };

        var result = await _executor.ExecuteAsync(rule, _dataSource);

        result.RecordsChecked.Should().Be(100);
        result.RecordsFailed.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteCustomRule()
    {
        var rule = new Rule
        {
            Type = RuleType.Custom,
            WorkspaceId = "ws-1",
            DatasetName = "ds",
            TableName = "t",
            Expression = "Score > 50"
        };

        var result = await _executor.ExecuteAsync(rule, _dataSource);

        result.RecordsChecked.Should().Be(100);
        result.Metrics.Should().ContainKey("CustomExpression");
    }

    [Fact]
    public async Task ExecuteRule_ReturnsViolations()
    {
        var rule = new Rule
        {
            Type = RuleType.Completeness,
            WorkspaceId = "ws-1",
            DatasetName = "ds",
            TableName = "t",
            ColumnName = "Email"
        };

        var result = await _executor.ExecuteAsync(rule, _dataSource);

        result.Violations.Should().NotBeEmpty();
        result.Violations.First().ViolationMessage.Should().Contain("null or empty");
    }
}
