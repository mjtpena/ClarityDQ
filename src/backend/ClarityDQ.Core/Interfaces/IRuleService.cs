using ClarityDQ.Core.Entities;

namespace ClarityDQ.Core.Interfaces;

public interface IRuleService
{
    Task<Rule> CreateRuleAsync(Rule rule, CancellationToken cancellationToken = default);
    Task<Rule?> GetRuleAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Rule>> GetRulesAsync(string workspaceId, bool? enabledOnly = null, CancellationToken cancellationToken = default);
    Task<Rule> UpdateRuleAsync(Rule rule, CancellationToken cancellationToken = default);
    Task DeleteRuleAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RuleExecution> ExecuteRuleAsync(Guid ruleId, CancellationToken cancellationToken = default);
    Task<List<RuleExecution>> GetRuleExecutionsAsync(Guid ruleId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
}
