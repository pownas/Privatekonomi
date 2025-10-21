-- Migration script for adding AuditLog table
-- This script should be run when moving from in-memory to a real database

-- Create AuditLogs table
CREATE TABLE AuditLogs (
    AuditLogId INT PRIMARY KEY IDENTITY(1,1),
    Action NVARCHAR(100) NOT NULL,
    EntityType NVARCHAR(100) NOT NULL,
    EntityId INT NULL,
    UserId NVARCHAR(100) NULL,
    IpAddress NVARCHAR(50) NULL,
    Details NVARCHAR(2000) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Create indexes for better query performance
CREATE INDEX IX_AuditLogs_Action ON AuditLogs(Action);
CREATE INDEX IX_AuditLogs_EntityType ON AuditLogs(EntityType);
CREATE INDEX IX_AuditLogs_CreatedAt ON AuditLogs(CreatedAt);
CREATE INDEX IX_AuditLogs_UserId ON AuditLogs(UserId);

-- Verify the table was created
SELECT COUNT(*) as AuditLogTableExists FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME = 'AuditLogs';

PRINT 'AuditLogs table created successfully';
