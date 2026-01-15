export interface DataProfile {
  id: string;
  workspaceId: string;
  datasetName: string;
  tableName: string;
  profiledAt: string;
  rowCount: number;
  columnCount: number;
  sizeInBytes: number;
  profileData: string;
  errorMessage?: string;
  status: ProfileStatus;
}

export enum ProfileStatus {
  Pending = 0,
  InProgress = 1,
  Completed = 2,
  Failed = 3,
}

export interface ProfileRequest {
  workspaceId: string;
  datasetName: string;
  tableName: string;
}
