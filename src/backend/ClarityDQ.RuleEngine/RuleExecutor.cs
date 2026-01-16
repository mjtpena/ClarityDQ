using ClarityDQ.Core.Entities;
using System.Text.RegularExpressions;

namespace ClarityDQ.RuleEngine;

public class RuleExecutor : IRuleExecutor
{
    public async Task<RuleExecutionResult> ExecuteAsync(Rule rule, IRuleDataSource dataSource, CancellationToken cancellationToken = default)
    {
        var result = new RuleExecutionResult();
        
        var data = await dataSource.GetDataAsync(rule.WorkspaceId, rule.DatasetName, rule.TableName, rule.ColumnName, cancellationToken);
        result.RecordsChecked = data.TotalRecords;

        switch (rule.Type)
        {
            case RuleType.Completeness:
                ExecuteCompletenessRule(rule, data, result);
                break;
            case RuleType.Uniqueness:
                ExecuteUniquenessRule(rule, data, result);
                break;
            case RuleType.Validity:
                ExecuteValidityRule(rule, data, result);
                break;
            case RuleType.Accuracy:
                ExecuteAccuracyRule(rule, data, result);
                break;
            case RuleType.Consistency:
                ExecuteConsistencyRule(rule, data, result);
                break;
            case RuleType.Custom:
                ExecuteCustomRule(rule, data, result);
                break;
        }

        result.RecordsPassed = result.RecordsChecked - result.RecordsFailed;
        result.SuccessRate = result.RecordsChecked > 0 
            ? (double)result.RecordsPassed / result.RecordsChecked * 100 
            : 0;

        return result;
    }

    private void ExecuteCompletenessRule(Rule rule, RuleDataSourceResult data, RuleExecutionResult result)
    {
        int rowIndex = 0;
        foreach (var row in data.Rows)
        {
            if (string.IsNullOrEmpty(rule.ColumnName))
            {
                var nullCount = row.Values.Count(v => v == null || (v is string s && string.IsNullOrWhiteSpace(s)));
                var completeness = 1.0 - ((double)nullCount / row.Count);
                
                if (completeness * 100 < rule.Threshold)
                {
                    result.RecordsFailed++;
                    result.Violations.Add(new RuleViolation
                    {
                        RowIndex = rowIndex,
                        RowData = row,
                        ViolationMessage = $"Row completeness {completeness * 100:F2}% below threshold {rule.Threshold}%"
                    });
                }
            }
            else if (row.TryGetValue(rule.ColumnName, out var value))
            {
                if (value == null || (value is string s && string.IsNullOrWhiteSpace(s)))
                {
                    result.RecordsFailed++;
                    result.Violations.Add(new RuleViolation
                    {
                        RowIndex = rowIndex,
                        RowData = row,
                        ViolationMessage = $"Column '{rule.ColumnName}' is null or empty"
                    });
                }
            }
            rowIndex++;
        }

        result.Metrics["CompletionRate"] = result.RecordsChecked > 0 
            ? (double)(result.RecordsChecked - result.RecordsFailed) / result.RecordsChecked * 100 
            : 0;
    }

    private void ExecuteUniquenessRule(Rule rule, RuleDataSourceResult data, RuleExecutionResult result)
    {
        var seen = new HashSet<object?>();
        var duplicates = new HashSet<object?>();
        int rowIndex = 0;

        foreach (var row in data.Rows)
        {
            if (row.TryGetValue(rule.ColumnName, out var value))
            {
                if (!seen.Add(value))
                {
                    if (duplicates.Add(value))
                    {
                        result.RecordsFailed++;
                        result.Violations.Add(new RuleViolation
                        {
                            RowIndex = rowIndex,
                            RowData = row,
                            ViolationMessage = $"Duplicate value found: {value}"
                        });
                    }
                }
            }
            rowIndex++;
        }

        result.Metrics["UniqueCount"] = seen.Count;
        result.Metrics["DuplicateCount"] = duplicates.Count;
    }

    private void ExecuteValidityRule(Rule rule, RuleDataSourceResult data, RuleExecutionResult result)
    {
        int rowIndex = 0;
        foreach (var row in data.Rows)
        {
            if (row.TryGetValue(rule.ColumnName, out var value))
            {
                bool isValid = value switch
                {
                    string s when rule.Expression.StartsWith("regex:") => 
                        Regex.IsMatch(s, rule.Expression[6..]),
                    string s when rule.Expression.StartsWith("length:") =>
                        ParseLengthRule(rule.Expression[7..], s.Length),
                    string s when rule.Expression.StartsWith("in:") =>
                        ParseInRule(rule.Expression[3..], s),
                    int i when rule.Expression.StartsWith("range:") =>
                        ParseRangeRule(rule.Expression[6..], i),
                    double d when rule.Expression.StartsWith("range:") =>
                        ParseRangeRule(rule.Expression[6..], d),
                    _ => true
                };

                if (!isValid)
                {
                    result.RecordsFailed++;
                    result.Violations.Add(new RuleViolation
                    {
                        RowIndex = rowIndex,
                        RowData = row,
                        ViolationMessage = $"Value '{value}' does not match validation rule: {rule.Expression}"
                    });
                }
            }
            rowIndex++;
        }

        result.Metrics["ValidationRule"] = rule.Expression;
    }

    private void ExecuteAccuracyRule(Rule rule, RuleDataSourceResult data, RuleExecutionResult result)
    {
        int rowIndex = 0;
        foreach (var row in data.Rows)
        {
            if (row.TryGetValue(rule.ColumnName, out var value))
            {
                if (value is double d && Math.Abs(d) > rule.Threshold)
                {
                    result.RecordsFailed++;
                    result.Violations.Add(new RuleViolation
                    {
                        RowIndex = rowIndex,
                        RowData = row,
                        ViolationMessage = $"Value {d} exceeds accuracy threshold {rule.Threshold}"
                    });
                }
            }
            rowIndex++;
        }
    }

    private void ExecuteConsistencyRule(Rule rule, RuleDataSourceResult data, RuleExecutionResult result)
    {
        if (rule.Expression.Contains("=="))
        {
            var parts = rule.Expression.Split("==").Select(p => p.Trim()).ToArray();
            if (parts.Length == 2)
            {
                int rowIndex = 0;
                foreach (var row in data.Rows)
                {
                    if (row.TryGetValue(parts[0], out var val1) && row.TryGetValue(parts[1], out var val2))
                    {
                        if (!Equals(val1, val2))
                        {
                            result.RecordsFailed++;
                            result.Violations.Add(new RuleViolation
                            {
                                RowIndex = rowIndex,
                                RowData = row,
                                ViolationMessage = $"Inconsistency: {parts[0]}={val1} != {parts[1]}={val2}"
                            });
                        }
                    }
                    rowIndex++;
                }
            }
        }
    }

    private void ExecuteCustomRule(Rule rule, RuleDataSourceResult data, RuleExecutionResult result)
    {
        result.Metrics["CustomExpression"] = rule.Expression;
        result.Metrics["Note"] = "Custom rule execution requires expression evaluator";
    }

    private bool ParseLengthRule(string expr, int length)
    {
        if (expr.StartsWith(">")) return length > int.Parse(expr[1..]);
        if (expr.StartsWith("<")) return length < int.Parse(expr[1..]);
        if (expr.StartsWith(">=")) return length >= int.Parse(expr[2..]);
        if (expr.StartsWith("<=")) return length <= int.Parse(expr[2..]);
        return length == int.Parse(expr);
    }

    private bool ParseInRule(string expr, string value)
    {
        var values = expr.Split(',').Select(v => v.Trim()).ToHashSet();
        return values.Contains(value);
    }

    private bool ParseRangeRule(string expr, double value)
    {
        var parts = expr.Split(',');
        if (parts.Length == 2)
        {
            var min = double.Parse(parts[0]);
            var max = double.Parse(parts[1]);
            return value >= min && value <= max;
        }
        return true;
    }
}
