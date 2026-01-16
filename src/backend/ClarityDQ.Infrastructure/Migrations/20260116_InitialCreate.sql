-- ClarityDQ Initial Schema Migration
-- Generated: 2026-01-16

CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    EntraIdObjectId NVARCHAR(100) NOT NULL UNIQUE,
    Email NVARCHAR(255) NOT NULL,
    DisplayName NVARCHAR(255) NOT NULL,
    Role INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    LastLoginAt DATETIME2 NULL
);

CREATE INDEX IX_Users_Email ON Users(Email);

CREATE TABLE DataProfiles (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    WorkspaceId NVARCHAR(100) NOT NULL,
    DatasetName NVARCHAR(255) NOT NULL,
    TableName NVARCHAR(255) NOT NULL,
    ProfiledAt DATETIME2 NOT NULL,
    RowCount BIGINT NOT NULL DEFAULT 0,
    ColumnCount INT NOT NULL DEFAULT 0,
    SizeInBytes BIGINT NOT NULL DEFAULT 0,
    ProfileData NVARCHAR(MAX) NULL,
    Status INT NOT NULL DEFAULT 0,
    ErrorMessage NVARCHAR(MAX) NULL
);

CREATE INDEX IX_DataProfiles_Workspace ON DataProfiles(WorkspaceId, DatasetName, TableName);
CREATE INDEX IX_DataProfiles_ProfiledAt ON DataProfiles(ProfiledAt);

CREATE TABLE Rules (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    Type INT NOT NULL,
    WorkspaceId NVARCHAR(100) NOT NULL,
    DatasetName NVARCHAR(255) NOT NULL,
    TableName NVARCHAR(255) NOT NULL,
    ColumnName NVARCHAR(255) NULL,
    Expression NVARCHAR(MAX) NOT NULL,
    Threshold FLOAT NOT NULL,
    Severity INT NOT NULL,
    IsEnabled BIT NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL,
    CreatedBy NVARCHAR(255) NOT NULL
);

CREATE INDEX IX_Rules_Workspace ON Rules(WorkspaceId, DatasetName, TableName);
CREATE INDEX IX_Rules_IsEnabled ON Rules(IsEnabled);

CREATE TABLE RuleExecutions (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    RuleId UNIQUEIDENTIFIER NOT NULL,
    ExecutedAt DATETIME2 NOT NULL,
    Status INT NOT NULL,
    RecordsChecked BIGINT NOT NULL DEFAULT 0,
    RecordsPassed BIGINT NOT NULL DEFAULT 0,
    RecordsFailed BIGINT NOT NULL DEFAULT 0,
    SuccessRate FLOAT NOT NULL DEFAULT 0,
    ResultDetails NVARCHAR(MAX) NULL,
    ErrorMessage NVARCHAR(MAX) NULL,
    DurationMs INT NOT NULL DEFAULT 0,
    FOREIGN KEY (RuleId) REFERENCES Rules(Id) ON DELETE CASCADE
);

CREATE INDEX IX_RuleExecutions_RuleId ON RuleExecutions(RuleId);
CREATE INDEX IX_RuleExecutions_ExecutedAt ON RuleExecutions(ExecutedAt);
