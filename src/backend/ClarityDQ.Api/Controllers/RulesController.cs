using ClarityDQ.Core.Entities;
using ClarityDQ.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClarityDQ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RulesController : ControllerBase
{
    private readonly IRuleService _ruleService;
    private readonly ILogger<RulesController> _logger;

    public RulesController(IRuleService ruleService, ILogger<RulesController> logger)
    {
        _ruleService = ruleService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<Rule>> CreateRule([FromBody] CreateRuleRequest request)
    {
        _logger.LogInformation("Creating rule: {RuleName}", request.Name);

        var rule = new Rule
        {
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            WorkspaceId = request.WorkspaceId,
            DatasetName = request.DatasetName,
            TableName = request.TableName,
            ColumnName = request.ColumnName ?? string.Empty,
            Expression = request.Expression,
            Threshold = request.Threshold,
            Severity = request.Severity,
            IsEnabled = request.IsEnabled,
            CreatedBy = User.Identity?.Name ?? "unknown"
        };

        var created = await _ruleService.CreateRuleAsync(rule);
        return CreatedAtAction(nameof(GetRule), new { id = created.Id }, created);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Rule>> GetRule(Guid id)
    {
        var rule = await _ruleService.GetRuleAsync(id);
        if (rule == null)
            return NotFound();

        return Ok(rule);
    }

    [HttpGet("workspace/{workspaceId}")]
    public async Task<ActionResult<List<Rule>>> GetRules(
        string workspaceId,
        [FromQuery] bool? enabledOnly = null)
    {
        var rules = await _ruleService.GetRulesAsync(workspaceId, enabledOnly);
        return Ok(rules);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Rule>> UpdateRule(Guid id, [FromBody] UpdateRuleRequest request)
    {
        var existing = await _ruleService.GetRuleAsync(id);
        if (existing == null)
            return NotFound();

        existing.Name = request.Name;
        existing.Description = request.Description;
        existing.Expression = request.Expression;
        existing.Threshold = request.Threshold;
        existing.Severity = request.Severity;
        existing.IsEnabled = request.IsEnabled;

        var updated = await _ruleService.UpdateRuleAsync(existing);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRule(Guid id)
    {
        await _ruleService.DeleteRuleAsync(id);
        return NoContent();
    }

    [HttpPost("{id}/execute")]
    public async Task<ActionResult<RuleExecution>> ExecuteRule(Guid id)
    {
        try
        {
            var execution = await _ruleService.ExecuteRuleAsync(id);
            return Ok(execution);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("{id}/executions")]
    public async Task<ActionResult<List<RuleExecution>>> GetRuleExecutions(
        Guid id,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50)
    {
        var executions = await _ruleService.GetRuleExecutionsAsync(id, skip, take);
        return Ok(executions);
    }
}

public record CreateRuleRequest(
    string Name,
    string Description,
    RuleType Type,
    string WorkspaceId,
    string DatasetName,
    string TableName,
    string? ColumnName,
    string Expression,
    double Threshold,
    RuleSeverity Severity,
    bool IsEnabled);

public record UpdateRuleRequest(
    string Name,
    string Description,
    string Expression,
    double Threshold,
    RuleSeverity Severity,
    bool IsEnabled);
