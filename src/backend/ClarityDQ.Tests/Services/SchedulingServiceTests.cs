using ClarityDQ.Core.Entities;
using ClarityDQ.Core.Interfaces;
using ClarityDQ.Infrastructure.Data;
using ClarityDQ.Profiling.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ClarityDQ.Tests.Services;

public class SchedulingServiceTests : IDisposable
{
    private readonly ClarityDbContext _context;
    private readonly Mock<IRuleService> _ruleServiceMock;
    private readonly Mock<IProfilingService> _profilingServiceMock;
    private readonly SchedulingService _service;

    public SchedulingServiceTests()
    {
        var options = new DbContextOptionsBuilder<ClarityDbContext>()
            .UseInMemoryDatabase(databaseName: $"SchedulingTest_{Guid.NewGuid()}")
            .Options;
        
        _context = new ClarityDbContext(options);
        _ruleServiceMock = new Mock<IRuleService>();
        _profilingServiceMock = new Mock<IProfilingService>();
        _service = new SchedulingService(_context, _ruleServiceMock.Object, _profilingServiceMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task CreateScheduleAsync_CreatesSchedule()
    {
        var schedule = new Schedule
        {
            Name = "Daily Job",
            Type = ScheduleType.RuleExecution,
            RuleId = Guid.NewGuid(),
            CronExpression = "0 0 * * *",
            IsEnabled = true
        };

        var result = await _service.CreateScheduleAsync(schedule);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.NotNull(result.CreatedAt);
        
        Assert.Equal("Daily Job", result.Name);
    }

    [Fact]
    public async Task GetScheduleAsync_ReturnsSchedule()
    {
        var schedule = new Schedule
        {
            Id = Guid.NewGuid(),
            Name = "Test Schedule",
            CronExpression = "0 0 * * *",
            CreatedAt = DateTime.UtcNow
        };
        _context.Schedules.Add(schedule);
        await _context.SaveChangesAsync();

        var result = await _service.GetScheduleAsync(schedule.Id);

        Assert.NotNull(result);
        Assert.Equal(schedule.Id, result.Id);
        Assert.Equal("Test Schedule", result.Name);
    }

    [Fact]
    public async Task GetScheduleAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _service.GetScheduleAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetSchedulesAsync_ReturnsAllSchedules()
    {
        var schedules = new[]
        {
            new Schedule { Id = Guid.NewGuid(), Name = "Schedule 1", CronExpression = "0 0 * * *", CreatedAt = DateTime.UtcNow, IsEnabled = true },
            new Schedule { Id = Guid.NewGuid(), Name = "Schedule 2", CronExpression = "0 0 * * *", CreatedAt = DateTime.UtcNow, IsEnabled = false }
        };
        _context.Schedules.AddRange(schedules);
        await _context.SaveChangesAsync();

        var result = await _service.GetSchedulesAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetSchedulesAsync_WithEnabledFilter_ReturnsOnlyEnabled()
    {
        var schedules = new[]
        {
            new Schedule { Id = Guid.NewGuid(), Name = "Enabled", CronExpression = "0 0 * * *", CreatedAt = DateTime.UtcNow, IsEnabled = true },
            new Schedule { Id = Guid.NewGuid(), Name = "Disabled", CronExpression = "0 0 * * *", CreatedAt = DateTime.UtcNow, IsEnabled = false }
        };
        _context.Schedules.AddRange(schedules);
        await _context.SaveChangesAsync();

        var result = await _service.GetSchedulesAsync(enabledOnly: true);

        Assert.Single(result);
        Assert.True(result[0].IsEnabled);
    }

    [Fact]
    public async Task UpdateScheduleAsync_UpdatesSchedule()
    {
        var schedule = new Schedule
        {
            Id = Guid.NewGuid(),
            Name = "Original",
            CronExpression = "0 0 * * *",
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Schedules.Add(schedule);
        await _context.SaveChangesAsync();

        schedule.Name = "Updated";
        schedule.CronExpression = "0 12 * * *";
        schedule.IsEnabled = false;

        var result = await _service.UpdateScheduleAsync(schedule);

        Assert.Equal("Updated", result.Name);
        Assert.Equal("0 12 * * *", result.CronExpression);
        Assert.False(result.IsEnabled);
        
    }

    [Fact]
    public async Task DeleteScheduleAsync_DeletesSchedule()
    {
        var schedule = new Schedule
        {
            Id = Guid.NewGuid(),
            Name = "To Delete",
            CronExpression = "0 0 * * *",
            CreatedAt = DateTime.UtcNow
        };
        _context.Schedules.Add(schedule);
        await _context.SaveChangesAsync();

        await _service.DeleteScheduleAsync(schedule.Id);

        var deleted = await _context.Schedules.FindAsync(schedule.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteScheduleAsync_DoesNothing_WhenScheduleNotFound()
    {
        await _service.DeleteScheduleAsync(Guid.NewGuid());
        // Should not throw
        Assert.True(true);
    }

    [Fact]
    public async Task ExecuteScheduleAsync_CreatesExecution()
    {
        var rule = new Rule
        {
            Id = Guid.NewGuid(),
            Name = "Test Rule",
            Type = RuleType.Completeness,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test"
        };
        var schedule = new Schedule
        {
            Id = Guid.NewGuid(),
            Name = "Test Schedule",
            Type = ScheduleType.RuleExecution,
            RuleId = rule.Id,
            CronExpression = "0 0 * * *",
            CreatedAt = DateTime.UtcNow
        };
        _context.Rules.Add(rule);
        _context.Schedules.Add(schedule);
        await _context.SaveChangesAsync();

        _ruleServiceMock
            .Setup(s => s.ExecuteRuleAsync(rule.Id, default))
            .ReturnsAsync(new RuleExecution());

        var result = await _service.ExecuteScheduleAsync(schedule.Id);

        Assert.NotNull(result);
        Assert.Equal(schedule.Id, result.ScheduleId);
        Assert.Equal(ScheduleExecutionStatus.Running, result.Status);
        
    }

    [Fact]
    public async Task ExecuteScheduleAsync_ThrowsException_WhenScheduleNotFound()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.ExecuteScheduleAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetScheduleExecutionsAsync_ReturnsExecutions()
    {
        var schedule = new Schedule
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            CronExpression = "0 0 * * *",
            CreatedAt = DateTime.UtcNow
        };
        var executions = new[]
        {
            new ScheduleExecution { Id = Guid.NewGuid(), ScheduleId = schedule.Id, StartedAt = DateTime.UtcNow.AddHours(-2), Status = ScheduleExecutionStatus.Completed },
            new ScheduleExecution { Id = Guid.NewGuid(), ScheduleId = schedule.Id, StartedAt = DateTime.UtcNow.AddHours(-1), Status = ScheduleExecutionStatus.Completed }
        };
        _context.Schedules.Add(schedule);
        _context.ScheduleExecutions.AddRange(executions);
        await _context.SaveChangesAsync();

        var result = await _service.GetScheduleExecutionsAsync(schedule.Id);

        Assert.Equal(2, result.Count);
        Assert.True(result[0].StartedAt > result[1].StartedAt); // Most recent first
    }

    [Fact]
    public async Task GetScheduleExecutionsAsync_WithPagination_ReturnsPagedResults()
    {
        var schedule = new Schedule
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            CronExpression = "0 0 * * *",
            CreatedAt = DateTime.UtcNow
        };
        var executions = Enumerable.Range(0, 100)
            .Select(i => new ScheduleExecution
            {
                Id = Guid.NewGuid(),
                ScheduleId = schedule.Id,
                StartedAt = DateTime.UtcNow.AddHours(-i),
                Status = ScheduleExecutionStatus.Completed
            });
        _context.Schedules.Add(schedule);
        _context.ScheduleExecutions.AddRange(executions);
        await _context.SaveChangesAsync();

        var result = await _service.GetScheduleExecutionsAsync(schedule.Id, skip: 10, take: 20);

        Assert.Equal(20, result.Count);
    }

    [Fact]
    public async Task ExecuteScheduleAsync_WithRuleExecution_CompletesSuccessfully()
    {
        var rule = new Rule
        {
            Id = Guid.NewGuid(),
            Name = "Test Rule",
            Type = RuleType.Completeness,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test"
        };
        var schedule = new Schedule
        {
            Id = Guid.NewGuid(),
            Name = "Test Schedule",
            Type = ScheduleType.RuleExecution,
            RuleId = rule.Id,
            CronExpression = "0 0 * * *",
            CreatedAt = DateTime.UtcNow,
            IsEnabled = true
        };
        _context.Rules.Add(rule);
        _context.Schedules.Add(schedule);
        await _context.SaveChangesAsync();

        _ruleServiceMock
            .Setup(s => s.ExecuteRuleAsync(rule.Id, default))
            .ReturnsAsync(new RuleExecution());

        var result = await _service.ExecuteScheduleAsync(schedule.Id);
        
        await Task.Delay(500);
        
        _ruleServiceMock.Verify(s => s.ExecuteRuleAsync(rule.Id, default), Times.Once);
    }

    [Fact]
    public async Task ExecuteScheduleAsync_WithDataProfiling_CompletesSuccessfully()
    {
        var schedule = new Schedule
        {
            Id = Guid.NewGuid(),
            Name = "Profiling Schedule",
            Type = ScheduleType.DataProfiling,
            WorkspaceId = "workspace123",
            DatasetName = "dataset",
            TableName = "table",
            CronExpression = "0 0 * * *",
            CreatedAt = DateTime.UtcNow,
            IsEnabled = true
        };
        _context.Schedules.Add(schedule);
        await _context.SaveChangesAsync();

        _profilingServiceMock
            .Setup(s => s.ProfileTableAsync("workspace123", "dataset", "table", default))
            .ReturnsAsync(Guid.NewGuid());

        var result = await _service.ExecuteScheduleAsync(schedule.Id);
        
        await Task.Delay(500);
        
        _profilingServiceMock.Verify(s => s.ProfileTableAsync("workspace123", "dataset", "table", default), Times.Once);
    }

    [Fact]
    public async Task CreateScheduleAsync_WithInvalidCron_SetsNextRunToNull()
    {
        var schedule = new Schedule
        {
            Name = "Invalid Cron",
            Type = ScheduleType.RuleExecution,
            CronExpression = "invalid cron",
            IsEnabled = true
        };

        var result = await _service.CreateScheduleAsync(schedule);

        Assert.Null(result.NextRunAt);
    }

    [Fact]
    public async Task CreateScheduleAsync_WithValidCron_CalculatesNextRun()
    {
        var schedule = new Schedule
        {
            Name = "Valid Cron",
            Type = ScheduleType.RuleExecution,
            CronExpression = "0 0 * * *",
            IsEnabled = true
        };

        var result = await _service.CreateScheduleAsync(schedule);

        Assert.NotNull(result.NextRunAt);
        Assert.True(result.NextRunAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task UpdateScheduleAsync_RecalculatesNextRun()
    {
        var schedule = new Schedule
        {
            Id = Guid.NewGuid(),
            Name = "Schedule",
            CronExpression = "0 0 * * *",
            CreatedAt = DateTime.UtcNow,
            IsEnabled = true
        };
        _context.Schedules.Add(schedule);
        await _context.SaveChangesAsync();

        schedule.CronExpression = "0 12 * * *";
        var result = await _service.UpdateScheduleAsync(schedule);

        Assert.NotNull(result.NextRunAt);
    }
}
