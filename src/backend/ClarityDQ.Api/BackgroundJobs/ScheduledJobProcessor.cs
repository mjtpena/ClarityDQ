using Hangfire;
using ClarityDQ.Core.Interfaces;

namespace ClarityDQ.Api.BackgroundJobs;

public class ScheduledJobProcessor
{
    private readonly ISchedulingService _schedulingService;

    public ScheduledJobProcessor(ISchedulingService schedulingService)
    {
        _schedulingService = schedulingService;
    }

    public async Task ProcessDueSchedules()
    {
        var schedules = await _schedulingService.GetSchedulesAsync(enabledOnly: true);
        var now = DateTime.UtcNow;

        foreach (var schedule in schedules.Where(s => s.NextRunAt.HasValue && s.NextRunAt.Value <= now))
        {
            BackgroundJob.Enqueue(() => ExecuteSchedule(schedule.Id));
        }
    }

    public async Task ExecuteSchedule(Guid scheduleId)
    {
        await _schedulingService.ExecuteScheduleAsync(scheduleId);
    }
}
