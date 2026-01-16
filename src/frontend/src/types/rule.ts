export enum RuleType {
  Completeness = 0,
  Accuracy = 1,
  Consistency = 2,
  Uniqueness = 3,
  Validity = 4,
  Custom = 5,
}

export enum RuleSeverity {
  Low = 0,
  Medium = 1,
  High = 2,
  Critical = 3,
}

export enum RuleExecutionStatus {
  Pending = 0,
  Running = 1,
  Completed = 2,
  Failed = 3,
  Skipped = 4,
}

export interface Rule {
  id: string;
  name: string;
  description: string;
  type: RuleType;
  workspaceId: string;
  datasetName: string;
  tableName: string;
  columnName?: string;
  expression: string;
  threshold: number;
  severity: RuleSeverity;
  isEnabled: boolean;
  createdAt: string;
  updatedAt?: string;
  createdBy: string;
}

export interface RuleExecution {
  id: string;
  ruleId: string;
  executedAt: string;
  status: RuleExecutionStatus;
  recordsChecked: number;
  recordsPassed: number;
  recordsFailed: number;
  successRate: number;
  resultDetails?: string;
  errorMessage?: string;
  durationMs: number;
}

export interface CreateRuleRequest {
  name: string;
  description: string;
  type: RuleType;
  workspaceId: string;
  datasetName: string;
  tableName: string;
  columnName?: string;
  expression: string;
  threshold: number;
  severity: RuleSeverity;
  isEnabled: boolean;
}

export interface UpdateRuleRequest {
  name: string;
  description: string;
  expression: string;
  threshold: number;
  severity: RuleSeverity;
  isEnabled: boolean;
}
