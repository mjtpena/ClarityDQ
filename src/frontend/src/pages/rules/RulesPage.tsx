import { useState, useEffect } from 'react';
import { Button, Text, Spinner } from '@fluentui/react-components';
import { Add20Regular } from '@fluentui/react-icons';
import { RuleForm } from '../../components/rules/RuleForm';
import { RuleList } from '../../components/rules/RuleList';
import { useRuleService } from '../../services/rules';
import { Rule, CreateRuleRequest } from '../../types/rule';

interface RulesPageProps {
  workspaceId: string;
}

export const RulesPage = ({ workspaceId }: RulesPageProps) => {
  const [rules, setRules] = useState<Rule[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const { createRule, getRules, deleteRule, executeRule } = useRuleService();

  const loadRules = async () => {
    try {
      setLoading(true);
      const data = await getRules(workspaceId);
      setRules(data);
    } catch (error) {
      console.error('Failed to load rules:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadRules();
  }, [workspaceId]);

  const handleCreateRule = async (request: CreateRuleRequest) => {
    try {
      await createRule(request);
      await loadRules();
    } catch (error) {
      console.error('Failed to create rule:', error);
    }
  };

  const handleExecuteRule = async (ruleId: string) => {
    try {
      await executeRule(ruleId);
      alert('Rule execution started!');
    } catch (error) {
      console.error('Failed to execute rule:', error);
    }
  };

  if (loading) {
    return <Spinner label="Loading rules..." />;
  }

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '24px' }}>
        <Text as="h2" size={600}>Quality Rules</Text>
        <Button icon={<Add20Regular />} onClick={() => setShowForm(true)}>Create Rule</Button>
      </div>
      <RuleList rules={rules} onExecute={handleExecuteRule} onEdit={() => {}} onDelete={() => {}} />
      <RuleForm open={showForm} workspaceId={workspaceId} onClose={() => setShowForm(false)} onSubmit={handleCreateRule} />
    </div>
  );
};
