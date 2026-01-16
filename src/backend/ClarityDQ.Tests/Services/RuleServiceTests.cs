using ClarityDQ.Core.Entities;
using ClarityDQ.Infrastructure.Data;
using ClarityDQ.Profiling.Services;
using ClarityDQ.RuleEngine;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ClarityDQ.Tests.Services;

public class RuleServiceTests : IDisposable
{
    private readonly ClarityDbContext _context;
    private readonly RuleService _service;

    public RuleServiceTests()
    {
        var options = new DbContextOptionsBuilder<ClarityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ClarityDbContext(options);
        var executor = new RuleExecutor();
        var dataSource = new MockRuleDataSource();
        _service = new RuleService(_context, executor, dataSource);
    }

    [Fact]
    public async Task CreateRuleAsync_CreatesRule()
    {
        var rule = new Rule
        {
            Name = "Test Rule",
            Description = "Test Description",
            Type = RuleType.Completeness,
            WorkspaceId = "ws-1",
            DatasetName = "dataset-1",
            TableName = "table-1",
            Expression = "column IS NOT NULL",
            Threshold = 95.0,
            Severity = RuleSeverity.High,
            IsEnabled = true,
            CreatedBy = "test-user"
        };

        var created = await _service.CreateRuleAsync(rule);

        created.Id.Should().NotBeEmpty();
        created.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        
        var retrieved = await _context.Rules.FindAsync(created.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Test Rule");
    }

    [Fact]
    public async Task GetRuleAsync_ReturnsRule_WhenExists()
    {
        var rule = new Rule
        {
            Id = Guid.NewGuid(),
            Name = "Existing Rule",
            WorkspaceId = "ws-1",
            DatasetName = "ds-1",
            TableName = "t-1",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "user"
        };

        _context.Rules.Add(rule);
        await _context.SaveChangesAsync();

        var result = await _service.GetRuleAsync(rule.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Existing Rule");
    }

    [Fact]
    public async Task GetRuleAsync_ReturnsNull_WhenNotExists()
    {
        var result = await _service.GetRuleAsync(Guid.NewGuid());
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRulesAsync_ReturnsAllRules_WhenNoFilter()
    {
        var rules = new[]
        {
            new Rule { Id = Guid.NewGuid(), Name = "Rule 1", WorkspaceId = "ws-1", DatasetName = "ds", TableName = "t", IsEnabled = true, CreatedAt = DateTime.UtcNow, CreatedBy = "user" },
            new Rule { Id = Guid.NewGuid(), Name = "Rule 2", WorkspaceId = "ws-1", DatasetName = "ds", TableName = "t", IsEnabled = false, CreatedAt = DateTime.UtcNow, CreatedBy = "user" },
        };

        _context.Rules.AddRange(rules);
        await _context.SaveChangesAsync();

        var result = await _service.GetRulesAsync("ws-1");

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRulesAsync_FiltersEnabledRules()
    {
        var rules = new[]
        {
            new Rule { Id = Guid.NewGuid(), Name = "Rule 1", WorkspaceId = "ws-1", DatasetName = "ds", TableName = "t", IsEnabled = true, CreatedAt = DateTime.UtcNow, CreatedBy = "user" },
            new Rule { Id = Guid.NewGuid(), Name = "Rule 2", WorkspaceId = "ws-1", DatasetName = "ds", TableName = "t", IsEnabled = false, CreatedAt = DateTime.UtcNow, CreatedBy = "user" },
        };

        _context.Rules.AddRange(rules);
        await _context.SaveChangesAsync();

        var result = await _service.GetRulesAsync("ws-1", enabledOnly: true);

        result.Should().HaveCount(1);
        result.First().IsEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task GetRulesAsync_FiltersDisabledRules()
    {
        var rules = new[]
        {
            new Rule { Id = Guid.NewGuid(), Name = "Rule 1", WorkspaceId = "ws-1", DatasetName = "ds", TableName = "t", IsEnabled = true, CreatedAt = DateTime.UtcNow, CreatedBy = "user" },
            new Rule { Id = Guid.NewGuid(), Name = "Rule 2", WorkspaceId = "ws-1", DatasetName = "ds", TableName = "t", IsEnabled = false, CreatedAt = DateTime.UtcNow, CreatedBy = "user" },
        };

        _context.Rules.AddRange(rules);
        await _context.SaveChangesAsync();

        var result = await _service.GetRulesAsync("ws-1", enabledOnly: false);

        result.Should().HaveCount(1);
        result.First().IsEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateRuleAsync_UpdatesRule()
    {
        var rule = new Rule
        {
            Id = Guid.NewGuid(),
            Name = "Original",
            WorkspaceId = "ws-1",
            DatasetName = "ds",
            TableName = "t",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "user"
        };

        _context.Rules.Add(rule);
        await _context.SaveChangesAsync();

        rule.Name = "Updated";
        var updated = await _service.UpdateRuleAsync(rule);

        updated.Name.Should().Be("Updated");
        updated.UpdatedAt.Should().NotBeNull();
        updated.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task DeleteRuleAsync_DeletesRule()
    {
        var rule = new Rule
        {
            Id = Guid.NewGuid(),
            Name = "To Delete",
            WorkspaceId = "ws-1",
            DatasetName = "ds",
            TableName = "t",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "user"
        };

        _context.Rules.Add(rule);
        await _context.SaveChangesAsync();

        await _service.DeleteRuleAsync(rule.Id);

        var deleted = await _context.Rules.FindAsync(rule.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteRuleAsync_DoesNotThrow_WhenNotExists()
    {
        var act = async () => await _service.DeleteRuleAsync(Guid.NewGuid());
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ExecuteRuleAsync_CreatesExecution()
    {
        var rule = new Rule
        {
            Id = Guid.NewGuid(),
            Name = "Test Rule",
            Type = RuleType.Completeness,
            WorkspaceId = "ws-1",
            DatasetName = "ds",
            TableName = "t",
            ColumnName = "Name",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "user"
        };

        _context.Rules.Add(rule);
        await _context.SaveChangesAsync();

        var execution = await _service.ExecuteRuleAsync(rule.Id);

        execution.Should().NotBeNull();
        execution.RuleId.Should().Be(rule.Id);
    }

    [Fact]
    public async Task ExecuteRuleAsync_ThrowsWhenRuleNotFound()
    {
        var act = async () => await _service.ExecuteRuleAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ExecuteRuleAsync_CompletesSuccessfully()
    {
        var rule = new Rule
        {
            Id = Guid.NewGuid(),
            Name = "Test Rule",
            Type = RuleType.Completeness,
            WorkspaceId = "ws-1",
            DatasetName = "ds",
            TableName = "t",
            ColumnName = "Name",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "user"
        };

        _context.Rules.Add(rule);
        await _context.SaveChangesAsync();

        var execution = await _service.ExecuteRuleAsync(rule.Id);
        await Task.Delay(2000);

        var result = await _context.RuleExecutions.FindAsync(execution.Id);
        result.Should().NotBeNull();
        result!.Status.Should().Be(RuleExecutionStatus.Completed);
        result.RecordsChecked.Should().BeGreaterThan(0);
        result.SuccessRate.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetRuleExecutionsAsync_ReturnsExecutions()
    {
        var rule = new Rule
        {
            Id = Guid.NewGuid(),
            Name = "Test Rule",
            WorkspaceId = "ws-1",
            DatasetName = "ds",
            TableName = "t",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "user"
        };

        _context.Rules.Add(rule);

        var executions = new[]
        {
            new RuleExecution { Id = Guid.NewGuid(), RuleId = rule.Id, ExecutedAt = DateTime.UtcNow, Status = RuleExecutionStatus.Completed },
            new RuleExecution { Id = Guid.NewGuid(), RuleId = rule.Id, ExecutedAt = DateTime.UtcNow.AddHours(-1), Status = RuleExecutionStatus.Completed },
        };

        _context.RuleExecutions.AddRange(executions);
        await _context.SaveChangesAsync();

        var result = await _service.GetRuleExecutionsAsync(rule.Id);

        result.Should().HaveCount(2);
        result.Should().BeInDescendingOrder(e => e.ExecutedAt);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task ExecuteRuleAsync_HandlesAllRuleTypes()
    {
        var rules = new[]
        {
            new Rule { Id = Guid.NewGuid(), Name = "Completeness", Type = RuleType.Completeness, WorkspaceId = "ws-1", DatasetName = "ds", TableName = "t", ColumnName = "Name", CreatedAt = DateTime.UtcNow, CreatedBy = "user" },
            new Rule { Id = Guid.NewGuid(), Name = "Uniqueness", Type = RuleType.Uniqueness, WorkspaceId = "ws-1", DatasetName = "ds", TableName = "t", ColumnName = "Email", CreatedAt = DateTime.UtcNow, CreatedBy = "user" },
            new Rule { Id = Guid.NewGuid(), Name = "Validity", Type = RuleType.Validity, WorkspaceId = "ws-1", DatasetName = "ds", TableName = "t", ColumnName = "Email", Expression = "regex:.*@.*", CreatedAt = DateTime.UtcNow, CreatedBy = "user" },
        };

        foreach (var rule in rules)
        {
            _context.Rules.Add(rule);
            await _context.SaveChangesAsync();

            var execution = await _service.ExecuteRuleAsync(rule.Id);
            await Task.Delay(1500);

            var result = await _context.RuleExecutions.FindAsync(execution.Id);
            result.Should().NotBeNull();
            result!.Status.Should().Be(RuleExecutionStatus.Completed);
            result.RecordsChecked.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public async Task GetRulesAsync_WithEnabledFilter_ReturnsFilteredRules()
    {
        var rule1 = new Rule { Id = Guid.NewGuid(), Name = "R1", Type = RuleType.Completeness, WorkspaceId = "ws", DatasetName = "ds", TableName = "t", ColumnName = "c", IsEnabled = true, CreatedAt = DateTime.UtcNow, CreatedBy = "user" };
        var rule2 = new Rule { Id = Guid.NewGuid(), Name = "R2", Type = RuleType.Uniqueness, WorkspaceId = "ws", DatasetName = "ds", TableName = "t", ColumnName = "c", IsEnabled = false, CreatedAt = DateTime.UtcNow, CreatedBy = "user" };
        _context.Rules.AddRange(rule1, rule2);
        await _context.SaveChangesAsync();

        var results = await _service.GetRulesAsync("ws", enabledOnly: true);

        results.Should().HaveCount(1);
        results.First().IsEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteRuleAsync_WithNonExistent_DoesNotThrow()
    {
        await _service.DeleteRuleAsync(Guid.NewGuid());
        
        Assert.True(true);
    }

    [Fact]
    public async Task GetRuleExecutionsAsync_WithPagination_ReturnsCorrectPage()
    {
        var rule = new Rule { Id = Guid.NewGuid(), Name = "R", Type = RuleType.Completeness, WorkspaceId = "ws", DatasetName = "ds", TableName = "t", ColumnName = "c", CreatedAt = DateTime.UtcNow, CreatedBy = "user" };
        _context.Rules.Add(rule);
        await _context.SaveChangesAsync();

        for (int i = 0; i < 10; i++)
        {
            _context.RuleExecutions.Add(new RuleExecution { Id = Guid.NewGuid(), RuleId = rule.Id, ExecutedAt = DateTime.UtcNow.AddMinutes(-i), Status = RuleExecutionStatus.Completed, RecordsPassed = 100, RecordsFailed = 0 });
        }
        await _context.SaveChangesAsync();

        var page1 = await _service.GetRuleExecutionsAsync(rule.Id, skip: 0, take: 5);
        var page2 = await _service.GetRuleExecutionsAsync(rule.Id, skip: 5, take: 5);

        page1.Should().HaveCount(5);
        page2.Should().HaveCount(5);
        page1.Should().NotIntersectWith(page2);
    }
}
