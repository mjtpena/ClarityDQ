export enum ScheduleType {
  RuleExecution = 0,
  DataProfiling = 1,
}

export enum ScheduleExecutionStatus {
  Running = 0,
  Completed = 1,
  Failed = 2,
}

export interface Schedule {
  id: string;
  name: string;
  type: ScheduleType;
  ruleId?: string;
  workspaceId?: string;
  datasetName?: string;
  tableName?: string;
  cronExpression: string;
  isEnabled: boolean;
  lastRunAt?: string;
  nextRunAt?: string;
  createdAt: string;
  createdBy: string;
}

export interface ScheduleExecution {
  id: string;
  scheduleId: string;
  startedAt: string;
  completedAt?: string;
  status: ScheduleExecutionStatus;
  resultSummary?: string;
  errorMessage?: string;
}

export interface CreateScheduleRequest {
  name: string;
  type: ScheduleType;
  ruleId?: string;
  workspaceId?: string;
  datasetName?: string;
  tableName?: string;
  cronExpression: string;
  isEnabled: boolean;
}
