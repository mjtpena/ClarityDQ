import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { RuleList } from '../rules/RuleList';
import { Rule, RuleType, RuleSeverity } from '../../types/rule';
import { FluentProvider, webLightTheme } from '@fluentui/react-components';

const renderWithProvider = (component: React.ReactElement) => {
  return render(
    <FluentProvider theme={webLightTheme}>
      {component}
    </FluentProvider>
  );
};

describe('RuleList', () => {
  const mockRules: Rule[] = [
    {
      id: '1',
      name: 'Test Rule 1',
      description: 'Test description',
      type: RuleType.Completeness,
      severity: RuleSeverity.High,
      workspaceId: 'ws1',
      datasetName: 'dataset1',
      tableName: 'table1',
      columnName: 'col1',
      threshold: 95,
      isEnabled: true,
      createdAt: new Date().toISOString(),
      createdBy: 'test@user.com'
    },
    {
      id: '2',
      name: 'Test Rule 2',
      description: 'Test description 2',
      type: RuleType.Uniqueness,
      severity: RuleSeverity.Medium,
      workspaceId: 'ws1',
      datasetName: 'dataset2',
      tableName: 'table2',
      columnName: 'col2',
      threshold: 100,
      isEnabled: false,
      createdAt: new Date().toISOString(),
      createdBy: 'test@user.com'
    }
  ];

  it('should render rules list', () => {
    const mockExecute = vi.fn();
    const mockEdit = vi.fn();
    const mockDelete = vi.fn();

    renderWithProvider(
      <RuleList 
        rules={mockRules} 
        onExecute={mockExecute}
        onEdit={mockEdit}
        onDelete={mockDelete}
      />
    );

    expect(screen.getByText('Test Rule 1')).toBeDefined();
    expect(screen.getByText('Test Rule 2')).toBeDefined();
  });

  it('should show empty state when no rules', () => {
    const mockExecute = vi.fn();
    const mockEdit = vi.fn();
    const mockDelete = vi.fn();

    renderWithProvider(
      <RuleList 
        rules={[]} 
        onExecute={mockExecute}
        onEdit={mockEdit}
        onDelete={mockDelete}
      />
    );

    expect(screen.getByText('No rules found')).toBeDefined();
  });

  it('should display dataset and table names', () => {
    const mockExecute = vi.fn();
    const mockEdit = vi.fn();
    const mockDelete = vi.fn();

    renderWithProvider(
      <RuleList 
        rules={mockRules} 
        onExecute={mockExecute}
        onEdit={mockEdit}
        onDelete={mockDelete}
      />
    );

    expect(screen.getByText('dataset1.table1')).toBeDefined();
    expect(screen.getByText('dataset2.table2')).toBeDefined();
  });
});
