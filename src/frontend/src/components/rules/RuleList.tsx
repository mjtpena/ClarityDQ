import { Card, Text, Button, Badge } from '@fluentui/react-components';
import { Play20Regular } from '@fluentui/react-icons';
import { Rule, RuleType, RuleSeverity } from '../../types/rule';

interface RuleListProps {
  rules: Rule[];
  onExecute: (ruleId: string) => void;
  onEdit: (rule: Rule) => void;
  onDelete: (ruleId: string) => void;
}

export const RuleList = ({ rules, onExecute }: RuleListProps) => {
  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
      {rules.map((rule) => (
        <Card key={rule.id} style={{ padding: '16px' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between' }}>
            <div>
              <Text weight="semibold">{rule.name}</Text>
              <Text size={300} style={{ color: '#666', display: 'block' }}>
                {rule.datasetName}.{rule.tableName}
              </Text>
            </div>
            <Button icon={<Play20Regular />} onClick={() => onExecute(rule.id)}>
              Execute
            </Button>
          </div>
        </Card>
      ))}
      {rules.length === 0 && (
        <Card style={{ padding: '40px', textAlign: 'center' }}>
          <Text>No rules found</Text>
        </Card>
      )}
    </div>
  );
};
