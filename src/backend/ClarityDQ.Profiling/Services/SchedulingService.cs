using ClarityDQ.Core.Entities;
using ClarityDQ.Core.Interfaces;
using ClarityDQ.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NCrontab;

namespace ClarityDQ.Profiling.Services;

public class SchedulingService : ISchedulingService
{
    private readonly ClarityDbContext _context;
    private readonly IRuleService _ruleService;
    private readonly IProfilingService _profilingService;

    public SchedulingService(ClarityDbContext context, IRuleService ruleService, IProfilingService profilingService)
    {
        _context = context;
        _ruleService = ruleService;
        _profilingService = profilingService;
    }

    public async Task<Schedule> CreateScheduleAsync(Schedule schedule, CancellationToken cancellationToken = default)
    {
        schedule.Id = Guid.NewGuid();
        schedule.CreatedAt = DateTime.UtcNow;
        schedule.NextRunAt = CalculateNextRun(schedule.CronExpression);
        
        _context.Schedules.Add(schedule);
        await _context.SaveChangesAsync(cancellationToken);
        
        return schedule;
    }

    public async Task<Schedule?> GetScheduleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Schedules
            .Include(s => s.Rule)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<List<Schedule>> GetSchedulesAsync(bool? enabledOnly = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Schedules
            .Include(s => s.Rule)
            .AsNoTracking();

        if (enabledOnly.HasValue)
        {
            query = query.Where(s => s.IsEnabled == enabledOnly.Value);
        }

        return await query
            .OrderBy(s => s.NextRunAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Schedule> UpdateScheduleAsync(Schedule schedule, CancellationToken cancellationToken = default)
    {
        schedule.NextRunAt = CalculateNextRun(schedule.CronExpression);
        
        _context.Schedules.Update(schedule);
        await _context.SaveChangesAsync(cancellationToken);
        
        return schedule;
    }

    public async Task DeleteScheduleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var schedule = await _context.Schedules.FindAsync(new object[] { id }, cancellationToken);
        if (schedule != null)
        {
            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<ScheduleExecution> ExecuteScheduleAsync(Guid scheduleId, CancellationToken cancellationToken = default)
    {
        var schedule = await _context.Schedules
            .Include(s => s.Rule)
            .FirstOrDefaultAsync(s => s.Id == scheduleId, cancellationToken);
        
        if (schedule == null)
        {
            throw new InvalidOperationException($"Schedule {scheduleId} not found");
        }

        var execution = new ScheduleExecution
        {
            Id = Guid.NewGuid(),
            ScheduleId = scheduleId,
            StartedAt = DateTime.UtcNow,
            Status = ScheduleExecutionStatus.Running
        };

        _context.ScheduleExecutions.Add(execution);
        await _context.SaveChangesAsync(cancellationToken);

        _ = Task.Run(async () => await ExecuteScheduleInternalAsync(execution.Id, schedule), cancellationToken);

        return execution;
    }

    public async Task<List<ScheduleExecution>> GetScheduleExecutionsAsync(Guid scheduleId, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
    {
        return await _context.ScheduleExecutions
            .AsNoTracking()
            .Where(e => e.ScheduleId == scheduleId)
            .OrderByDescending(e => e.StartedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    private async Task ExecuteScheduleInternalAsync(Guid executionId, Schedule schedule)
    {
        var execution = await _context.ScheduleExecutions.FindAsync(executionId);
        if (execution == null) return;

        try
        {
            if (schedule.Type == ScheduleType.RuleExecution && schedule.RuleId.HasValue)
            {
                await _ruleService.ExecuteRuleAsync(schedule.RuleId.Value);
                execution.ResultSummary = "Rule executed successfully";
            }
            else if (schedule.Type == ScheduleType.DataProfiling)
            {
                await _profilingService.ProfileTableAsync(
                    schedule.WorkspaceId ?? "",
                    schedule.DatasetName ?? "",
                    schedule.TableName ?? "");
                execution.ResultSummary = "Profile created successfully";
            }

            execution.Status = ScheduleExecutionStatus.Completed;
            execution.CompletedAt = DateTime.UtcNow;

            schedule.LastRunAt = DateTime.UtcNow;
            schedule.NextRunAt = CalculateNextRun(schedule.CronExpression);
            _context.Schedules.Update(schedule);

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            execution.Status = ScheduleExecutionStatus.Failed;
            execution.ErrorMessage = ex.Message;
            execution.CompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    private DateTime? CalculateNextRun(string cronExpression)
    {
        try
        {
            var cron = CrontabSchedule.Parse(cronExpression);
            return cron.GetNextOccurrence(DateTime.UtcNow);
        }
        catch
        {
            return null;
        }
    }
}
