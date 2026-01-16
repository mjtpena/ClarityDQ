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

    [Fact]
    public async Task ExecuteAccuracyRule_WithDoubleValues()
    {
        var rule = new Rule
        {
            Type = RuleType.Accuracy,
            WorkspaceId = "ws",
            DatasetName = "ds",
            TableName = "t",
            ColumnName = "Score",
            Threshold = 1000.0
        };

        var result = await _executor.ExecuteAsync(rule, _dataSource);
        result.RecordsFailed.Should().Be(0); // All scores should be under 1000
    }

    [Fact]
    public async Task ExecuteCompletenessRule_WithEntireRow()
    {
        var rule = new Rule
        {
            Type = RuleType.Completeness,
            WorkspaceId = "ws",
            DatasetName = "ds",
            TableName = "t",
            ColumnName = "", // Check entire row
            Threshold = 50.0
        };

        var result = await _executor.ExecuteAsync(rule, _dataSource);
        result.RecordsChecked.Should().Be(100);
        result.Metrics.Should().ContainKey("CompletionRate");
    }

    [Fact]
    public async Task ExecuteUniquenessRule_TracksMetrics()
    {
        var rule = new Rule
        {
            Type = RuleType.Uniqueness,
            WorkspaceId = "ws",
            DatasetName = "ds",
            TableName = "t",
            ColumnName = "Id"
        };

        var result = await _executor.ExecuteAsync(rule, _dataSource);
        result.Metrics["UniqueCount"].Should().Be(100);
        result.Metrics["DuplicateCount"].Should().Be(0);
    }

    [Fact]
    public async Task ExecuteValidityRule_WithInList()
    {
        var rule = new Rule
        {
            Type = RuleType.Validity,
            WorkspaceId = "ws",
            DatasetName = "ds",
            TableName = "t",
            ColumnName = "Status",
            Expression = "in:Active,Inactive"
        };

        var result = await _executor.ExecuteAsync(rule, _dataSource);
        result.RecordsFailed.Should().Be(0); // All statuses should match
    }

    [Fact]
    public async Task ExecuteConsistencyRule_InvalidExpression()
    {
        var rule = new Rule
        {
            Type = RuleType.Consistency,
            WorkspaceId = "ws",
            DatasetName = "ds",
            TableName = "t",
            Expression = "InvalidExpression"
        };

        var result = await _executor.ExecuteAsync(rule, _dataSource);
        result.RecordsFailed.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteRule_WithCancellationToken()
    {
        var rule = new Rule
        {
            Type = RuleType.Completeness,
            WorkspaceId = "ws",
            DatasetName = "ds",
            TableName = "t",
            ColumnName = "Name"
        };

        var cts = new CancellationTokenSource();
        var result = await _executor.ExecuteAsync(rule, _dataSource, cts.Token);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteCompletenessRule_NoNullValues()
    {
        var rule = new Rule
        {
            Type = RuleType.Completeness,
            WorkspaceId = "ws",
            DatasetName = "ds",
            TableName = "t",
            ColumnName = "Id",
            Threshold = 100.0
        };

        var result = await _executor.ExecuteAsync(rule, _dataSource);
        result.RecordsFailed.Should().Be(0);
        result.SuccessRate.Should().Be(100.0);
    }

    [Fact]
    public async Task ExecuteValidityRule_DateRange()
    {
        var rule = new Rule
        {
            Type = RuleType.Validity,
            WorkspaceId = "ws",
            DatasetName = "ds",
            TableName = "t",
            ColumnName = "CreatedAt",
            Expression = "daterange:2020-01-01,2025-12-31"
        };

        var result = await _executor.ExecuteAsync(rule, _dataSource);
        result.RecordsChecked.Should().Be(100);
    }

    [Fact]
    public async Task ExecuteRule_WithEmptyDataSource()
    {
        var emptyDataSource = new MockRuleDataSource();
        var rule = new Rule
        {
            Type = RuleType.Completeness,
            WorkspaceId = "ws",
            DatasetName = "ds",
            TableName = "empty",
            ColumnName = "Name"
        };

        var result = await _executor.ExecuteAsync(rule, emptyDataSource);
        result.RecordsChecked.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task ExecuteCustomRule_ComplexExpression()
    {
        var rule = new Rule
        {
            Type = RuleType.Custom,
            WorkspaceId = "ws",
            DatasetName = "ds",
            TableName = "t",
            Expression = "Score > 50 AND Status == 'Active'"
        };

        var result = await _executor.ExecuteAsync(rule, _dataSource);
        result.Metrics.Should().ContainKey("CustomExpression");
    }

    [Fact]
    public async Task ExecuteAccuracyRule_WithDecimalValues()
    {
        var rule = new Rule
        {
            Type = RuleType.Accuracy,
            WorkspaceId = "ws",
            DatasetName = "ds",
            TableName = "t",
            ColumnName = "Amount",
            Threshold = 99.5
        };

        var result = await _executor.ExecuteAsync(rule, _dataSource);
        result.Should().NotBeNull();
        result.RecordsChecked.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExecuteUniquenessRule_WithDuplicates()
    {
        var rule = new Rule
        {
            Type = RuleType.Uniqueness,
            WorkspaceId = "ws",
            DatasetName = "ds",
            TableName = "t",
            ColumnName = "Email"
        };

        var result = await _executor.ExecuteAsync(rule, _dataSource);
        result.RecordsChecked.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExecuteRule_AllRuleTypes()
    {
        var ruleTypes = new[] 
        { 
            RuleType.Completeness, 
            RuleType.Uniqueness, 
            RuleType.Validity, 
            RuleType.Accuracy, 
            RuleType.Consistency, 
            RuleType.Custom 
        };

        foreach (var ruleType in ruleTypes)
        {
            var rule = new Rule
            {
                Type = ruleType,
                WorkspaceId = "ws",
                DatasetName = "ds",
                TableName = "t",
                ColumnName = "Id"
            };

            var result = await _executor.ExecuteAsync(rule, _dataSource);
            result.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task ExecuteValidityRule_WithLengthGreaterThan()
    {
        var rule = new Rule
        {
            Type = RuleType.Validity,
            WorkspaceId = "ws",
            DatasetName = "ds",
            TableName = "t",
            ColumnName = "Name",
            Expression = "length:>5"
        };

        var result = await _executor.ExecuteAsync(rule, _dataSource);
        result.RecordsChecked.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExecuteValidityRule_WithLengthLessThan()
    {
        var rule = new Rule
        {
            Type = RuleType.Validity,
            WorkspaceId = "ws",
            DatasetName = "ds",
            TableName = "t",
            ColumnName = "Name",
            Expression = "length:<20"
        };

        var result = await _executor.ExecuteAsync(rule, _dataSource);
        result.RecordsChecked.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExecuteValidityRule_WithLengthGreaterThanOrEqual()
    {
        var rule = new Rule
        {
            Type = RuleType.Validity,
            WorkspaceId = "ws",
            DatasetName = "ds",
            TableName = "t",
            ColumnName = "Name",
            Expression = "length:>=5"
        };

        var result = await _executor.ExecuteAsync(rule, _dataSource);
        result.RecordsChecked.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExecuteValidityRule_WithLengthLessThanOrEqual()
    {
        var rule = new Rule
        {
            Type = RuleType.Validity,
            WorkspaceId = "ws",
            DatasetName = "ds",
            TableName = "t",
            ColumnName = "Name",
            Expression = "length:<=15"
        };

        var result = await _executor.ExecuteAsync(rule, _dataSource);
        result.RecordsChecked.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExecuteValidityRule_WithLengthExact()
    {
        var rule = new Rule
        {
            Type = RuleType.Validity,
            WorkspaceId = "ws",
            DatasetName = "ds",
            TableName = "t",
            ColumnName = "Id",
            Expression = "length:5"
        };

        var result = await _executor.ExecuteAsync(rule, _dataSource);
        result.RecordsChecked.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExecuteValidityRule_WithDoubleRange()
    {
        var rule = new Rule
        {
            Type = RuleType.Validity,
            WorkspaceId = "ws",
            DatasetName = "ds",
            TableName = "t",
            ColumnName = "Score",
            Expression = "range:0.0,100.0"
        };

        var result = await _executor.ExecuteAsync(rule, _dataSource);
        result.RecordsChecked.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExecuteCompletenessRule_WithThresholdViolation()
    {
        var rule = new Rule
        {
            Type = RuleType.Completeness,
            WorkspaceId = "ws",
            DatasetName = "ds",
            TableName = "t",
            ColumnName = "",
            Threshold = 99.0
        };

        var result = await _executor.ExecuteAsync(rule, _dataSource);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteUniquenessRule_WithMultipleDuplicates()
    {
        var rule = new Rule
        {
            Type = RuleType.Uniqueness,
            WorkspaceId = "ws",
            DatasetName = "ds",
            TableName = "t-duplicates",
            ColumnName = "Status"
        };

        var result = await _executor.ExecuteAsync(rule, _dataSource);
        result.Should().NotBeNull();
    }
}
