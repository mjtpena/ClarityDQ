using ClarityDQ.Core.Entities;
using ClarityDQ.Core.Interfaces;
using ClarityDQ.Infrastructure.Data;
using ClarityDQ.RuleEngine;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ClarityDQ.Profiling.Services;

public class RuleService : IRuleService
{
    private readonly ClarityDbContext _context;
    private readonly IRuleExecutor _ruleExecutor;
    private readonly IRuleDataSource _dataSource;

    public RuleService(ClarityDbContext context, IRuleExecutor ruleExecutor, IRuleDataSource dataSource)
    {
        _context = context;
        _ruleExecutor = ruleExecutor;
        _dataSource = dataSource;
    }

    public async Task<Rule> CreateRuleAsync(Rule rule, CancellationToken cancellationToken = default)
    {
        rule.Id = Guid.NewGuid();
        rule.CreatedAt = DateTime.UtcNow;
        
        _context.Rules.Add(rule);
        await _context.SaveChangesAsync(cancellationToken);
        
        return rule;
    }

    public async Task<Rule?> GetRuleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Rules
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<List<Rule>> GetRulesAsync(string workspaceId, bool? enabledOnly = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Rules
            .AsNoTracking()
            .Where(r => r.WorkspaceId == workspaceId);

        if (enabledOnly.HasValue)
        {
            query = query.Where(r => r.IsEnabled == enabledOnly.Value);
        }

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Rule> UpdateRuleAsync(Rule rule, CancellationToken cancellationToken = default)
    {
        rule.UpdatedAt = DateTime.UtcNow;
        
        _context.Rules.Update(rule);
        await _context.SaveChangesAsync(cancellationToken);
        
        return rule;
    }

    public async Task DeleteRuleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var rule = await _context.Rules.FindAsync(new object[] { id }, cancellationToken);
        if (rule != null)
        {
            _context.Rules.Remove(rule);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<RuleExecution> ExecuteRuleAsync(Guid ruleId, CancellationToken cancellationToken = default)
    {
        var rule = await _context.Rules.FindAsync(new object[] { ruleId }, cancellationToken);
        if (rule == null)
        {
            throw new InvalidOperationException($"Rule {ruleId} not found");
        }

        var execution = new RuleExecution
        {
            Id = Guid.NewGuid(),
            RuleId = ruleId,
            ExecutedAt = DateTime.UtcNow,
            Status = RuleExecutionStatus.Running
        };

        _context.RuleExecutions.Add(execution);
        await _context.SaveChangesAsync(cancellationToken);

        // Execute rule asynchronously
        _ = Task.Run(async () => await ExecuteRuleInternalAsync(execution.Id, rule), cancellationToken);

        return execution;
    }

    public async Task<List<RuleExecution>> GetRuleExecutionsAsync(Guid ruleId, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
    {
        return await _context.RuleExecutions
            .AsNoTracking()
            .Where(e => e.RuleId == ruleId)
            .OrderByDescending(e => e.ExecutedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    private async Task ExecuteRuleInternalAsync(Guid executionId, Rule rule)
    {
        var stopwatch = Stopwatch.StartNew();
        var execution = await _context.RuleExecutions.FindAsync(executionId);
        if (execution == null) return;

        try
        {
            var result = await _ruleExecutor.ExecuteAsync(rule, _dataSource);

            execution.Status = RuleExecutionStatus.Completed;
            execution.RecordsChecked = result.RecordsChecked;
            execution.RecordsPassed = result.RecordsPassed;
            execution.RecordsFailed = result.RecordsFailed;
            execution.SuccessRate = result.SuccessRate;
            execution.ResultDetails = System.Text.Json.JsonSerializer.Serialize(new
            {
                Violations = result.Violations.Take(10),
                Metrics = result.Metrics
            });
            execution.DurationMs = (int)stopwatch.ElapsedMilliseconds;

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            execution.Status = RuleExecutionStatus.Failed;
            execution.ErrorMessage = ex.Message;
            execution.DurationMs = (int)stopwatch.ElapsedMilliseconds;
            await _context.SaveChangesAsync();
        }
    }
}
