USE [CRMPLMDEV_2025]
GO

IF OBJECT_ID('dbo.RE_FacilityAudits', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[RE_FacilityAudits] (
        [AuditId] INT IDENTITY(1,1) NOT NULL,
        [Action] NVARCHAR(10) NOT NULL,
        [Id] INT NOT NULL,
        [Name] NVARCHAR(250) NOT NULL,
        [CCCId] INT NOT NULL,
        [Address] NVARCHAR(500) NULL,
        [DepartmentId] INT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [IsLocked] BIT NULL DEFAULT 0,
        [CreatedBySystemUserId] INT NULL,
        [CreatedDateTime] DATETIME NULL,
        [ModifiedBySystemUserId] INT NULL,
        [ModifiedDateTime] DATETIME NULL,
        CONSTRAINT [PK_RE_FacilityAudits] PRIMARY KEY CLUSTERED ([AuditId] ASC)
    );
END
GO

IF OBJECT_ID('dbo.RE_FacilityCategoryAudits', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[RE_FacilityCategoryAudits] (
        [AuditId] INT IDENTITY(1,1) NOT NULL,
        [Action] NVARCHAR(10) NOT NULL,
        [Id] INT NOT NULL,
        [Key] NVARCHAR(100) NOT NULL,
        [Name] NVARCHAR(250) NOT NULL,
        [TariffPerSqm] DECIMAL(18,2) NOT NULL,
        [DepartmentId] INT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [IsLocked] BIT NULL DEFAULT 0,
        [CreatedBySystemUserId] INT NULL,
        [CreatedDateTime] DATETIME NULL,
        [ModifiedBySystemUserId] INT NULL,
        [ModifiedDateTime] DATETIME NULL,
        CONSTRAINT [PK_RE_FacilityCategoryAudits] PRIMARY KEY CLUSTERED ([AuditId] ASC)
    );
END
GO

IF OBJECT_ID('dbo.RE_FacilityUnitAudits', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[RE_FacilityUnitAudits] (
        [AuditId] INT IDENTITY(1,1) NOT NULL,
        [Action] NVARCHAR(10) NOT NULL,
        [Id] INT NOT NULL,
        [FacilityId] INT NOT NULL,
        [FacilityCategoryId] INT NOT NULL,
        [UnitType] NVARCHAR(150) NOT NULL,
        [UnitSize] DECIMAL(18,2) NOT NULL,
        [MaxUnits] INT NOT NULL,
        [DepartmentId] INT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [IsLocked] BIT NULL DEFAULT 0,
        [CreatedBySystemUserId] INT NULL,
        [CreatedDateTime] DATETIME NULL,
        [ModifiedBySystemUserId] INT NULL,
        [ModifiedDateTime] DATETIME NULL,
        CONSTRAINT [PK_RE_FacilityUnitAudits] PRIMARY KEY CLUSTERED ([AuditId] ASC)
    );
END
GO

IF OBJECT_ID('dbo.RE_DepartmentalCommentAudits', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[RE_DepartmentalCommentAudits] (
        [AuditId] INT IDENTITY(1,1) NOT NULL,
        [Action] NVARCHAR(10) NOT NULL,
        [Id] INT NOT NULL,
        [RE_ApplicationId] INT NOT NULL,
        [DepartmentName] NVARCHAR(250) NOT NULL,
        [RepresentativeName] NVARCHAR(100) NULL,
        [Outcome] NVARCHAR(100) NULL,
        [Comments] NVARCHAR(MAX) NULL,
        [SupportingDocumentFileId] INT NULL,
        [DateStamp] DATETIME NULL,
        [DepartmentId] INT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [IsLocked] BIT NULL DEFAULT 0,
        [CreatedBySystemUserId] INT NULL,
        [CreatedDateTime] DATETIME NULL,
        [ModifiedBySystemUserId] INT NULL,
        [ModifiedDateTime] DATETIME NULL,
        CONSTRAINT [PK_RE_DepartmentalCommentAudits] PRIMARY KEY CLUSTERED ([AuditId] ASC)
    );
END
GO
