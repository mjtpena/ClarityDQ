import { useState } from 'react';
import {
  Dialog,
  DialogSurface,
  DialogTitle,
  DialogBody,
  DialogActions,
  DialogContent,
  Button,
  Input,
  Textarea,
  Dropdown,
  Option,
  Switch,
  Field,
} from '@fluentui/react-components';
import { RuleType, RuleSeverity, CreateRuleRequest } from '../../types/rule';

interface RuleFormProps {
  open: boolean;
  workspaceId: string;
  onClose: () => void;
  onSubmit: (rule: CreateRuleRequest) => void;
}

export const RuleForm = ({ open, workspaceId, onClose, onSubmit }: RuleFormProps) => {
  const [formData, setFormData] = useState<CreateRuleRequest>({
    name: '',
    description: '',
    type: RuleType.Completeness,
    workspaceId,
    datasetName: '',
    tableName: '',
    columnName: '',
    expression: '',
    threshold: 95,
    severity: RuleSeverity.Medium,
    isEnabled: true,
  });

  const handleSubmit = () => {
    onSubmit(formData);
    onClose();
  };

  const ruleTypeOptions = [
    { key: RuleType.Completeness, text: 'Completeness' },
    { key: RuleType.Accuracy, text: 'Accuracy' },
    { key: RuleType.Consistency, text: 'Consistency' },
    { key: RuleType.Uniqueness, text: 'Uniqueness' },
    { key: RuleType.Validity, text: 'Validity' },
    { key: RuleType.Custom, text: 'Custom' },
  ];

  const severityOptions = [
    { key: RuleSeverity.Low, text: 'Low' },
    { key: RuleSeverity.Medium, text: 'Medium' },
    { key: RuleSeverity.High, text: 'High' },
    { key: RuleSeverity.Critical, text: 'Critical' },
  ];

  return (
    <Dialog open={open} onOpenChange={(_, data) => !data.open && onClose()}>
      <DialogSurface>
        <DialogBody>
          <DialogTitle>Create Quality Rule</DialogTitle>
          <DialogContent>
            <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
              <Field label="Rule Name" required>
                <Input
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                />
              </Field>
              <Field label="Description">
                <Textarea
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  rows={3}
                />
              </Field>
              <Field label="Dataset">
                <Input
                  value={formData.datasetName}
                  onChange={(e) => setFormData({ ...formData, datasetName: e.target.value })}
                />
              </Field>
              <Field label="Table">
                <Input
                  value={formData.tableName}
                  onChange={(e) => setFormData({ ...formData, tableName: e.target.value })}
                />
              </Field>
              <Field label="Expression">
                <Textarea
                  value={formData.expression}
                  onChange={(e) => setFormData({ ...formData, expression: e.target.value })}
                  rows={2}
                />
              </Field>
              <Field label="Enabled">
                <Switch
                  checked={formData.isEnabled}
                  onChange={(e) => setFormData({ ...formData, isEnabled: e.currentTarget.checked })}
                />
              </Field>
            </div>
          </DialogContent>
          <DialogActions>
            <Button appearance="secondary" onClick={onClose}>Cancel</Button>
            <Button appearance="primary" onClick={handleSubmit}>Create</Button>
          </DialogActions>
        </DialogBody>
      </DialogSurface>
    </Dialog>
  );
};
