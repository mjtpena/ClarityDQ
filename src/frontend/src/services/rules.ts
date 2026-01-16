import { Rule, RuleExecution, CreateRuleRequest, UpdateRuleRequest } from '../types/rule';
import { useApi } from './api';

export const useRuleService = () => {
  const { fetchWithAuth } = useApi();

  const createRule = async (request: CreateRuleRequest): Promise<Rule> => {
    return await fetchWithAuth('/api/rules', {
      method: 'POST',
      body: JSON.stringify(request),
    });
  };

  const getRule = async (id: string): Promise<Rule> => {
    return await fetchWithAuth(`/api/rules/${id}`);
  };

  const getRules = async (workspaceId: string, enabledOnly?: boolean): Promise<Rule[]> => {
    const params = enabledOnly !== undefined ? `?enabledOnly=${enabledOnly}` : '';
    return await fetchWithAuth(`/api/rules/workspace/${workspaceId}${params}`);
  };

  const updateRule = async (id: string, request: UpdateRuleRequest): Promise<Rule> => {
    return await fetchWithAuth(`/api/rules/${id}`, {
      method: 'PUT',
      body: JSON.stringify(request),
    });
  };

  const deleteRule = async (id: string): Promise<void> => {
    await fetchWithAuth(`/api/rules/${id}`, {
      method: 'DELETE',
    });
  };

  const executeRule = async (id: string): Promise<RuleExecution> => {
    return await fetchWithAuth(`/api/rules/${id}/execute`, {
      method: 'POST',
    });
  };

  const getRuleExecutions = async (id: string, skip = 0, take = 50): Promise<RuleExecution[]> => {
    return await fetchWithAuth(`/api/rules/${id}/executions?skip=${skip}&take=${take}`);
  };

  return { createRule, getRule, getRules, updateRule, deleteRule, executeRule, getRuleExecutions };
};
