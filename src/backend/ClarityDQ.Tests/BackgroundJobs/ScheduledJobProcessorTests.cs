using ClarityDQ.Api.BackgroundJobs;
using ClarityDQ.Core.Entities;
using ClarityDQ.Core.Interfaces;
using Moq;

namespace ClarityDQ.Tests.BackgroundJobs;

public class ScheduledJobProcessorTests
{
    private readonly Mock<ISchedulingService> _schedulingServiceMock;
    private readonly ScheduledJobProcessor _processor;

    public ScheduledJobProcessorTests()
    {
        _schedulingServiceMock = new Mock<ISchedulingService>();
        _processor = new ScheduledJobProcessor(_schedulingServiceMock.Object);
    }

    [Fact]
    public async Task ProcessDueSchedules_CallsGetSchedules()
    {
        _schedulingServiceMock
            .Setup(s => s.GetSchedulesAsync(true, default))
            .ReturnsAsync(new List<Schedule>());

        await _processor.ProcessDueSchedules();

        _schedulingServiceMock.Verify(s => s.GetSchedulesAsync(true, default), Times.Once);
    }

    [Fact]
    public async Task ExecuteSchedule_CallsSchedulingService()
    {
        var scheduleId = Guid.NewGuid();
        var execution = new ScheduleExecution
        {
            Id = Guid.NewGuid(),
            ScheduleId = scheduleId,
            Status = ScheduleExecutionStatus.Running,
            StartedAt = DateTime.UtcNow
        };

        _schedulingServiceMock
            .Setup(s => s.ExecuteScheduleAsync(scheduleId, default))
            .ReturnsAsync(execution);

        await _processor.ExecuteSchedule(scheduleId);

        _schedulingServiceMock.Verify(s => s.ExecuteScheduleAsync(scheduleId, default), Times.Once);
    }
}
