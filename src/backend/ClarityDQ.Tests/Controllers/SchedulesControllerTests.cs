using ClarityDQ.Api.Controllers;
using ClarityDQ.Core.Entities;
using ClarityDQ.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace ClarityDQ.Tests.Controllers;

public class SchedulesControllerTests
{
    private readonly Mock<ISchedulingService> _schedulingServiceMock;
    private readonly Mock<ILogger<SchedulesController>> _loggerMock;
    private readonly SchedulesController _controller;

    public SchedulesControllerTests()
    {
        _schedulingServiceMock = new Mock<ISchedulingService>();
        _loggerMock = new Mock<ILogger<SchedulesController>>();
        _controller = new SchedulesController(_schedulingServiceMock.Object, _loggerMock.Object);
        
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "testuser")
        }, "mock"));
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task CreateSchedule_ReturnsCreatedResult()
    {
        var request = new CreateScheduleRequest(
            "Daily Rule",
            ScheduleType.RuleExecution,
            Guid.NewGuid(),
            "workspace1",
            "dataset1",
            "table1",
            "0 0 * * *",
            true);

        var expectedSchedule = new Schedule
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Type = request.Type,
            RuleId = request.RuleId,
            WorkspaceId = request.WorkspaceId,
            DatasetName = request.DatasetName,
            TableName = request.TableName,
            CronExpression = request.CronExpression,
            IsEnabled = request.IsEnabled,
            CreatedBy = "testuser"
        };

        _schedulingServiceMock
            .Setup(s => s.CreateScheduleAsync(It.IsAny<Schedule>(), default))
            .ReturnsAsync(expectedSchedule);

        var result = await _controller.CreateSchedule(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var schedule = Assert.IsType<Schedule>(createdResult.Value);
        Assert.Equal(expectedSchedule.Id, schedule.Id);
        Assert.Equal(expectedSchedule.Name, schedule.Name);
    }

    [Fact]
    public async Task GetSchedule_ReturnsSchedule_WhenExists()
    {
        var scheduleId = Guid.NewGuid();
        var expectedSchedule = new Schedule { Id = scheduleId, Name = "Test Schedule" };

        _schedulingServiceMock
            .Setup(s => s.GetScheduleAsync(scheduleId, default))
            .ReturnsAsync(expectedSchedule);

        var result = await _controller.GetSchedule(scheduleId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var schedule = Assert.IsType<Schedule>(okResult.Value);
        Assert.Equal(scheduleId, schedule.Id);
    }

    [Fact]
    public async Task GetSchedule_ReturnsNotFound_WhenDoesNotExist()
    {
        var scheduleId = Guid.NewGuid();

        _schedulingServiceMock
            .Setup(s => s.GetScheduleAsync(scheduleId, default))
            .ReturnsAsync((Schedule?)null);

        var result = await _controller.GetSchedule(scheduleId);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetSchedules_ReturnsAllSchedules()
    {
        var schedules = new List<Schedule>
        {
            new() { Id = Guid.NewGuid(), Name = "Schedule 1" },
            new() { Id = Guid.NewGuid(), Name = "Schedule 2" }
        };

        _schedulingServiceMock
            .Setup(s => s.GetSchedulesAsync(null, default))
            .ReturnsAsync(schedules);

        var result = await _controller.GetSchedules();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedSchedules = Assert.IsType<List<Schedule>>(okResult.Value);
        Assert.Equal(2, returnedSchedules.Count);
    }

    [Fact]
    public async Task GetSchedules_WithEnabledFilter_ReturnsFilteredSchedules()
    {
        var schedules = new List<Schedule>
        {
            new() { Id = Guid.NewGuid(), Name = "Enabled Schedule", IsEnabled = true }
        };

        _schedulingServiceMock
            .Setup(s => s.GetSchedulesAsync(true, default))
            .ReturnsAsync(schedules);

        var result = await _controller.GetSchedules(enabledOnly: true);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedSchedules = Assert.IsType<List<Schedule>>(okResult.Value);
        Assert.Single(returnedSchedules);
        Assert.True(returnedSchedules[0].IsEnabled);
    }

    [Fact]
    public async Task UpdateSchedule_ReturnsUpdatedSchedule()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = new Schedule
        {
            Id = scheduleId,
            Name = "Old Name",
            CronExpression = "0 0 * * *",
            IsEnabled = true
        };

        var updateRequest = new UpdateScheduleRequest("New Name", "0 12 * * *", false);

        _schedulingServiceMock
            .Setup(s => s.GetScheduleAsync(scheduleId, default))
            .ReturnsAsync(existingSchedule);

        _schedulingServiceMock
            .Setup(s => s.UpdateScheduleAsync(It.IsAny<Schedule>(), default))
            .ReturnsAsync((Schedule s, CancellationToken _) => s);

        var result = await _controller.UpdateSchedule(scheduleId, updateRequest);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var schedule = Assert.IsType<Schedule>(okResult.Value);
        Assert.Equal("New Name", schedule.Name);
        Assert.Equal("0 12 * * *", schedule.CronExpression);
        Assert.False(schedule.IsEnabled);
    }

    [Fact]
    public async Task UpdateSchedule_ReturnsNotFound_WhenScheduleDoesNotExist()
    {
        var scheduleId = Guid.NewGuid();
        var updateRequest = new UpdateScheduleRequest("Name", "0 0 * * *", true);

        _schedulingServiceMock
            .Setup(s => s.GetScheduleAsync(scheduleId, default))
            .ReturnsAsync((Schedule?)null);

        var result = await _controller.UpdateSchedule(scheduleId, updateRequest);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task DeleteSchedule_ReturnsNoContent()
    {
        var scheduleId = Guid.NewGuid();

        _schedulingServiceMock
            .Setup(s => s.DeleteScheduleAsync(scheduleId, default))
            .Returns(Task.CompletedTask);

        var result = await _controller.DeleteSchedule(scheduleId);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task ExecuteSchedule_ReturnsExecution()
    {
        var scheduleId = Guid.NewGuid();
        var execution = new ScheduleExecution
        {
            Id = Guid.NewGuid(),
            ScheduleId = scheduleId,
            Status = ScheduleExecutionStatus.Running
        };

        _schedulingServiceMock
            .Setup(s => s.ExecuteScheduleAsync(scheduleId, default))
            .ReturnsAsync(execution);

        var result = await _controller.ExecuteSchedule(scheduleId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedExecution = Assert.IsType<ScheduleExecution>(okResult.Value);
        Assert.Equal(scheduleId, returnedExecution.ScheduleId);
    }

    [Fact]
    public async Task ExecuteSchedule_ReturnsNotFound_WhenScheduleDoesNotExist()
    {
        var scheduleId = Guid.NewGuid();

        _schedulingServiceMock
            .Setup(s => s.ExecuteScheduleAsync(scheduleId, default))
            .ThrowsAsync(new InvalidOperationException("Schedule not found"));

        var result = await _controller.ExecuteSchedule(scheduleId);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetScheduleExecutions_ReturnsExecutions()
    {
        var scheduleId = Guid.NewGuid();
        var executions = new List<ScheduleExecution>
        {
            new() { Id = Guid.NewGuid(), ScheduleId = scheduleId },
            new() { Id = Guid.NewGuid(), ScheduleId = scheduleId }
        };

        _schedulingServiceMock
            .Setup(s => s.GetScheduleExecutionsAsync(scheduleId, 0, 50, default))
            .ReturnsAsync(executions);

        var result = await _controller.GetScheduleExecutions(scheduleId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedExecutions = Assert.IsType<List<ScheduleExecution>>(okResult.Value);
        Assert.Equal(2, returnedExecutions.Count);
    }

    [Fact]
    public async Task GetScheduleExecutions_WithPagination_ReturnsPagedResults()
    {
        var scheduleId = Guid.NewGuid();
        var executions = new List<ScheduleExecution>
        {
            new() { Id = Guid.NewGuid(), ScheduleId = scheduleId }
        };

        _schedulingServiceMock
            .Setup(s => s.GetScheduleExecutionsAsync(scheduleId, 10, 20, default))
            .ReturnsAsync(executions);

        var result = await _controller.GetScheduleExecutions(scheduleId, skip: 10, take: 20);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedExecutions = Assert.IsType<List<ScheduleExecution>>(okResult.Value);
        Assert.Single(returnedExecutions);
    }
}
