-- USE [PropertyLeaseManagementRealEstate]
-- GO

PRINT '--- STARTING REAL ESTATE DATABASE SETUP ---';

-- 1. Create RE_FacilityCategories table
IF OBJECT_ID('dbo.RE_FacilityCategories', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RE_FacilityCategories (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        [Key] NVARCHAR(100) NOT NULL UNIQUE,
        Name NVARCHAR(250) NOT NULL,
        TariffPerSqm DECIMAL(18,2) NOT NULL,
        DepartmentId INT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsLocked BIT NULL DEFAULT 0,
        CreatedBySystemUserId INT NULL,
        CreatedDateTime DATETIME NULL,
        ModifiedBySystemUserId INT NULL,
        ModifiedDateTime DATETIME NULL
    );
    PRINT 'Created Table: RE_FacilityCategories';
END;
ELSE
BEGIN
    IF COL_LENGTH('dbo.RE_FacilityCategories', 'DepartmentId') IS NULL
    BEGIN
        ALTER TABLE dbo.RE_FacilityCategories ADD DepartmentId INT NULL;
        PRINT 'Altered Table: RE_FacilityCategories - Added DepartmentId';
    END
END;

-- 2. Create RE_Facilities table
IF OBJECT_ID('dbo.RE_Facilities', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RE_Facilities (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(250) NOT NULL,
        CCCId INT NOT NULL,
        Address NVARCHAR(500) NULL,
        DepartmentId INT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsLocked BIT NULL DEFAULT 0,
        CreatedBySystemUserId INT NULL,
        CreatedDateTime DATETIME NULL,
        ModifiedBySystemUserId INT NULL,
        ModifiedDateTime DATETIME NULL,
        CONSTRAINT FK_RE_Facilities_CCCs FOREIGN KEY (CCCId) REFERENCES dbo.CCCs (Id)
    );
    PRINT 'Created Table: RE_Facilities';
END;
ELSE
BEGIN
    IF COL_LENGTH('dbo.RE_Facilities', 'DepartmentId') IS NULL
    BEGIN
        ALTER TABLE dbo.RE_Facilities ADD DepartmentId INT NULL;
        PRINT 'Altered Table: RE_Facilities - Added DepartmentId';
    END
END;

-- 3. Create RE_FacilityUnits table
IF OBJECT_ID('dbo.RE_FacilityUnits', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RE_FacilityUnits (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FacilityId INT NOT NULL,
        FacilityCategoryId INT NOT NULL,
        UnitType NVARCHAR(150) NOT NULL,
        UnitSize DECIMAL(18,2) NOT NULL,
        MaxUnits INT NOT NULL DEFAULT 1,
        DepartmentId INT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsLocked BIT NULL DEFAULT 0,
        CreatedBySystemUserId INT NULL,
        CreatedDateTime DATETIME NULL,
        ModifiedBySystemUserId INT NULL,
        ModifiedDateTime DATETIME NULL,
        CONSTRAINT FK_RE_FacilityUnits_Facilities FOREIGN KEY (FacilityId) REFERENCES dbo.RE_Facilities (Id),
        CONSTRAINT FK_RE_FacilityUnits_FacilityCategories FOREIGN KEY (FacilityCategoryId) REFERENCES dbo.RE_FacilityCategories (Id)
    );
    PRINT 'Created Table: RE_FacilityUnits';
END;
ELSE
BEGIN
    IF COL_LENGTH('dbo.RE_FacilityUnits', 'DepartmentId') IS NULL
    BEGIN
        ALTER TABLE dbo.RE_FacilityUnits ADD DepartmentId INT NULL;
        PRINT 'Altered Table: RE_FacilityUnits - Added DepartmentId';
    END
END;

-- 4. Create RE_Applications table
IF OBJECT_ID('dbo.RE_Applications', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[RE_Applications] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [ApplicationReferenceNumber] NVARCHAR(100) NOT NULL,
        [SystemUserId] INT NOT NULL,
        [CustomerId] INT NOT NULL,
        [ApplicantType] NVARCHAR(100) NOT NULL,
        
        -- Entity Details
        [EntityName] NVARCHAR(250) NULL,
        [CompanyRegistrationNumber] NVARCHAR(100) NULL,
        [VatRegistrationNumber] NVARCHAR(100) NULL,
        [TaxReferenceNumber] NVARCHAR(100) NULL,
        [EntityRegisteredAddress] NVARCHAR(500) NULL,
        [EntityRegisteredPostalCode] NVARCHAR(20) NULL,
        [AuthorizedRepresentativeName] NVARCHAR(250) NULL,
        [AuthorizedRepresentativeCapacity] NVARCHAR(100) NULL,
        [EntityTelephone] NVARCHAR(50) NULL,
        [EntityMobile] NVARCHAR(50) NULL,
        [EntityFax] NVARCHAR(50) NULL,
        [EntityEmail] NVARCHAR(150) NULL,

        -- Banking Details
        [BankName] NVARCHAR(100) NOT NULL,
        [BankAccountType] NVARCHAR(50) NOT NULL,
        [BankAccountName] NVARCHAR(150) NOT NULL,
        [BankAccountNumber] NVARCHAR(100) NOT NULL,
        [BankBranchCode] NVARCHAR(50) NOT NULL,

        -- Lease Details
        [PurposeOfLease] NVARCHAR(100) NOT NULL,
        [CCCId] INT NOT NULL,
        [ErfFarmNumber] NVARCHAR(100) NOT NULL,
        [PropertyAddress] NVARCHAR(500) NOT NULL,
        [TownshipSuburbFarmName] NVARCHAR(200) NOT NULL,
        [PropertyPostalCode] NVARCHAR(20) NOT NULL,

        -- Facility Types
        [FacilityOutdoorAdvertising] BIT NOT NULL DEFAULT 0,
        [FacilityTelecommunications] BIT NOT NULL DEFAULT 0,
        [FacilityInformalTrading] BIT NOT NULL DEFAULT 0,
        [FacilityTaxiRankTrading] BIT NOT NULL DEFAULT 0,
        [FacilityVocationalSkills] BIT NOT NULL DEFAULT 0,
        [FacilityComputerTraining] BIT NOT NULL DEFAULT 0,
        [FacilityIndustrialPark] BIT NOT NULL DEFAULT 0,
        [FacilityBusinessHub] BIT NOT NULL DEFAULT 0,
        [FacilityAutomotiveHub] BIT NOT NULL DEFAULT 0,
        [FacilityAgriPark] BIT NOT NULL DEFAULT 0,
        [FacilityIncubationFarm] BIT NOT NULL DEFAULT 0,

        -- BaseModel fields
        [StatusId] INT NOT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [IsLocked] BIT NULL DEFAULT 0,
        [CreatedBySystemUserId] INT NULL,
        [CreatedDateTime] DATETIME NULL,
        [ModifiedBySystemUserId] INT NULL,
        [ModifiedDateTime] DATETIME NULL,

        -- Real Estate extensions
        [DepartmentId] INT NULL,
        [SelectedFacilityId] INT NULL,
        [SelectedFacilityUnitId] INT NULL,
        [SelectedUnitCount] INT NULL,
        [CalculatedMonthlyRental] DECIMAL(18,2) NULL,

        -- Verification / Workflow steps
        [CreditBureauResult] NVARCHAR(MAX) NULL,
        [HomeAffairsResult] NVARCHAR(MAX) NULL,
        [DeedsResult] NVARCHAR(MAX) NULL,
        [SassaResult] NVARCHAR(MAX) NULL,
        [CipcResult] NVARCHAR(MAX) NULL,
        [RiskAssessmentRecommendation] NVARCHAR(MAX) NULL,
        [RiskAssessmentReason] NVARCHAR(MAX) NULL,
        [RiskAssessmentEvidenceFileId] INT NULL,
        [PaymentValidationComment] NVARCHAR(MAX) NULL,

        -- Committee / Final approvals
        [CommitteeDecision] NVARCHAR(MAX) NULL,
        [CommitteeComments] NVARCHAR(MAX) NULL,
        [CommitteeResolutionFileId] INT NULL,
        [FinalOutcome] NVARCHAR(MAX) NULL,
        [FinalComments] NVARCHAR(MAX) NULL,
        [FinalSignature] NVARCHAR(MAX) NULL,

        -- Handover / Inspection
        [InspectionType] NVARCHAR(MAX) NULL,
        [InspectionDate] DATETIME NULL,
        [InspectionTime] NVARCHAR(MAX) NULL,
        [InspectionStatus] NVARCHAR(MAX) NULL,
        [InspectionComments] NVARCHAR(MAX) NULL,
        [InspectionPlumbing] NVARCHAR(MAX) NULL,
        [InspectionElectrical] NVARCHAR(MAX) NULL,
        [InspectionFixtures] NVARCHAR(MAX) NULL,
        [InspectionSanitation] NVARCHAR(MAX) NULL,
        [InspectionHazards] NVARCHAR(MAX) NULL,
        [InspectionWearTear] NVARCHAR(MAX) NULL,
        [InspectionFormFileId] INT NULL,

        -- Maintenance / Works orders
        [WorkOrderNumber] NVARCHAR(MAX) NULL,
        [WorkOrderStatus] NVARCHAR(MAX) NULL,
        [WorkOrderTasks] NVARCHAR(MAX) NULL,
        [WorkOrderIssueDescription] NVARCHAR(MAX) NULL,
        [WorkOrderPriority] NVARCHAR(MAX) NULL,
        [WorkOrderDueDate] DATETIME NULL,
        [WorkOrderMaterials] NVARCHAR(MAX) NULL,
        [WorkOrderSafetyInstructions] NVARCHAR(MAX) NULL,
        [WorkOrderAssignmentType] NVARCHAR(MAX) NULL,
        [WorkOrderTechnicianName] NVARCHAR(MAX) NULL,
        [WorkOrderRejectionReason] NVARCHAR(MAX) NULL,
        [WorkOrderJobSheetFileId] INT NULL,
        [WorkOrderManagerComments] NVARCHAR(MAX) NULL,
        [WorkOrderManagerSignature] NVARCHAR(MAX) NULL,

        -- Permission to Occupy (PTO)
        [PtoReferenceNumber] NVARCHAR(MAX) NULL,
        [PtoPurposeOfOccupation] NVARCHAR(MAX) NULL,
        [PtoStartDate] DATETIME NULL,
        [PtoEndDate] DATETIME NULL,
        [PtoAcceptedIndemnity] BIT NULL,
        [PtoStatus] NVARCHAR(MAX) NULL,
        [PtoReviewRecommendation] NVARCHAR(MAX) NULL,
        [PtoReviewReason] NVARCHAR(MAX) NULL,
        [PtoDecision] NVARCHAR(MAX) NULL,
        [PtoDecisionReason] NVARCHAR(MAX) NULL,
        [PtoSignature] NVARCHAR(MAX) NULL,

        CONSTRAINT [PK_RE_Applications] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_RE_Applications_SystemUsers] FOREIGN KEY ([SystemUserId]) REFERENCES [dbo].[SystemUsers] ([Id]),
        CONSTRAINT [FK_RE_Applications_Customers] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customers] ([Id]),
        CONSTRAINT [FK_RE_Applications_Status] FOREIGN KEY ([StatusId]) REFERENCES [dbo].[Status] ([Id]),
        CONSTRAINT [FK_RE_Applications_CCCs] FOREIGN KEY ([CCCId]) REFERENCES [dbo].[CCCs] ([Id]),
        CONSTRAINT [FK_RE_Applications_Facilities] FOREIGN KEY ([SelectedFacilityId]) REFERENCES [dbo].[RE_Facilities] ([Id]),
        CONSTRAINT [FK_RE_Applications_FacilityUnits] FOREIGN KEY ([SelectedFacilityUnitId]) REFERENCES [dbo].[RE_FacilityUnits] ([Id])
    );
    PRINT 'Created Table: RE_Applications';
END;

-- Schema Migration: ensure RealEstateApplicationId exists on Documents and PLMApplicationHistorty tables
IF OBJECT_ID('dbo.Documents', 'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.Documents', 'RealEstateApplicationId') IS NULL
    BEGIN
        ALTER TABLE dbo.Documents ADD RealEstateApplicationId INT NULL;
        ALTER TABLE dbo.Documents ADD CONSTRAINT FK_Documents_RE_Applications FOREIGN KEY (RealEstateApplicationId) REFERENCES dbo.RE_Applications (Id);
        PRINT 'Altered Table: Documents - Added RealEstateApplicationId';
    END
END

IF OBJECT_ID('dbo.PLMApplicationHistortyLogs', 'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.PLMApplicationHistortyLogs', 'RealEstateApplicationId') IS NULL
    BEGIN
        ALTER TABLE dbo.PLMApplicationHistortyLogs ADD RealEstateApplicationId INT NULL;
        ALTER TABLE dbo.PLMApplicationHistortyLogs ADD CONSTRAINT FK_PLMApplicationHistortyLogs_RE_Applications FOREIGN KEY (RealEstateApplicationId) REFERENCES dbo.RE_Applications (Id);
        PRINT 'Altered Table: PLMApplicationHistortyLogs - Added RealEstateApplicationId';
    END
END

IF OBJECT_ID('dbo.PLMApplicationHistortyLogAudits', 'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.PLMApplicationHistortyLogAudits', 'RealEstateApplicationId') IS NULL
    BEGIN
        ALTER TABLE dbo.PLMApplicationHistortyLogAudits ADD RealEstateApplicationId INT NULL;
        PRINT 'Altered Table: PLMApplicationHistortyLogAudits - Added RealEstateApplicationId';
    END
END

-- 5. Create RE_DepartmentalComments table
IF OBJECT_ID('dbo.RE_DepartmentalComments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RE_DepartmentalComments (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        RE_ApplicationId INT NOT NULL,
        DepartmentName NVARCHAR(250) NOT NULL,
        RepresentativeName NVARCHAR(100) NULL,
        Outcome NVARCHAR(100) NULL,
        Comments NVARCHAR(MAX) NULL,
        SupportingDocumentFileId INT NULL,
        DateStamp DATETIME NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsLocked BIT NOT NULL DEFAULT 0,
        CreatedBySystemUserId INT NULL,
        CreatedDateTime DATETIME NULL,
        ModifiedBySystemUserId INT NULL,
        ModifiedDateTime DATETIME NULL,
        DepartmentId INT NULL,
        CONSTRAINT FK_RE_DepartmentalComments_Applications FOREIGN KEY (RE_ApplicationId) REFERENCES dbo.RE_Applications (Id)
    );
    PRINT 'Created Table: RE_DepartmentalComments';
END;

-- 6. Create Audit Tables
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
    PRINT 'Created Table: RE_FacilityAudits';
END;

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
    PRINT 'Created Table: RE_FacilityCategoryAudits';
END;

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
    PRINT 'Created Table: RE_FacilityUnitAudits';
END;

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
    PRINT 'Created Table: RE_DepartmentalCommentAudits';
END;

IF OBJECT_ID('dbo.RE_ApplicationsAudit', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[RE_ApplicationsAudit] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [RE_ApplicationId] INT NOT NULL,
        [ApplicationReferenceNumber] NVARCHAR(100) NOT NULL,
        [SystemUserId] INT NOT NULL,
        [CustomerId] INT NOT NULL,
        [ApplicantType] NVARCHAR(100) NOT NULL,
        [EntityName] NVARCHAR(250) NULL,
        [CompanyRegistrationNumber] NVARCHAR(100) NULL,
        [VatRegistrationNumber] NVARCHAR(100) NULL,
        [TaxReferenceNumber] NVARCHAR(100) NULL,
        [EntityRegisteredAddress] NVARCHAR(500) NULL,
        [EntityRegisteredPostalCode] NVARCHAR(20) NULL,
        [AuthorizedRepresentativeName] NVARCHAR(250) NULL,
        [AuthorizedRepresentativeCapacity] NVARCHAR(100) NULL,
        [EntityTelephone] NVARCHAR(50) NULL,
        [EntityMobile] NVARCHAR(50) NULL,
        [EntityFax] NVARCHAR(50) NULL,
        [EntityEmail] NVARCHAR(150) NULL,
        [BankName] NVARCHAR(100) NOT NULL,
        [BankAccountType] NVARCHAR(50) NOT NULL,
        [BankAccountName] NVARCHAR(150) NOT NULL,
        [BankAccountNumber] NVARCHAR(100) NOT NULL,
        [BankBranchCode] NVARCHAR(50) NOT NULL,
        [PurposeOfLease] NVARCHAR(100) NOT NULL,
        [CCCId] INT NOT NULL,
        [ErfFarmNumber] NVARCHAR(100) NOT NULL,
        [PropertyAddress] NVARCHAR(500) NOT NULL,
        [TownshipSuburbFarmName] NVARCHAR(200) NOT NULL,
        [PropertyPostalCode] NVARCHAR(20) NOT NULL,
        [FacilityOutdoorAdvertising] BIT NOT NULL DEFAULT 0,
        [FacilityTelecommunications] BIT NOT NULL DEFAULT 0,
        [FacilityInformalTrading] BIT NOT NULL DEFAULT 0,
        [FacilityTaxiRankTrading] BIT NOT NULL DEFAULT 0,
        [FacilityVocationalSkills] BIT NOT NULL DEFAULT 0,
        [FacilityComputerTraining] BIT NOT NULL DEFAULT 0,
        [FacilityIndustrialPark] BIT NOT NULL DEFAULT 0,
        [FacilityBusinessHub] BIT NOT NULL DEFAULT 0,
        [FacilityAutomotiveHub] BIT NOT NULL DEFAULT 0,
        [FacilityAgriPark] BIT NOT NULL DEFAULT 0,
        [FacilityIncubationFarm] BIT NOT NULL DEFAULT 0,
        
        [Action] NVARCHAR(50) NOT NULL,
        [StatusId] INT NOT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [IsLocked] BIT NULL DEFAULT 0,
        [ModifiedBySystemUserId] INT NULL,
        [ModifiedDateTime] DATETIME NULL,
        [DepartmentId] INT NULL,

        -- Real Estate extensions
        [SelectedFacilityId] INT NULL,
        [SelectedFacilityUnitId] INT NULL,
        [SelectedUnitCount] INT NULL,
        [CalculatedMonthlyRental] DECIMAL(18,2) NULL,

        -- Verification / Workflow steps
        [CreditBureauResult] NVARCHAR(MAX) NULL,
        [HomeAffairsResult] NVARCHAR(MAX) NULL,
        [DeedsResult] NVARCHAR(MAX) NULL,
        [SassaResult] NVARCHAR(MAX) NULL,
        [CipcResult] NVARCHAR(MAX) NULL,
        [RiskAssessmentRecommendation] NVARCHAR(MAX) NULL,
        [RiskAssessmentReason] NVARCHAR(MAX) NULL,
        [RiskAssessmentEvidenceFileId] INT NULL,
        [PaymentValidationComment] NVARCHAR(MAX) NULL,

        -- Committee / Final approvals
        [CommitteeDecision] NVARCHAR(MAX) NULL,
        [CommitteeComments] NVARCHAR(MAX) NULL,
        [CommitteeResolutionFileId] INT NULL,
        [FinalOutcome] NVARCHAR(MAX) NULL,
        [FinalComments] NVARCHAR(MAX) NULL,
        [FinalSignature] NVARCHAR(MAX) NULL,

        -- Handover / Inspection
        [InspectionType] NVARCHAR(MAX) NULL,
        [InspectionDate] DATETIME NULL,
        [InspectionTime] NVARCHAR(MAX) NULL,
        [InspectionStatus] NVARCHAR(MAX) NULL,
        [InspectionComments] NVARCHAR(MAX) NULL,
        [InspectionPlumbing] NVARCHAR(MAX) NULL,
        [InspectionElectrical] NVARCHAR(MAX) NULL,
        [InspectionFixtures] NVARCHAR(MAX) NULL,
        [InspectionSanitation] NVARCHAR(MAX) NULL,
        [InspectionHazards] NVARCHAR(MAX) NULL,
        [InspectionWearTear] NVARCHAR(MAX) NULL,
        [InspectionFormFileId] INT NULL,

        -- Maintenance / Works orders
        [WorkOrderNumber] NVARCHAR(MAX) NULL,
        [WorkOrderStatus] NVARCHAR(MAX) NULL,
        [WorkOrderTasks] NVARCHAR(MAX) NULL,
        [WorkOrderIssueDescription] NVARCHAR(MAX) NULL,
        [WorkOrderPriority] NVARCHAR(MAX) NULL,
        [WorkOrderDueDate] DATETIME NULL,
        [WorkOrderMaterials] NVARCHAR(MAX) NULL,
        [WorkOrderSafetyInstructions] NVARCHAR(MAX) NULL,
        [WorkOrderAssignmentType] NVARCHAR(MAX) NULL,
        [WorkOrderTechnicianName] NVARCHAR(MAX) NULL,
        [WorkOrderRejectionReason] NVARCHAR(MAX) NULL,
        [WorkOrderJobSheetFileId] INT NULL,
        [WorkOrderManagerComments] NVARCHAR(MAX) NULL,
        [WorkOrderManagerSignature] NVARCHAR(MAX) NULL,

        -- Permission to Occupy (PTO)
        [PtoReferenceNumber] NVARCHAR(MAX) NULL,
        [PtoPurposeOfOccupation] NVARCHAR(MAX) NULL,
        [PtoStartDate] DATETIME NULL,
        [PtoEndDate] DATETIME NULL,
        [PtoAcceptedIndemnity] BIT NULL,
        [PtoStatus] NVARCHAR(MAX) NULL,
        [PtoReviewRecommendation] NVARCHAR(MAX) NULL,
        [PtoReviewReason] NVARCHAR(MAX) NULL,
        [PtoDecision] NVARCHAR(MAX) NULL,
        [PtoDecisionReason] NVARCHAR(MAX) NULL,
        [PtoSignature] NVARCHAR(MAX) NULL,

        CONSTRAINT [PK_RE_ApplicationsAudit] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    PRINT 'Created Table: RE_ApplicationsAudit';
END;
GO

-- 7. Seed missing CCCs
SET IDENTITY_INSERT [dbo].[CCCs] ON;
IF NOT EXISTS (SELECT 1 FROM [dbo].[CCCs] WHERE Id = 10)
    INSERT INTO [dbo].[CCCs] (Id, CCCTypeId, CCCName, Prefix, IsActive, IsDeleted)
    VALUES (10, 1, 'Tokoza', '90', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CCCs] WHERE Id = 11)
    INSERT INTO [dbo].[CCCs] (Id, CCCTypeId, CCCName, Prefix, IsActive, IsDeleted)
    VALUES (11, 1, 'Tsakane', '91', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CCCs] WHERE Id = 12)
    INSERT INTO [dbo].[CCCs] (Id, CCCTypeId, CCCName, Prefix, IsActive, IsDeleted)
    VALUES (12, 1, 'Kwa - Thema', '92', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CCCs] WHERE Id = 13)
    INSERT INTO [dbo].[CCCs] (Id, CCCTypeId, CCCName, Prefix, IsActive, IsDeleted)
    VALUES (13, 1, 'Daveyton', '93', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CCCs] WHERE Id = 14)
    INSERT INTO [dbo].[CCCs] (Id, CCCTypeId, CCCName, Prefix, IsActive, IsDeleted)
    VALUES (14, 1, 'Etwatwa', '94', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CCCs] WHERE Id = 15)
    INSERT INTO [dbo].[CCCs] (Id, CCCTypeId, CCCName, Prefix, IsActive, IsDeleted)
    VALUES (15, 1, 'Thembisa', '95', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CCCs] WHERE Id = 16)
    INSERT INTO [dbo].[CCCs] (Id, CCCTypeId, CCCName, Prefix, IsActive, IsDeleted)
    VALUES (16, 1, 'Katlehong', '96', 1, 0);
SET IDENTITY_INSERT [dbo].[CCCs] OFF;
GO

-- 8. Seed categories
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityCategories] WHERE [Key] = 'township_industrial_parks')
    INSERT INTO [dbo].[RE_FacilityCategories] ([Key], [Name], [TariffPerSqm], [IsActive], [IsDeleted], [IsLocked])
    VALUES ('township_industrial_parks', 'Township Industrial Parks', 57.00, 1, 0, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityCategories] WHERE [Key] = 'township_business_hubs')
    INSERT INTO [dbo].[RE_FacilityCategories] ([Key], [Name], [TariffPerSqm], [IsActive], [IsDeleted], [IsLocked])
    VALUES ('township_business_hubs', 'Township Business Hubs & Skills Centre', 57.00, 1, 0, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityCategories] WHERE [Key] = 'township_automotive_hubs')
    INSERT INTO [dbo].[RE_FacilityCategories] ([Key], [Name], [TariffPerSqm], [IsActive], [IsDeleted], [IsLocked])
    VALUES ('township_automotive_hubs', 'Township Automotive Hubs.', 67.00, 1, 0, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityCategories] WHERE [Key] = 'fablab_facilities')
    INSERT INTO [dbo].[RE_FacilityCategories] ([Key], [Name], [TariffPerSqm], [IsActive], [IsDeleted], [IsLocked])
    VALUES ('fablab_facilities', 'FabLab Facilities', 36.00, 1, 0, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityCategories] WHERE [Key] = 'incubation_farms')
    INSERT INTO [dbo].[RE_FacilityCategories] ([Key], [Name], [TariffPerSqm], [IsActive], [IsDeleted], [IsLocked])
    VALUES ('incubation_farms', 'Incubation Farms and/or Agri-parks', 0.70, 1, 0, 0);
GO

-- 9. Seed Facilities
SET IDENTITY_INSERT [dbo].[RE_Facilities] ON;
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 1)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted)
    VALUES (1, 'Fannie Malape Co-operatives Industrial Hive Centre', 10, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 2)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted)
    VALUES (2, 'Tokoza Traders Market', 10, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 3)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted)
    VALUES (3, 'Tsakane Business Park', 11, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 4)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted)
    VALUES (4, 'KwaThema Business Park', 12, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 5)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted)
    VALUES (5, 'Springs Traders Market', 4, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 6)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted)
    VALUES (6, 'Brakpan Civic Centre Kiosk', 6, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 7)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted)
    VALUES (7, 'Oscar Mabika Co-operatives Industrial Hive Centre', 13, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 8)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted)
    VALUES (8, 'Barcelona Traders Market', 14, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 9)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted)
    VALUES (9, 'Etwatwa Business Hive', 14, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 10)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted)
    VALUES (10, 'Etwatwa Unserviced Portions', 14, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 11)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted)
    VALUES (11, 'Bomba Sibiya Co-ops Industrial Hive Centre', 15, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 12)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted)
    VALUES (12, 'Tembisa Business Park', 15, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 13)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted)
    VALUES (13, 'Motsu Buy Back Centre', 15, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 14)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted)
    VALUES (14, 'Sethokga Buy Back Centre', 15, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 15)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted)
    VALUES (15, 'Sethokga Traders Market', 15, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 16)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted)
    VALUES (16, 'Sedibeng Hive Centre', 15, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 17)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted)
    VALUES (17, 'Katlehong Automotive manufacturing hub', 16, '', 1, 0);
SET IDENTITY_INSERT [dbo].[RE_Facilities] OFF;
GO

-- 10. Seed Facility Units
DECLARE @CatParks INT = (SELECT Id FROM [dbo].[RE_FacilityCategories] WHERE [Key] = 'township_industrial_parks');
DECLARE @CatHubs INT = (SELECT Id FROM [dbo].[RE_FacilityCategories] WHERE [Key] = 'township_business_hubs');
DECLARE @CatAuto INT = (SELECT Id FROM [dbo].[RE_FacilityCategories] WHERE [Key] = 'township_automotive_hubs');
DECLARE @CatFab INT = (SELECT Id FROM [dbo].[RE_FacilityCategories] WHERE [Key] = 'fablab_facilities');
DECLARE @CatFarms INT = (SELECT Id FROM [dbo].[RE_FacilityCategories] WHERE [Key] = 'incubation_farms');

SET IDENTITY_INSERT [dbo].[RE_FacilityUnits] ON;

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 1)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (1, 1, @CatParks, 'Indoor Unit', 65.00, 4, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 2)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (2, 2, @CatHubs, 'Indoor Unit', 65.00, 4, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 3)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (3, 3, @CatHubs, 'Offices', 10.00, 5, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 4)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (4, 3, @CatHubs, 'Single Garage Size Units', 18.00, 10, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 5)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (5, 3, @CatHubs, 'Double Garage Size Units', 36.00, 10, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 6)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (6, 3, @CatHubs, 'Kiosk', 17.00, 12, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 7)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (7, 4, @CatHubs, 'Indoor Unit (Block A)', 21.00, 32, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 8)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (8, 4, @CatHubs, 'Indoor Unit (Block B)', 42.50, 12, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 9)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (9, 5, @CatHubs, 'Small Shop', 20.00, 21, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 10)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (10, 6, @CatHubs, 'Kiosk', 17.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 11)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (11, 7, @CatHubs, 'Offices', 24.00, 8, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 12)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (12, 7, @CatHubs, 'Indoor Unit', 80.00, 3, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 13)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (13, 7, @CatHubs, 'Outdoor Roof Covered Units', 70.00, 4, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 14)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (14, 7, @CatHubs, 'Boardroom', 26.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 15)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (15, 8, @CatHubs, 'Small Shop', 20.00, 21, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 16)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (16, 9, @CatHubs, 'Indoor Unit (35m²)', 35.00, 12, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 17)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (17, 9, @CatHubs, 'Kiosk', 17.00, 3, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 18)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (18, 9, @CatHubs, 'Indoor Unit (34m²)', 34.00, 27, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 19)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (19, 9, @CatHubs, 'Indoor Unit (57m²)', 57.00, 5, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 20)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (20, 9, @CatHubs, 'Admin Block with Boardroom', 173.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 21)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (21, 10, @CatFarms, 'Outdoor Plot (4300m²)', 4300.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 22)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (22, 10, @CatFarms, 'Outdoor Plot (4840m²)', 4840.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 23)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (23, 10, @CatFarms, 'Outdoor Plot (4100m²)', 4100.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 24)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (24, 10, @CatFarms, 'Outdoor Plot (3100m²)', 3100.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 25)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (25, 11, @CatHubs, 'Indoor Unit', 40.00, 7, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 26)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (26, 11, @CatHubs, 'Offices', 13.00, 3, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 27)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (27, 12, @CatHubs, 'Offices', 13.00, 18, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 28)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (28, 13, @CatAuto, 'Workshop', 322.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 29)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (29, 14, @CatHubs, 'Offices', 14.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 30)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (30, 14, @CatAuto, 'Workshop', 50.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 31)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (31, 14, @CatFab, 'Roof Covered Sorting Area', 11.00, 5, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 32)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (32, 15, @CatHubs, 'Open Stalls', 6.00, 22, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 33)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (33, 15, @CatHubs, 'Lockable Stalls', 12.00, 18, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 34)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (34, 16, @CatAuto, 'Workshop', 85.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 35)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (35, 16, @CatHubs, 'Kitchen', 7.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 36)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (36, 16, @CatHubs, 'Offices', 24.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 37)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (37, 17, @CatAuto, 'Unit A01 Workshop', 128.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 38)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (38, 17, @CatAuto, 'Unit A02 Workshop', 128.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 39)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (39, 17, @CatAuto, 'Unit A03 Workshop', 117.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 40)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (40, 17, @CatAuto, 'Unit A04 Workshop', 117.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 41)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (41, 17, @CatAuto, 'Unit A05 Workshop', 150.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 42)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (42, 17, @CatAuto, 'Unit A06 Workshop', 30.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 43)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (43, 17, @CatHubs, 'Offices', 30.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 44)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (44, 17, @CatHubs, 'Canteen', 30.00, 1, 1, 0);

SET IDENTITY_INSERT [dbo].[RE_FacilityUnits] OFF;
GO

-- 11. Seed missing Caretaker role
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Caretaker')
    INSERT INTO AspNetRoles (Id, Name) VALUES ('e141a06a-68a8-4e76-a06b-c0da7682dc06', 'Caretaker');
GO

-- 11.5 Seed missing Real Estate Application ReferenceType
IF NOT EXISTS (SELECT 1 FROM ReferenceTypes WHERE [Key] = 'rt_real_estate_application')
BEGIN
    SET IDENTITY_INSERT [dbo].[ReferenceTypes] ON;
    INSERT INTO [dbo].[ReferenceTypes] ([Id], [Name], [Description], [Key], [IsActive], [IsDeleted], [IsLocked], [CreatedBySystemUserId], [CreatedDateTime])
    VALUES (16, N'Real Estate Application', N'Real Estate Application', N'rt_real_estate_application', 1, 0, 0, 1, GETDATE());
    SET IDENTITY_INSERT [dbo].[ReferenceTypes] OFF;
    PRINT 'Seeded ReferenceType: rt_real_estate_application';
END
GO

-- 12. Seed users and mappings
DECLARE @Role_Customer NVARCHAR(128) = '9524ba14-6dea-44af-b2e4-c94f8980a412';
DECLARE @Role_Finance NVARCHAR(128) = '95660a1d-c242-49cc-a549-1097a5a419f9';
DECLARE @Role_Property NVARCHAR(128) = 'c23949ee-e73d-4d87-a466-6a47b28bd0d3';
DECLARE @Role_Area NVARCHAR(128) = 'cc8e81f0-8440-4740-a746-e51a9f49d68d';
DECLARE @Role_BackOffice NVARCHAR(128) = '90103409-5e37-493f-b80a-20e216178cba';
DECLARE @Role_FacManager NVARCHAR(128) = 'ADDBE943-6C29-47C6-A69A-CFDE77F6FE4D';
DECLARE @Role_Technician NVARCHAR(128) = 'e141a06a-68a8-4e76-a06b-c0da7682dc06';

DECLARE @PwdHash NVARCHAR(MAX) = 'AIrxMxCDw+uPXdvWWYzwF1kac9gt2e5G/AF83mz1RjdyStI81NRJ18bVFq1ZGkxoCA==';
DECLARE @SecurityStamp NVARCHAR(MAX) = '669cc746-a1ef-46c6-afe1-2ffa3053b5e0';

DECLARE @AspNetUserId NVARCHAR(128);
DECLARE @SystemUserId INT;

-- 1. RealEstateCustomer
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE UserName = 'RealEstateCustomer')
BEGIN
    INSERT INTO SystemUsers (FirstName, LastName, UserName, EmailAddress, IsActive, IsDeleted, IsPasswordReset, IsTemporaryPassword, IsIAMRegistered, DepartmentId)
    VALUES ('Real', 'Estate', 'RealEstateCustomer', 'realestate@test.com', 1, 0, 1, 0, 0, 3);
    SET @SystemUserId = SCOPE_IDENTITY();

    SET @AspNetUserId = 'a001a001-beef-4c7b-a2fc-b04afb57b3c1';
    INSERT INTO AspNetUsers (Id, SystemUserId, UserName, PasswordHash, SecurityStamp, Email, EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
    VALUES (@AspNetUserId, @SystemUserId, 'RealEstateCustomer', @PwdHash, @SecurityStamp, 'realestate@test.com', 1, 0, 0, 1, 0);

    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@AspNetUserId, @Role_Customer);

    DECLARE @StatusId INT = (SELECT TOP 1 Id FROM Status WHERE [Key] = 's_customer_active');
    DECLARE @CustomerTypeId INT = (SELECT TOP 1 Id FROM CustomerTypes WHERE [Key] = 'ct_individual');
    DECLARE @IdTypeId INT = (SELECT TOP 1 Id FROM IdentificationTypes WHERE [Key] = 'id_south_african');

    DECLARE @NewCustomerId INT;
    INSERT INTO Customers (CustomerTypeId, IdentificationTypeId, IdentificationNumber, TitleTypeId, FirstName, LastName, EmailAddress, SystemUserId, StatusId, IsActive, IsDeleted, PhysicalAddressCode, PostalAddressCode)
    VALUES (@CustomerTypeId, @IdTypeId, '9001015000088', 1, 'Real', 'Estate', 'realestate@test.com', @SystemUserId, @StatusId, 1, 0, '1234', '1234');
    SET @NewCustomerId = SCOPE_IDENTITY();

    INSERT INTO Agents (CustomerId, CustomerTypeId, IdentificationTypeId, IdentificationNumber, TitleTypeId, FirstName, LastName, EmailAddress, StatusId, IsActive, IsDeleted, PhysicalAddressCode, PostalAddressCode, DepartmentId)
    VALUES (@NewCustomerId, @CustomerTypeId, @IdTypeId, '9001015000088', 1, 'Real', 'Estate', 'realestate@test.com', @StatusId, 1, 0, '1234', '1234', 3);
END
ELSE
BEGIN
    UPDATE SystemUsers SET DepartmentId = 3 WHERE UserName = 'RealEstateCustomer';
    
    DECLARE @ExistingSysUserId INT = (SELECT Id FROM SystemUsers WHERE UserName = 'RealEstateCustomer');
    DECLARE @ExistingCustId INT = (SELECT Id FROM Customers WHERE SystemUserId = @ExistingSysUserId);
    
    IF @ExistingCustId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Agents WHERE CustomerId = @ExistingCustId)
    BEGIN
        DECLARE @StatusId2 INT = (SELECT TOP 1 Id FROM Status WHERE [Key] = 's_customer_active');
        DECLARE @CustomerTypeId2 INT = (SELECT TOP 1 Id FROM CustomerTypes WHERE [Key] = 'ct_individual');
        DECLARE @IdTypeId2 INT = (SELECT TOP 1 Id FROM IdentificationTypes WHERE [Key] = 'id_south_african');
        
        INSERT INTO Agents (CustomerId, CustomerTypeId, IdentificationTypeId, IdentificationNumber, TitleTypeId, FirstName, LastName, EmailAddress, StatusId, IsActive, IsDeleted, PhysicalAddressCode, PostalAddressCode, DepartmentId)
        VALUES (@ExistingCustId, @CustomerTypeId2, @IdTypeId2, '9001015000088', 1, 'Real', 'Estate', 'realestate@test.com', @StatusId2, 1, 0, '1234', '1234', 3);
    END
END

-- 2. re_finance_officer
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE UserName = 're_finance_officer')
BEGIN
    INSERT INTO SystemUsers (FirstName, LastName, UserName, EmailAddress, IsActive, IsDeleted, IsPasswordReset, IsTemporaryPassword, IsIAMRegistered)
    VALUES ('Finance', 'Officer', 're_finance_officer', 'finance_officer@siyakhokha-test.gov.za', 1, 0, 1, 0, 1);
    SET @SystemUserId = SCOPE_IDENTITY();

    SET @AspNetUserId = 'a001a001-beef-4c7b-a2fc-b04afb57b3c2';
    INSERT INTO AspNetUsers (Id, SystemUserId, UserName, PasswordHash, SecurityStamp, Email, EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
    VALUES (@AspNetUserId, @SystemUserId, 're_finance_officer', @PwdHash, @SecurityStamp, 'finance_officer@siyakhokha-test.gov.za', 1, 0, 0, 1, 0);

    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@AspNetUserId, @Role_Finance);
END

-- 3. re_property_officer
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE UserName = 're_property_officer')
BEGIN
    INSERT INTO SystemUsers (FirstName, LastName, UserName, EmailAddress, IsActive, IsDeleted, IsPasswordReset, IsTemporaryPassword, IsIAMRegistered)
    VALUES ('Property', 'Officer', 're_property_officer', 'property_officer@siyakhokha-test.gov.za', 1, 0, 1, 0, 1);
    SET @SystemUserId = SCOPE_IDENTITY();

    SET @AspNetUserId = 'a001a001-beef-4c7b-a2fc-b04afb57b3c3';
    INSERT INTO AspNetUsers (Id, SystemUserId, UserName, PasswordHash, SecurityStamp, Email, EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
    VALUES (@AspNetUserId, @SystemUserId, 're_property_officer', @PwdHash, @SecurityStamp, 'property_officer@siyakhokha-test.gov.za', 1, 0, 0, 1, 0);

    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@AspNetUserId, @Role_Property);
END

-- 4. re_committee_member
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE UserName = 're_committee_member')
BEGIN
    INSERT INTO SystemUsers (FirstName, LastName, UserName, EmailAddress, IsActive, IsDeleted, IsPasswordReset, IsTemporaryPassword, IsIAMRegistered)
    VALUES ('Committee', 'Member', 're_committee_member', 'committee_member@siyakhokha-test.gov.za', 1, 0, 1, 0, 1);
    SET @SystemUserId = SCOPE_IDENTITY();

    SET @AspNetUserId = 'a001a001-beef-4c7b-a2fc-b04afb57b3c4';
    INSERT INTO AspNetUsers (Id, SystemUserId, UserName, PasswordHash, SecurityStamp, Email, EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
    VALUES (@AspNetUserId, @SystemUserId, 're_committee_member', @PwdHash, @SecurityStamp, 'committee_member@siyakhokha-test.gov.za', 1, 0, 0, 1, 0);

    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@AspNetUserId, @Role_Area);
END

-- 5. re_hod
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE UserName = 're_hod')
BEGIN
    INSERT INTO SystemUsers (FirstName, LastName, UserName, EmailAddress, IsActive, IsDeleted, IsPasswordReset, IsTemporaryPassword, IsIAMRegistered)
    VALUES ('HOD', 'RealEstate', 're_hod', 'hod_realestate@siyakhokha-test.gov.za', 1, 0, 1, 0, 1);
    SET @SystemUserId = SCOPE_IDENTITY();

    SET @AspNetUserId = 'a001a001-beef-4c7b-a2fc-b04afb57b3c5';
    INSERT INTO AspNetUsers (Id, SystemUserId, UserName, PasswordHash, SecurityStamp, Email, EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
    VALUES (@AspNetUserId, @SystemUserId, 're_hod', @PwdHash, @SecurityStamp, 'hod_realestate@siyakhokha-test.gov.za', 1, 0, 0, 1, 0);

    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@AspNetUserId, @Role_BackOffice);
END

-- 6. re_facilities_manager
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE UserName = 're_facilities_manager')
BEGIN
    INSERT INTO SystemUsers (FirstName, LastName, UserName, EmailAddress, IsActive, IsDeleted, IsPasswordReset, IsTemporaryPassword, IsIAMRegistered)
    VALUES ('Facilities', 'Manager', 're_facilities_manager', 'fac_manager@siyakhokha-test.gov.za', 1, 0, 1, 0, 1);
    SET @SystemUserId = SCOPE_IDENTITY();

    SET @AspNetUserId = 'a001a001-beef-4c7b-a2fc-b04afb57b3c6';
    INSERT INTO AspNetUsers (Id, SystemUserId, UserName, PasswordHash, SecurityStamp, Email, EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
    VALUES (@AspNetUserId, @SystemUserId, 're_facilities_manager', @PwdHash, @SecurityStamp, 'fac_manager@siyakhokha-test.gov.za', 1, 0, 0, 1, 0);

    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@AspNetUserId, @Role_FacManager);
END

-- 7. re_technician
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE UserName = 're_technician')
BEGIN
    INSERT INTO SystemUsers (FirstName, LastName, UserName, EmailAddress, IsActive, IsDeleted, IsPasswordReset, IsTemporaryPassword, IsIAMRegistered)
    VALUES ('Technician', 'Electrician', 're_technician', 'technician@siyakhokha-test.gov.za', 1, 0, 1, 0, 1);
    SET @SystemUserId = SCOPE_IDENTITY();

    SET @AspNetUserId = 'a001a001-beef-4c7b-a2fc-b04afb57b3c7';
    INSERT INTO AspNetUsers (Id, SystemUserId, UserName, PasswordHash, SecurityStamp, Email, EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
    VALUES (@AspNetUserId, @SystemUserId, 're_technician', @PwdHash, @SecurityStamp, 'technician@siyakhokha-test.gov.za', 1, 0, 0, 1, 0);

    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@AspNetUserId, @Role_Technician);
END
GO

-- 13. Seed missing workflow statuses, responsibility types, and activity tracker messages
DECLARE @RcsStatusTypeId INT = (SELECT TOP 1 Id FROM StatusTypes WHERE [Key] = 'st_rcs');
DECLARE @CustomerAccountStatusTypeId INT = (SELECT TOP 1 Id FROM StatusTypes WHERE [Key] = 'st_customer_account');

-- Real Estate Statuses (under st_rcs)
IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 're_in_circulation_for_evaluation')
    INSERT INTO Status ([Key], [Name], [Description], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES ('re_in_circulation_for_evaluation', 'In Circulation for Evaluation', 'Real Estate: Application is in circulation for departmental review', @RcsStatusTypeId, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 're_supported')
    INSERT INTO Status ([Key], [Name], [Description], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES ('re_supported', 'Supported', 'Real Estate: Department supported', @RcsStatusTypeId, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 're_supported_conditions')
    INSERT INTO Status ([Key], [Name], [Description], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES ('re_supported_conditions', 'Supported with Conditions', 'Real Estate: Department supported with conditions', @RcsStatusTypeId, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 're_not_supported')
    INSERT INTO Status ([Key], [Name], [Description], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES ('re_not_supported', 'Not Supported', 'Real Estate: Department not supported', @RcsStatusTypeId, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 're_additional_info_req')
    INSERT INTO Status ([Key], [Name], [Description], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES ('re_additional_info_req', 'Additional Information Required', 'Real Estate: Department requested additional info', @RcsStatusTypeId, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 're_pending_committee_outcome')
    INSERT INTO Status ([Key], [Name], [Description], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES ('re_pending_committee_outcome', 'Pending Committee Outcome', 'Real Estate: Pending DPRE Evaluation Committee review', @RcsStatusTypeId, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 're_recommended')
    INSERT INTO Status ([Key], [Name], [Description], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES ('re_recommended', 'Recommended', 'Real Estate: Recommended by Committee', @RcsStatusTypeId, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 're_recommended_conditions')
    INSERT INTO Status ([Key], [Name], [Description], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES ('re_recommended_conditions', 'Recommended with Conditions', 'Real Estate: Recommended with conditions by Committee', @RcsStatusTypeId, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 're_not_recommended')
    INSERT INTO Status ([Key], [Name], [Description], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES ('re_not_recommended', 'Not Recommended', 'Real Estate: Not recommended by Committee', @RcsStatusTypeId, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 're_deferred')
    INSERT INTO Status ([Key], [Name], [Description], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES ('re_deferred', 'Deferred', 'Real Estate: Deferred by Committee', @RcsStatusTypeId, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 're_concluded_approved')
    INSERT INTO Status ([Key], [Name], [Description], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES ('re_concluded_approved', 'Concluded Approved', 'Real Estate: Approved by HoD', @RcsStatusTypeId, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 're_concluded_rejected')
    INSERT INTO Status ([Key], [Name], [Description], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES ('re_concluded_rejected', 'Concluded Rejected', 'Real Estate: Rejected by HoD', @RcsStatusTypeId, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 're_awaiting_inspection')
    INSERT INTO Status ([Key], [Name], [Description], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES ('re_awaiting_inspection', 'Awaiting Inspection', 'Real Estate: Awaiting unit inspection', @RcsStatusTypeId, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 're_awaiting_agreement_conclusion')
    INSERT INTO Status ([Key], [Name], [Description], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES ('re_awaiting_agreement_conclusion', 'Awaiting Agreement Conclusion', 'Real Estate: Awaiting lease agreement signing', @RcsStatusTypeId, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 're_awaiting_pto_review')
    INSERT INTO Status ([Key], [Name], [Description], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES ('re_awaiting_pto_review', 'Awaiting PTO Review', 'Real Estate: Awaiting Permission to Occupy review', @RcsStatusTypeId, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 're_awaiting_pto_approval')
    INSERT INTO Status ([Key], [Name], [Description], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES ('re_awaiting_pto_approval', 'Awaiting PTO Approval', 'Real Estate: Awaiting Permission to Occupy final approval', @RcsStatusTypeId, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 're_pto_approved')
    INSERT INTO Status ([Key], [Name], [Description], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES ('re_pto_approved', 'PTO Approved', 'Real Estate: Permission to Occupy approved', @RcsStatusTypeId, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 're_pto_rejected')
    INSERT INTO Status ([Key], [Name], [Description], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES ('re_pto_rejected', 'PTO Rejected', 'Real Estate: Permission to Occupy rejected', @RcsStatusTypeId, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 're_pto_approved_conditions')
    INSERT INTO Status ([Key], [Name], [Description], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES ('re_pto_approved_conditions', 'PTO Approved with Conditions', 'Real Estate: Permission to Occupy approved with conditions', @RcsStatusTypeId, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 're_pto_additional_info')
    INSERT INTO Status ([Key], [Name], [Description], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES ('re_pto_additional_info', 'PTO Additional Info Required', 'Real Estate: Permission to Occupy additional info required', @RcsStatusTypeId, 1, 0, GETDATE());

-- Dispute/Eviction Statuses (under st_rcs)
IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_awaiting_eviction_service')
    INSERT INTO Status ([Key], [Name], [Description], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES ('s_awaiting_eviction_service', 'Awaiting Eviction Service', 'Eviction notice approved — awaiting physical service', @RcsStatusTypeId, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_eviction_notice_served')
    INSERT INTO Status ([Key], [Name], [Description], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES ('s_eviction_notice_served', 'Eviction Notice Served', 'Notice served — awaiting proof of service', @RcsStatusTypeId, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_awaiting_proof_of_service')
    INSERT INTO Status ([Key], [Name], [Description], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES ('s_awaiting_proof_of_service', 'Awaiting Proof of Service', 'Notice served — proof of service required', @RcsStatusTypeId, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_proof_of_service_captured')
    INSERT INTO Status ([Key], [Name], [Description], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES ('s_proof_of_service_captured', 'Proof of Service Captured', 'Proof captured — advancing to exit inspection', @RcsStatusTypeId, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_dispute_open_awaiting_review')
    INSERT INTO Status ([Key], [Name], [Description], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES ('s_dispute_open_awaiting_review', 'Dispute Open — Awaiting Review', 'New dispute registered, pending CSO review', @RcsStatusTypeId, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_dispute_referred')
    INSERT INTO Status ([Key], [Name], [Description], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES ('s_dispute_referred', 'Dispute Referred', 'Dispute reviewed and referred for resolution', @RcsStatusTypeId, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_dispute_resolved')
    INSERT INTO Status ([Key], [Name], [Description], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES ('s_dispute_resolved', 'Dispute Resolved', 'Revenue Manager resolved — awaiting CEO closure', @RcsStatusTypeId, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_dispute_closed')
    INSERT INTO Status ([Key], [Name], [Description], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES ('s_dispute_closed', 'Dispute Closed', 'Dispute closed by CEO', @RcsStatusTypeId, 1, 0, GETDATE());

-- Refund Statuses (under st_customer_account)
IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_awaiting_refund_authorisation')
    INSERT INTO Status ([Key], [Name], [Description], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES ('s_awaiting_refund_authorisation', 'Awaiting Refund Authorisation', 'Refund reviewed by RM — awaiting CEO approval', @CustomerAccountStatusTypeId, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_former_tenant')
    INSERT INTO Status ([Key], [Name], [Description], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES ('s_former_tenant', 'Former Tenant', 'Tenant who has successfully vacated and completed final processes', @CustomerAccountStatusTypeId, 1, 0, GETDATE());

-- Responsibility Types
IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key] = 'r_cso_termination_review')
    INSERT INTO ResponsibilityTypes ([Key], [Name], [Description], IsActive, IsDeleted, CreatedDateTime)
    VALUES ('r_cso_termination_review', 'CSO Termination Review', 'CSO review of termination request', 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key] = 'r_eviction_ceo_auth')
    INSERT INTO ResponsibilityTypes ([Key], [Name], [Description], IsActive, IsDeleted, CreatedDateTime)
    VALUES ('r_eviction_ceo_auth', 'CEO Eviction Authorization', 'CEO authorization of eviction', 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key] = 'r_eviction_service')
    INSERT INTO ResponsibilityTypes ([Key], [Name], [Description], IsActive, IsDeleted, CreatedDateTime)
    VALUES ('r_eviction_service', 'Eviction Service', 'UC025 — Serve eviction notice', 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key] = 'r_proof_of_service')
    INSERT INTO ResponsibilityTypes ([Key], [Name], [Description], IsActive, IsDeleted, CreatedDateTime)
    VALUES ('r_proof_of_service', 'Proof of Service', 'UC025 — Capture proof of service', 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key] = 'r_dispute_review')
    INSERT INTO ResponsibilityTypes ([Key], [Name], [Description], IsActive, IsDeleted, CreatedDateTime)
    VALUES ('r_dispute_review', 'Dispute Review', 'UC026 — CSO dispute review', 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key] = 'r_dispute_resolution')
    INSERT INTO ResponsibilityTypes ([Key], [Name], [Description], IsActive, IsDeleted, CreatedDateTime)
    VALUES ('r_dispute_resolution', 'Dispute Resolution', 'UC026 — RM dispute resolution', 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key] = 'r_dispute_closure')
    INSERT INTO ResponsibilityTypes ([Key], [Name], [Description], IsActive, IsDeleted, CreatedDateTime)
    VALUES ('r_dispute_closure', 'Dispute Closure', 'UC026 — CEO dispute closure', 1, 0, GETDATE());

-- Activity Tracker Messages
IF NOT EXISTS (SELECT 1 FROM ActivityTrackerMessages WHERE [Key] = 'at_eviction_notice_served')
    INSERT INTO ActivityTrackerMessages ([Key], [Name], [Description], IsActive, IsDeleted, CreatedDateTime)
    VALUES ('at_eviction_notice_served', 'Eviction Notice Served', 'The eviction notice has been served on the tenant', 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM ActivityTrackerMessages WHERE [Key] = 'at_proof_of_service_captured')
    INSERT INTO ActivityTrackerMessages ([Key], [Name], [Description], IsActive, IsDeleted, CreatedDateTime)
    VALUES ('at_proof_of_service_captured', 'Proof of Service Captured', 'Proof of eviction notice service has been captured', 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM ActivityTrackerMessages WHERE [Key] = 'at_dispute_registered')
    INSERT INTO ActivityTrackerMessages ([Key], [Name], [Description], IsActive, IsDeleted, CreatedDateTime)
    VALUES ('at_dispute_registered', 'Dispute Registered', 'A lease dispute has been registered', 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM ActivityTrackerMessages WHERE [Key] = 'at_dispute_reviewed')
    INSERT INTO ActivityTrackerMessages ([Key], [Name], [Description], IsActive, IsDeleted, CreatedDateTime)
    VALUES ('at_dispute_reviewed', 'Dispute Reviewed', 'The dispute has been reviewed by CSO', 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM ActivityTrackerMessages WHERE [Key] = 'at_dispute_referred_legal')
    INSERT INTO ActivityTrackerMessages ([Key], [Name], [Description], IsActive, IsDeleted, CreatedDateTime)
    VALUES ('at_dispute_referred_legal', 'Dispute Referred to Legal', 'The dispute has been referred to legal services', 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM ActivityTrackerMessages WHERE [Key] = 'at_dispute_resolved')
    INSERT INTO ActivityTrackerMessages ([Key], [Name], [Description], IsActive, IsDeleted, CreatedDateTime)
    VALUES ('at_dispute_resolved', 'Dispute Resolved', 'The dispute has been resolved by Revenue Manager', 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM ActivityTrackerMessages WHERE [Key] = 'at_dispute_not_resolved')
    INSERT INTO ActivityTrackerMessages ([Key], [Name], [Description], IsActive, IsDeleted, CreatedDateTime)
    VALUES ('at_dispute_not_resolved', 'Dispute Not Resolved', 'Revenue Manager could not resolve the dispute', 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM ActivityTrackerMessages WHERE [Key] = 'at_dispute_closed_ceo')
    INSERT INTO ActivityTrackerMessages ([Key], [Name], [Description], IsActive, IsDeleted, CreatedDateTime)
    VALUES ('at_dispute_closed_ceo', 'Dispute Closed by CEO', 'The dispute has been officially closed by the CEO', 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM ActivityTrackerMessages WHERE [Key] = 'at_dispute_rejected_ceo')
    INSERT INTO ActivityTrackerMessages ([Key], [Name], [Description], IsActive, IsDeleted, CreatedDateTime)
    VALUES ('at_dispute_rejected_ceo', 'Dispute Rejected by CEO', 'The CEO has rejected the dispute closure', 1, 0, GETDATE());
GO

PRINT '--- REAL ESTATE DATABASE SETUP COMPLETE ---';
GO
