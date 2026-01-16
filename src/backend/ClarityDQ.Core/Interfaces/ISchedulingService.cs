using ClarityDQ.Core.Entities;

namespace ClarityDQ.Core.Interfaces;

public interface ISchedulingService
{
    Task<Schedule> CreateScheduleAsync(Schedule schedule, CancellationToken cancellationToken = default);
    Task<Schedule?> GetScheduleAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Schedule>> GetSchedulesAsync(bool? enabledOnly = null, CancellationToken cancellationToken = default);
    Task<Schedule> UpdateScheduleAsync(Schedule schedule, CancellationToken cancellationToken = default);
    Task DeleteScheduleAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ScheduleExecution> ExecuteScheduleAsync(Guid scheduleId, CancellationToken cancellationToken = default);
    Task<List<ScheduleExecution>> GetScheduleExecutionsAsync(Guid scheduleId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
}
