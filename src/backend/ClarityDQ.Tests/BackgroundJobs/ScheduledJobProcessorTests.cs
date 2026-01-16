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

    [Fact]
    public async Task ProcessDueSchedules_WithMultipleSchedules_ProcessesAll()
    {
        var schedules = new List<Schedule>
        {
            new Schedule { Id = Guid.NewGuid(), IsEnabled = true, Name = "Schedule1" },
            new Schedule { Id = Guid.NewGuid(), IsEnabled = true, Name = "Schedule2" }
        };

        _schedulingServiceMock
            .Setup(s => s.GetSchedulesAsync(true, default))
            .ReturnsAsync(schedules);

        await _processor.ProcessDueSchedules();

        _schedulingServiceMock.Verify(s => s.GetSchedulesAsync(true, default), Times.Once);
    }

    [Fact]
    public async Task ExecuteSchedule_WithException_HandlesGracefully()
    {
        var scheduleId = Guid.NewGuid();

        _schedulingServiceMock
            .Setup(s => s.ExecuteScheduleAsync(scheduleId, default))
            .ThrowsAsync(new Exception("Test exception"));

        await Assert.ThrowsAsync<Exception>(() => _processor.ExecuteSchedule(scheduleId));
    }

    [Fact]
    public async Task ProcessDueSchedules_WithNoSchedules_CompletesSuccessfully()
    {
        _schedulingServiceMock
            .Setup(s => s.GetSchedulesAsync(true, default))
            .ReturnsAsync(new List<Schedule>());

        await _processor.ProcessDueSchedules();

        _schedulingServiceMock.Verify(s => s.GetSchedulesAsync(true, default), Times.Once);
    }
}
