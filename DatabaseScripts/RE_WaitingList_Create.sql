-- =====================================================================
-- DPRE BR07 / DPRE-Q50: Business Hub waiting list (RE_WaitingList + audit).
-- Applicants join it from the Business Hub form when no unit is free;
-- Property Officers work it from Lease Administration > Waiting List.
-- Safe to re-run.
-- =====================================================================

IF OBJECT_ID('dbo.RE_WaitingList', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[RE_WaitingList] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [CustomerId] INT NOT NULL,
        [CCCId] INT NOT NULL,
        [FacilityId] INT NULL,
        [LeaseType] NVARCHAR(60) NULL,
        [PurposeOfLease] NVARCHAR(60) NULL,
        [WaitingStatus] NVARCHAR(20) NOT NULL DEFAULT 'Waiting',
        [OfferedDateTime] DATETIME NULL,
        [OutcomeReason] NVARCHAR(500) NULL,
        [DepartmentId] INT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [IsLocked] BIT NULL DEFAULT 0,
        [CreatedBySystemUserId] INT NULL,
        [CreatedDateTime] DATETIME NULL,
        [ModifiedBySystemUserId] INT NULL,
        [ModifiedDateTime] DATETIME NULL,
        CONSTRAINT [PK_RE_WaitingList] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_RE_WaitingList_Customers] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customers] ([Id]),
        CONSTRAINT [FK_RE_WaitingList_CCCs] FOREIGN KEY ([CCCId]) REFERENCES [dbo].[CCCs] ([Id]),
        CONSTRAINT [FK_RE_WaitingList_RE_Facilities] FOREIGN KEY ([FacilityId]) REFERENCES [dbo].[RE_Facilities] ([Id])
    );
    CREATE INDEX [IX_RE_WaitingList_Queue] ON [dbo].[RE_WaitingList] ([CCCId], [WaitingStatus], [CreatedDateTime]);
    PRINT 'Created Table: RE_WaitingList';
END
GO

IF OBJECT_ID('dbo.RE_WaitingListAudits', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[RE_WaitingListAudits] (
        [AuditId] INT IDENTITY(1,1) NOT NULL,
        [Action] NVARCHAR(10) NOT NULL,
        [Id] INT NOT NULL,
        [CustomerId] INT NOT NULL,
        [CCCId] INT NOT NULL,
        [FacilityId] INT NULL,
        [LeaseType] NVARCHAR(60) NULL,
        [PurposeOfLease] NVARCHAR(60) NULL,
        [WaitingStatus] NVARCHAR(20) NULL,
        [OfferedDateTime] DATETIME NULL,
        [OutcomeReason] NVARCHAR(500) NULL,
        [DepartmentId] INT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [IsLocked] BIT NULL DEFAULT 0,
        [CreatedBySystemUserId] INT NULL,
        [CreatedDateTime] DATETIME NULL,
        [ModifiedBySystemUserId] INT NULL,
        [ModifiedDateTime] DATETIME NULL,
        CONSTRAINT [PK_RE_WaitingListAudits] PRIMARY KEY CLUSTERED ([AuditId] ASC)
    );
    PRINT 'Created Table: RE_WaitingListAudits';
END
GO
