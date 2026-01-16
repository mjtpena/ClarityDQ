using ClarityDQ.Core.Entities;
using ClarityDQ.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClarityDQ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SchedulesController : ControllerBase
{
    private readonly ISchedulingService _schedulingService;
    private readonly ILogger<SchedulesController> _logger;

    public SchedulesController(ISchedulingService schedulingService, ILogger<SchedulesController> logger)
    {
        _schedulingService = schedulingService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<Schedule>> CreateSchedule([FromBody] CreateScheduleRequest request)
    {
        _logger.LogInformation("Creating schedule: {ScheduleName}", request.Name);

        var schedule = new Schedule
        {
            Name = request.Name,
            Type = request.Type,
            RuleId = request.RuleId,
            WorkspaceId = request.WorkspaceId,
            DatasetName = request.DatasetName,
            TableName = request.TableName,
            CronExpression = request.CronExpression,
            IsEnabled = request.IsEnabled,
            CreatedBy = User.Identity?.Name ?? "unknown"
        };

        var created = await _schedulingService.CreateScheduleAsync(schedule);
        return CreatedAtAction(nameof(GetSchedule), new { id = created.Id }, created);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Schedule>> GetSchedule(Guid id)
    {
        var schedule = await _schedulingService.GetScheduleAsync(id);
        if (schedule == null)
            return NotFound();

        return Ok(schedule);
    }

    [HttpGet]
    public async Task<ActionResult<List<Schedule>>> GetSchedules([FromQuery] bool? enabledOnly = null)
    {
        var schedules = await _schedulingService.GetSchedulesAsync(enabledOnly);
        return Ok(schedules);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Schedule>> UpdateSchedule(Guid id, [FromBody] UpdateScheduleRequest request)
    {
        var existing = await _schedulingService.GetScheduleAsync(id);
        if (existing == null)
            return NotFound();

        existing.Name = request.Name;
        existing.CronExpression = request.CronExpression;
        existing.IsEnabled = request.IsEnabled;

        var updated = await _schedulingService.UpdateScheduleAsync(existing);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSchedule(Guid id)
    {
        await _schedulingService.DeleteScheduleAsync(id);
        return NoContent();
    }

    [HttpPost("{id}/execute")]
    public async Task<ActionResult<ScheduleExecution>> ExecuteSchedule(Guid id)
    {
        try
        {
            var execution = await _schedulingService.ExecuteScheduleAsync(id);
            return Ok(execution);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("{id}/executions")]
    public async Task<ActionResult<List<ScheduleExecution>>> GetScheduleExecutions(
        Guid id,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50)
    {
        var executions = await _schedulingService.GetScheduleExecutionsAsync(id, skip, take);
        return Ok(executions);
    }
}

public record CreateScheduleRequest(
    string Name,
    ScheduleType Type,
    Guid? RuleId,
    string? WorkspaceId,
    string? DatasetName,
    string? TableName,
    string CronExpression,
    bool IsEnabled);

public record UpdateScheduleRequest(
    string Name,
    string CronExpression,
    bool IsEnabled);
