USE [PropertyLeaseManagementRealEstate]
GO

-- ==========================================================================
-- AuditTrail_CreateTables.sql
-- Creates the 4 audit trail tables for the enterprise audit system.
-- Safe to re-run: uses IF NOT EXISTS guards.
-- ==========================================================================

PRINT '--- Creating Audit Trail Tables ---';

-- ============================================================
-- 1. AuditTrail_UserLogins
-- Tracks every login, logout, and failed login attempt
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditTrail_UserLogins')
BEGIN
    CREATE TABLE [dbo].[AuditTrail_UserLogins] (
        [Id]              INT            IDENTITY(1,1) NOT NULL,
        [SystemUserId]    INT            NOT NULL,
        [DepartmentId]    INT            NULL,
        [EventType]       NVARCHAR(50)   NOT NULL,  -- Login, Logout, LoginFailed
        [IPAddress]       NVARCHAR(100)  NULL,
        [UserAgent]       NVARCHAR(500)  NULL,
        [EventDateTime]   DATETIME       NOT NULL DEFAULT GETDATE(),
        [IsSuccessful]    BIT            NOT NULL DEFAULT 1,
        [FailureReason]   NVARCHAR(500)  NULL,
        CONSTRAINT [PK_AuditTrail_UserLogins] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_AuditTrail_UserLogins_DeptDate]
        ON [dbo].[AuditTrail_UserLogins] ([DepartmentId], [EventDateTime] DESC);

    CREATE NONCLUSTERED INDEX [IX_AuditTrail_UserLogins_UserId]
        ON [dbo].[AuditTrail_UserLogins] ([SystemUserId]);

    PRINT 'Created table: AuditTrail_UserLogins';
END
ELSE
    PRINT 'Table AuditTrail_UserLogins already exists — skipping.';


-- ============================================================
-- 2. AuditTrail_Activities
-- Every user action/transaction: page views, form submits,
-- workflow state changes, data modifications
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditTrail_Activities')
BEGIN
    CREATE TABLE [dbo].[AuditTrail_Activities] (
        [Id]               INT              IDENTITY(1,1) NOT NULL,
        [SystemUserId]     INT              NULL,
        [DepartmentId]     INT              NULL,
        [ActivityType]     NVARCHAR(100)    NOT NULL,  -- PageView, FormSubmit, StatusChange, DataModification, FileUpload, Export
        [Controller]       NVARCHAR(200)    NULL,
        [Action]           NVARCHAR(200)    NULL,
        [Description]      NVARCHAR(1000)   NULL,
        [EntityName]       NVARCHAR(200)    NULL,
        [EntityId]         INT              NULL,
        [OldValue]         NVARCHAR(MAX)    NULL,
        [NewValue]         NVARCHAR(MAX)    NULL,
        [IPAddress]        NVARCHAR(100)    NULL,
        [UserAgent]        NVARCHAR(500)    NULL,
        [IsAdminAction]    BIT              NOT NULL DEFAULT 0,
        [ActivityDateTime] DATETIME         NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_AuditTrail_Activities] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_AuditTrail_Activities_DeptDate]
        ON [dbo].[AuditTrail_Activities] ([DepartmentId], [ActivityDateTime] DESC);

    CREATE NONCLUSTERED INDEX [IX_AuditTrail_Activities_UserId]
        ON [dbo].[AuditTrail_Activities] ([SystemUserId]);

    CREATE NONCLUSTERED INDEX [IX_AuditTrail_Activities_IsAdmin]
        ON [dbo].[AuditTrail_Activities] ([IsAdminAction], [ActivityDateTime] DESC);

    PRINT 'Created table: AuditTrail_Activities';
END
ELSE
    PRINT 'Table AuditTrail_Activities already exists — skipping.';


-- ============================================================
-- 3. AuditTrail_PasswordResets
-- Tracks who reset whose password and when
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditTrail_PasswordResets')
BEGIN
    CREATE TABLE [dbo].[AuditTrail_PasswordResets] (
        [Id]                    INT            IDENTITY(1,1) NOT NULL,
        [TargetSystemUserId]    INT            NOT NULL,
        [TargetDepartmentId]    INT            NULL,
        [ResetBySystemUserId]   INT            NOT NULL,
        [ResetType]             NVARCHAR(50)   NOT NULL,  -- AdminReset, SelfReset, ForcedExpiry
        [IPAddress]             NVARCHAR(100)  NULL,
        [ResetDateTime]         DATETIME       NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_AuditTrail_PasswordResets] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_AuditTrail_PasswordResets_DeptDate]
        ON [dbo].[AuditTrail_PasswordResets] ([TargetDepartmentId], [ResetDateTime] DESC);

    PRINT 'Created table: AuditTrail_PasswordResets';
END
ELSE
    PRINT 'Table AuditTrail_PasswordResets already exists — skipping.';


-- ============================================================
-- 4. AuditTrail_RoleModifications
-- Tracks role assignments, removals, and changes
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditTrail_RoleModifications')
BEGIN
    CREATE TABLE [dbo].[AuditTrail_RoleModifications] (
        [Id]                      INT            IDENTITY(1,1) NOT NULL,
        [TargetSystemUserId]      INT            NOT NULL,
        [TargetDepartmentId]      INT            NULL,
        [ModifiedBySystemUserId]  INT            NOT NULL,
        [ModificationType]        NVARCHAR(50)   NOT NULL,  -- RoleAdded, RoleRemoved, RoleChanged
        [PreviousRoleName]        NVARCHAR(200)  NULL,
        [NewRoleName]             NVARCHAR(200)  NULL,
        [IPAddress]               NVARCHAR(100)  NULL,
        [ModificationDateTime]    DATETIME       NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_AuditTrail_RoleModifications] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_AuditTrail_RoleModifications_DeptDate]
        ON [dbo].[AuditTrail_RoleModifications] ([TargetDepartmentId], [ModificationDateTime] DESC);

    PRINT 'Created table: AuditTrail_RoleModifications';
END
ELSE
    PRINT 'Table AuditTrail_RoleModifications already exists — skipping.';


PRINT '--- Audit Trail Tables Created Successfully ---';
GO
