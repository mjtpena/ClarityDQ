using ClarityDQ.Core.Entities;
using FluentAssertions;

namespace ClarityDQ.Tests.Entities;

public class RuleTests
{
    [Fact]
    public void Rule_CanBeCreated()
    {
        var rule = new Rule
        {
            Id = Guid.NewGuid(),
            Name = "Test Rule",
            Description = "Test Description",
            Type = RuleType.Completeness,
            WorkspaceId = "ws-1",
            DatasetName = "ds-1",
            TableName = "t-1",
            ColumnName = "col-1",
            Expression = "column IS NOT NULL",
            Threshold = 95.0,
            Severity = RuleSeverity.High,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test-user"
        };

        rule.Should().NotBeNull();
        rule.Name.Should().Be("Test Rule");
        rule.Type.Should().Be(RuleType.Completeness);
    }

    [Fact]
    public void RuleType_AllValuesAreValid()
    {
        var types = new[]
        {
            RuleType.Completeness,
            RuleType.Accuracy,
            RuleType.Consistency,
            RuleType.Uniqueness,
            RuleType.Validity,
            RuleType.Custom
        };

        types.Should().HaveCount(6);
    }

    [Fact]
    public void RuleSeverity_AllValuesAreValid()
    {
        var severities = new[]
        {
            RuleSeverity.Low,
            RuleSeverity.Medium,
            RuleSeverity.High,
            RuleSeverity.Critical
        };

        severities.Should().HaveCount(4);
    }

    [Fact]
    public void RuleExecution_CanBeCreated()
    {
        var execution = new RuleExecution
        {
            Id = Guid.NewGuid(),
            RuleId = Guid.NewGuid(),
            ExecutedAt = DateTime.UtcNow,
            Status = RuleExecutionStatus.Completed,
            RecordsChecked = 1000,
            RecordsPassed = 950,
            RecordsFailed = 50,
            SuccessRate = 95.0,
            DurationMs = 1500
        };

        execution.Should().NotBeNull();
        execution.SuccessRate.Should().Be(95.0);
    }

    [Fact]
    public void RuleExecution_CanHaveNullValues()
    {
        var execution = new RuleExecution
        {
            Id = Guid.NewGuid(),
            RuleId = Guid.NewGuid(),
            ExecutedAt = DateTime.UtcNow,
            Status = RuleExecutionStatus.Pending,
            ResultDetails = null,
            ErrorMessage = null
        };

        execution.ResultDetails.Should().BeNull();
        execution.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void RuleExecution_CanSetRuleNavigation()
    {
        var rule = new Rule
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            WorkspaceId = "ws",
            DatasetName = "ds",
            TableName = "t",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "user"
        };

        var execution = new RuleExecution
        {
            Id = Guid.NewGuid(),
            RuleId = rule.Id,
            Rule = rule,
            ExecutedAt = DateTime.UtcNow,
            Status = RuleExecutionStatus.Completed
        };

        execution.Rule.Should().NotBeNull();
        execution.Rule!.Id.Should().Be(rule.Id);
    }

    [Fact]
    public void RuleExecutionStatus_AllValuesAreValid()
    {
        var statuses = new[]
        {
            RuleExecutionStatus.Pending,
            RuleExecutionStatus.Running,
            RuleExecutionStatus.Completed,
            RuleExecutionStatus.Failed,
            RuleExecutionStatus.Skipped
        };

        statuses.Should().HaveCount(5);
    }
}
