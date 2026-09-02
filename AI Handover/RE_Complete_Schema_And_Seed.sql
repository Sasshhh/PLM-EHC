-- ====================================================================================
-- CITY OF EKURHULENI (CoE) - PROPERTY LEASE MANAGEMENT SYSTEM (PLM V1)
-- CONSOLIDATED DATABASE SETUP & MASTERDATA SEED SCRIPT FOR REAL ESTATE DEVELOPMENT (RED)
-- ====================================================================================
-- Target Database: PropertyLeaseManagementRealEstate
-- Includes:
-- 1. Core Schema Definition (RE_ tables)
-- 2. Nullable Banking Details Alterations
-- 3. Inventory & Tariff Matrix Master Data Seeding
-- 4. Vetting Departments, Workflow Roles, and Representative Users Seeding
-- 5. PTO & Lease Agreement Contract Execution Table Columns (UC21 - UC25)
-- 6. Workflow Statuses, Responsibility Types, and Activity Messages Seeding
-- ====================================================================================

USE [PropertyLeaseManagementRealEstate]
GO

SET NOCOUNT ON;
PRINT '=== STARTING CONSOLIDATED REAL ESTATE SETUP ===';

-- ------------------------------------------------------------------------------------
-- SECTION 1: CORE TABLES SETUP
-- ------------------------------------------------------------------------------------

PRINT '1. Creating core Real Estate tables...';

-- 1. RE_FacilityCategories
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

-- 2. RE_Facilities
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

-- 3. RE_FacilityUnits
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

-- 4. RE_Applications
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

        -- Banking Details (Nullable)
        [BankName] NVARCHAR(100) NULL,
        [BankAccountType] NVARCHAR(50) NULL,
        [BankAccountName] NVARCHAR(150) NULL,
        [BankAccountNumber] NVARCHAR(100) NULL,
        [BankBranchCode] NVARCHAR(50) NULL,

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

        -- Vetting / Risk Outcome
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

-- Shared table linking schemas
IF OBJECT_ID('dbo.Documents', 'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.Documents', 'RealEstateApplicationId') IS NULL
    BEGIN
        ALTER TABLE dbo.Documents ADD RealEstateApplicationId INT NULL;
        ALTER TABLE dbo.Documents ADD CONSTRAINT FK_Documents_RE_Applications FOREIGN KEY (RealEstateApplicationId) REFERENCES dbo.RE_Applications (Id);
        PRINT 'Altered Table: Documents - Linked to RE_Applications';
    END
END;

IF OBJECT_ID('dbo.PLMApplicationHistortyLogs', 'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.PLMApplicationHistortyLogs', 'RealEstateApplicationId') IS NULL
    BEGIN
        ALTER TABLE dbo.PLMApplicationHistortyLogs ADD RealEstateApplicationId INT NULL;
        ALTER TABLE dbo.PLMApplicationHistortyLogs ADD CONSTRAINT FK_PLMApplicationHistortyLogs_RE_Applications FOREIGN KEY (RealEstateApplicationId) REFERENCES dbo.RE_Applications (Id);
        PRINT 'Altered Table: PLMApplicationHistortyLogs - Linked to RE_Applications';
    END
END;

-- 5. RE_DepartmentalComments
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

-- ------------------------------------------------------------------------------------
-- SECTION 2: NULLABLE BANKING ALTERATIONS
-- ------------------------------------------------------------------------------------

PRINT '2. Configuring banking details nullability...';
ALTER TABLE RE_Applications ALTER COLUMN BankName NVARCHAR(100) NULL;
ALTER TABLE RE_Applications ALTER COLUMN BankAccountType NVARCHAR(50) NULL;
ALTER TABLE RE_Applications ALTER COLUMN BankAccountName NVARCHAR(150) NULL;
ALTER TABLE RE_Applications ALTER COLUMN BankAccountNumber NVARCHAR(100) NULL;
ALTER TABLE RE_Applications ALTER COLUMN BankBranchCode NVARCHAR(50) NULL;

-- ------------------------------------------------------------------------------------
-- SECTION 3: AUDIT TABLES SETUP
-- ------------------------------------------------------------------------------------

PRINT '3. Creating audit tables...';

IF OBJECT_ID('dbo.RE_FacilityAudits', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[RE_FacilityAudits] (
        [AuditId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Action] NVARCHAR(10) NOT NULL,
        [Id] INT NOT NULL,
        [Name] NVARCHAR(250) NOT NULL,
        [CCCId] INT NOT NULL,
        [Address] NVARCHAR(500) NULL,
        [DepartmentId] INT NULL,
        [IsActive] BIT NOT NULL,
        [IsDeleted] BIT NOT NULL,
        [IsLocked] BIT NULL,
        [CreatedBySystemUserId] INT NULL,
        [CreatedDateTime] DATETIME NULL,
        [ModifiedBySystemUserId] INT NULL,
        [ModifiedDateTime] DATETIME NULL
    );
END;

IF OBJECT_ID('dbo.RE_FacilityCategoryAudits', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[RE_FacilityCategoryAudits] (
        [AuditId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Action] NVARCHAR(10) NOT NULL,
        [Id] INT NOT NULL,
        [Key] NVARCHAR(100) NOT NULL,
        [Name] NVARCHAR(250) NOT NULL,
        [TariffPerSqm] DECIMAL(18,2) NOT NULL,
        [DepartmentId] INT NULL,
        [IsActive] BIT NOT NULL,
        [IsDeleted] BIT NOT NULL,
        [IsLocked] BIT NULL,
        [CreatedBySystemUserId] INT NULL,
        [CreatedDateTime] DATETIME NULL,
        [ModifiedBySystemUserId] INT NULL,
        [ModifiedDateTime] DATETIME NULL
    );
END;

IF OBJECT_ID('dbo.RE_FacilityUnitAudits', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[RE_FacilityUnitAudits] (
        [AuditId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Action] NVARCHAR(10) NOT NULL,
        [Id] INT NOT NULL,
        [FacilityId] INT NOT NULL,
        [FacilityCategoryId] INT NOT NULL,
        [UnitType] NVARCHAR(150) NOT NULL,
        [UnitSize] DECIMAL(18,2) NOT NULL,
        [MaxUnits] INT NOT NULL,
        [DepartmentId] INT NULL,
        [IsActive] BIT NOT NULL,
        [IsDeleted] BIT NOT NULL,
        [IsLocked] BIT NULL,
        [CreatedBySystemUserId] INT NULL,
        [CreatedDateTime] DATETIME NULL,
        [ModifiedBySystemUserId] INT NULL,
        [ModifiedDateTime] DATETIME NULL
    );
END;

IF OBJECT_ID('dbo.RE_DepartmentalCommentAudits', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[RE_DepartmentalCommentAudits] (
        [AuditId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
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
        [IsActive] BIT NOT NULL,
        [IsDeleted] BIT NOT NULL,
        [IsLocked] BIT NULL,
        [CreatedBySystemUserId] INT NULL,
        [CreatedDateTime] DATETIME NULL,
        [ModifiedBySystemUserId] INT NULL,
        [ModifiedDateTime] DATETIME NULL
    );
END;

IF OBJECT_ID('dbo.RE_ApplicationsAudit', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[RE_ApplicationsAudit] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
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
        [BankName] NVARCHAR(100) NULL,
        [BankAccountType] NVARCHAR(50) NULL,
        [BankAccountName] NVARCHAR(150) NULL,
        [BankAccountNumber] NVARCHAR(100) NULL,
        [BankBranchCode] NVARCHAR(50) NULL,
        [PurposeOfLease] NVARCHAR(100) NOT NULL,
        [CCCId] INT NOT NULL,
        [ErfFarmNumber] NVARCHAR(100) NOT NULL,
        [PropertyAddress] NVARCHAR(500) NOT NULL,
        [TownshipSuburbFarmName] NVARCHAR(200) NOT NULL,
        [PropertyPostalCode] NVARCHAR(20) NOT NULL,
        [FacilityOutdoorAdvertising] BIT NOT NULL,
        [FacilityTelecommunications] BIT NOT NULL,
        [FacilityInformalTrading] BIT NOT NULL,
        [FacilityTaxiRankTrading] BIT NOT NULL,
        [FacilityVocationalSkills] BIT NOT NULL,
        [FacilityComputerTraining] BIT NOT NULL,
        [FacilityIndustrialPark] BIT NOT NULL,
        [FacilityBusinessHub] BIT NOT NULL,
        [FacilityAutomotiveHub] BIT NOT NULL,
        [FacilityAgriPark] BIT NOT NULL,
        [FacilityIncubationFarm] BIT NOT NULL,
        
        [Action] NVARCHAR(50) NOT NULL,
        [StatusId] INT NOT NULL,
        [IsActive] BIT NOT NULL,
        [IsDeleted] BIT NOT NULL,
        [IsLocked] BIT NULL,
        [ModifiedBySystemUserId] INT NULL,
        [ModifiedDateTime] DATETIME NULL,
        [DepartmentId] INT NULL,

        [SelectedFacilityId] INT NULL,
        [SelectedFacilityUnitId] INT NULL,
        [SelectedUnitCount] INT NULL,
        [CalculatedMonthlyRental] DECIMAL(18,2) NULL,

        [CreditBureauResult] NVARCHAR(MAX) NULL,
        [HomeAffairsResult] NVARCHAR(MAX) NULL,
        [DeedsResult] NVARCHAR(MAX) NULL,
        [SassaResult] NVARCHAR(MAX) NULL,
        [CipcResult] NVARCHAR(MAX) NULL,
        [RiskAssessmentRecommendation] NVARCHAR(MAX) NULL,
        [RiskAssessmentReason] NVARCHAR(MAX) NULL,
        [RiskAssessmentEvidenceFileId] INT NULL,
        [PaymentValidationComment] NVARCHAR(MAX) NULL,

        [CommitteeDecision] NVARCHAR(MAX) NULL,
        [CommitteeComments] NVARCHAR(MAX) NULL,
        [CommitteeResolutionFileId] INT NULL,
        [FinalOutcome] NVARCHAR(MAX) NULL,
        [FinalComments] NVARCHAR(MAX) NULL,
        [FinalSignature] NVARCHAR(MAX) NULL,

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
        [PtoSignature] NVARCHAR(MAX) NULL
    );
    PRINT 'Created Table: RE_ApplicationsAudit';
END;

-- ------------------------------------------------------------------------------------
-- SECTION 4: INVENTORY MASTER DATA SEEDING
-- ------------------------------------------------------------------------------------

PRINT '4. Seeding Customer Care Centers (CCCs)...';
SET IDENTITY_INSERT [dbo].[CCCs] ON;
IF NOT EXISTS (SELECT 1 FROM [dbo].[CCCs] WHERE Id = 10)
    INSERT INTO [dbo].[CCCs] (Id, CCCTypeId, CCCName, Prefix, IsActive, IsDeleted) VALUES (10, 1, 'Tokoza', '90', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CCCs] WHERE Id = 11)
    INSERT INTO [dbo].[CCCs] (Id, CCCTypeId, CCCName, Prefix, IsActive, IsDeleted) VALUES (11, 1, 'Tsakane', '91', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CCCs] WHERE Id = 12)
    INSERT INTO [dbo].[CCCs] (Id, CCCTypeId, CCCName, Prefix, IsActive, IsDeleted) VALUES (12, 1, 'Kwa - Thema', '92', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CCCs] WHERE Id = 13)
    INSERT INTO [dbo].[CCCs] (Id, CCCTypeId, CCCName, Prefix, IsActive, IsDeleted) VALUES (13, 1, 'Daveyton', '93', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CCCs] WHERE Id = 14)
    INSERT INTO [dbo].[CCCs] (Id, CCCTypeId, CCCName, Prefix, IsActive, IsDeleted) VALUES (14, 1, 'Etwatwa', '94', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CCCs] WHERE Id = 15)
    INSERT INTO [dbo].[CCCs] (Id, CCCTypeId, CCCName, Prefix, IsActive, IsDeleted) VALUES (15, 1, 'Thembisa', '95', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CCCs] WHERE Id = 16)
    INSERT INTO [dbo].[CCCs] (Id, CCCTypeId, CCCName, Prefix, IsActive, IsDeleted) VALUES (16, 1, 'Katlehong', '96', 1, 0);
SET IDENTITY_INSERT [dbo].[CCCs] OFF;

PRINT 'Seeding Facility Categories...';
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

PRINT 'Seeding Facilities...';
SET IDENTITY_INSERT [dbo].[RE_Facilities] ON;
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 1)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted) VALUES (1, 'Fannie Malape Co-operatives Industrial Hive Centre', 10, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 2)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted) VALUES (2, 'Tokoza Traders Market', 10, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 3)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted) VALUES (3, 'Tsakane Business Park', 11, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 4)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted) VALUES (4, 'KwaThema Business Park', 12, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 5)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted) VALUES (5, 'Springs Traders Market', 4, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 6)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted) VALUES (6, 'Brakpan Civic Centre Kiosk', 6, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 7)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted) VALUES (7, 'Oscar Mabika Co-operatives Industrial Hive Centre', 13, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 8)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted) VALUES (8, 'Barcelona Traders Market', 14, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 9)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted) VALUES (9, 'Etwatwa Business Hive', 14, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 10)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted) VALUES (10, 'Etwatwa Unserviced Portions', 14, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 11)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted) VALUES (11, 'Bomba Sibiya Co-ops Industrial Hive Centre', 15, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 12)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted) VALUES (12, 'Tembisa Business Park', 15, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 13)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted) VALUES (13, 'Motsu Buy Back Centre', 15, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 14)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted) VALUES (14, 'Sethokga Buy Back Centre', 15, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 15)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted) VALUES (15, 'Sethokga Traders Market', 15, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 16)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted) VALUES (16, 'Sedibeng Hive Centre', 15, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 17)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted) VALUES (17, 'Katlehong Automotive manufacturing hub', 16, '', 1, 0);
SET IDENTITY_INSERT [dbo].[RE_Facilities] OFF;

PRINT 'Seeding Facility Units...';
DECLARE @CatParks INT = (SELECT Id FROM [dbo].[RE_FacilityCategories] WHERE [Key] = 'township_industrial_parks');
DECLARE @CatHubs INT = (SELECT Id FROM [dbo].[RE_FacilityCategories] WHERE [Key] = 'township_business_hubs');
DECLARE @CatAuto INT = (SELECT Id FROM [dbo].[RE_FacilityCategories] WHERE [Key] = 'township_automotive_hubs');
DECLARE @CatFab INT = (SELECT Id FROM [dbo].[RE_FacilityCategories] WHERE [Key] = 'fablab_facilities');
DECLARE @CatFarms INT = (SELECT Id FROM [dbo].[RE_FacilityCategories] WHERE [Key] = 'incubation_farms');

SET IDENTITY_INSERT [dbo].[RE_FacilityUnits] ON;
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 1)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted) VALUES (1, 1, @CatParks, 'Indoor Unit', 65.00, 4, 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 2)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted) VALUES (2, 2, @CatHubs, 'Indoor Unit', 65.00, 4, 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 3)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted) VALUES (3, 3, @CatHubs, 'Offices', 10.00, 5, 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 4)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted) VALUES (4, 3, @CatHubs, 'Single Garage Size Units', 18.00, 10, 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 5)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted) VALUES (5, 3, @CatHubs, 'Double Garage Size Units', 36.00, 10, 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 6)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted) VALUES (6, 3, @CatHubs, 'Kiosk', 17.00, 12, 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 7)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted) VALUES (7, 4, @CatHubs, 'Indoor Unit (Block A)', 21.00, 32, 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 8)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted) VALUES (8, 4, @CatHubs, 'Indoor Unit (Block B)', 42.50, 12, 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 9)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted) VALUES (9, 5, @CatHubs, 'Small Shop', 20.00, 21, 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 10)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted) VALUES (10, 6, @CatHubs, 'Kiosk', 17.00, 1, 1, 0);
SET IDENTITY_INSERT [dbo].[RE_FacilityUnits] OFF;

-- ------------------------------------------------------------------------------------
-- SECTION 5: ROLES AND WORKFLOW USERS SEEDING
-- ------------------------------------------------------------------------------------

PRINT '5. Seeding Security Roles & Test Accounts...';

-- Ensure Roles exist
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Caretaker')
    INSERT INTO AspNetRoles (Id, Name) VALUES ('e141a06a-68a8-4e76-a06b-c0da7682dc06', 'Caretaker');

-- Seeding roles
DECLARE @Role_Customer NVARCHAR(128) = '9524ba14-6dea-44af-b2e4-c94f8980a412';
DECLARE @Role_Finance NVARCHAR(128) = '95660a1d-c242-49cc-a549-1097a5a419f9';
DECLARE @Role_Property NVARCHAR(128) = 'c23949ee-e73d-4d87-a466-6a47b28bd0d3';
DECLARE @Role_Area NVARCHAR(128) = 'cc8e81f0-8440-4740-a746-e51a9f49d68d';
DECLARE @Role_BackOffice NVARCHAR(128) = '90103409-5e37-493f-b80a-20e216178cba';
DECLARE @Role_FacManager NVARCHAR(128) = 'ADDBE943-6C29-47C6-A69A-CFDE77F6FE4D';
DECLARE @Role_Technician NVARCHAR(128) = 'e141a06a-68a8-4e76-a06b-c0da7682dc06';

-- Passwords variables (Arsenal5@)
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
END;

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
END;

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
END;

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
END;

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
END;

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
END;

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
END;

-- ------------------------------------------------------------------------------------
-- SECTION 6: CONTRACT SIGNING & PTO COLUMNS (UC21 - UC25 SCHEMA)
-- ------------------------------------------------------------------------------------

PRINT '6. Adding contract signing and PTO columns to RE_Applications...';

-- Add columns if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RE_Applications]') AND name = N'PtoRevocationReason')
    ALTER TABLE [dbo].[RE_Applications] ADD [PtoRevocationReason] NVARCHAR(MAX) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RE_Applications]') AND name = N'PtoRevocationDate')
    ALTER TABLE [dbo].[RE_Applications] ADD [PtoRevocationDate] DATETIME NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RE_Applications]') AND name = N'LeaseAgreementFileId')
    ALTER TABLE [dbo].[RE_Applications] ADD [LeaseAgreementFileId] INT NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RE_Applications]') AND name = N'LeaseAgreementSignedFileId')
    ALTER TABLE [dbo].[RE_Applications] ADD [LeaseAgreementSignedFileId] INT NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RE_Applications]') AND name = N'LeaseAgreementTenantSignatureDate')
    ALTER TABLE [dbo].[RE_Applications] ADD [LeaseAgreementTenantSignatureDate] DATETIME NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RE_Applications]') AND name = N'LeaseAgreementHodSignature')
    ALTER TABLE [dbo].[RE_Applications] ADD [LeaseAgreementHodSignature] NVARCHAR(MAX) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RE_Applications]') AND name = N'LeaseAgreementHodSignatureDate')
    ALTER TABLE [dbo].[RE_Applications] ADD [LeaseAgreementHodSignatureDate] DATETIME NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RE_Applications]') AND name = N'LeaseCategory')
    ALTER TABLE [dbo].[RE_Applications] ADD [LeaseCategory] NVARCHAR(100) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RE_Applications]') AND name = N'UniqueTenancyLeaseNumber')
    ALTER TABLE [dbo].[RE_Applications] ADD [UniqueTenancyLeaseNumber] NVARCHAR(100) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RE_Applications]') AND name = N'LeaseStartDate')
    ALTER TABLE [dbo].[RE_Applications] ADD [LeaseStartDate] DATETIME NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RE_Applications]') AND name = N'LeaseEndDate')
    ALTER TABLE [dbo].[RE_Applications] ADD [LeaseEndDate] DATETIME NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RE_Applications]') AND name = N'LeaseDateOfOccupation')
    ALTER TABLE [dbo].[RE_Applications] ADD [LeaseDateOfOccupation] DATETIME NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RE_Applications]') AND name = N'LeaseEscalationTerms')
    ALTER TABLE [dbo].[RE_Applications] ADD [LeaseEscalationTerms] NVARCHAR(MAX) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RE_Applications]') AND name = N'LeasePaymentFrequency')
    ALTER TABLE [dbo].[RE_Applications] ADD [LeasePaymentFrequency] NVARCHAR(100) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RE_Applications]') AND name = N'LeaseDepositAmount')
    ALTER TABLE [dbo].[RE_Applications] ADD [LeaseDepositAmount] DECIMAL(18,2) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RE_Applications]') AND name = N'LeaseStatus')
    ALTER TABLE [dbo].[RE_Applications] ADD [LeaseStatus] NVARCHAR(100) NULL;

-- FKs for RE_Applications
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_RE_Applications_LeaseAgreementFile]') AND parent_object_id = OBJECT_ID(N'[dbo].[RE_Applications]'))
    ALTER TABLE [dbo].[RE_Applications] ADD CONSTRAINT [FK_RE_Applications_LeaseAgreementFile] FOREIGN KEY ([LeaseAgreementFileId]) REFERENCES [dbo].[Files]([Id]);

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_RE_Applications_LeaseAgreementSignedFile]') AND parent_object_id = OBJECT_ID(N'[dbo].[RE_Applications]'))
    ALTER TABLE [dbo].[RE_Applications] ADD CONSTRAINT [FK_RE_Applications_LeaseAgreementSignedFile] FOREIGN KEY ([LeaseAgreementSignedFileId]) REFERENCES [dbo].[Files]([Id]);

-- Add columns to audit table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RE_ApplicationsAudit]') AND name = N'PtoRevocationReason')
    ALTER TABLE [dbo].[RE_ApplicationsAudit] ADD [PtoRevocationReason] NVARCHAR(MAX) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RE_ApplicationsAudit]') AND name = N'PtoRevocationDate')
    ALTER TABLE [dbo].[RE_ApplicationsAudit] ADD [PtoRevocationDate] DATETIME NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RE_ApplicationsAudit]') AND name = N'LeaseAgreementFileId')
    ALTER TABLE [dbo].[RE_ApplicationsAudit] ADD [LeaseAgreementFileId] INT NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RE_ApplicationsAudit]') AND name = N'LeaseAgreementSignedFileId')
    ALTER TABLE [dbo].[RE_ApplicationsAudit] ADD [LeaseAgreementSignedFileId] INT NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RE_ApplicationsAudit]') AND name = N'LeaseAgreementTenantSignatureDate')
    ALTER TABLE [dbo].[RE_ApplicationsAudit] ADD [LeaseAgreementTenantSignatureDate] DATETIME NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RE_ApplicationsAudit]') AND name = N'LeaseAgreementHodSignature')
    ALTER TABLE [dbo].[RE_ApplicationsAudit] ADD [LeaseAgreementHodSignature] NVARCHAR(MAX) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RE_ApplicationsAudit]') AND name = N'LeaseAgreementHodSignatureDate')
    ALTER TABLE [dbo].[RE_ApplicationsAudit] ADD [LeaseAgreementHodSignatureDate] DATETIME NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RE_ApplicationsAudit]') AND name = N'LeaseCategory')
    ALTER TABLE [dbo].[RE_ApplicationsAudit] ADD [LeaseCategory] NVARCHAR(100) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RE_ApplicationsAudit]') AND name = N'UniqueTenancyLeaseNumber')
    ALTER TABLE [dbo].[RE_ApplicationsAudit] ADD [UniqueTenancyLeaseNumber] NVARCHAR(100) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RE_ApplicationsAudit]') AND name = N'LeaseStartDate')
    ALTER TABLE [dbo].[RE_ApplicationsAudit] ADD [LeaseStartDate] DATETIME NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RE_ApplicationsAudit]') AND name = N'LeaseEndDate')
    ALTER TABLE [dbo].[RE_ApplicationsAudit] ADD [LeaseEndDate] DATETIME NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RE_ApplicationsAudit]') AND name = N'LeaseDateOfOccupation')
    ALTER TABLE [dbo].[RE_ApplicationsAudit] ADD [LeaseDateOfOccupation] DATETIME NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RE_ApplicationsAudit]') AND name = N'LeaseEscalationTerms')
    ALTER TABLE [dbo].[RE_ApplicationsAudit] ADD [LeaseEscalationTerms] NVARCHAR(MAX) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RE_ApplicationsAudit]') AND name = N'LeasePaymentFrequency')
    ALTER TABLE [dbo].[RE_ApplicationsAudit] ADD [LeasePaymentFrequency] NVARCHAR(100) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RE_ApplicationsAudit]') AND name = N'LeaseDepositAmount')
    ALTER TABLE [dbo].[RE_ApplicationsAudit] ADD [LeaseDepositAmount] DECIMAL(18,2) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RE_ApplicationsAudit]') AND name = N'LeaseStatus')
    ALTER TABLE [dbo].[RE_ApplicationsAudit] ADD [LeaseStatus] NVARCHAR(100) NULL;

-- ------------------------------------------------------------------------------------
-- SECTION 7: SEEDING REAL ESTATE WORKFLOW METADATA & STATUSES
-- ------------------------------------------------------------------------------------

PRINT '7. Seeding workflow statuses and routing metadata...';

DECLARE @RcsStatusTypeId INT = (SELECT TOP 1 Id FROM StatusTypes WHERE [Key] = 'st_rcs');

-- Add new Statuses under st_rcs / StatusTypeId = 20
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
    VALUES ('re_awaiting_agreement_conclusion', 'Awaiting Lease/User Agreement Conclusion', 'Real Estate: Awaiting lease agreement signing', @RcsStatusTypeId, 1, 0, GETDATE());

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

-- UC21-25 Added Statuses (under StatusTypeId = 20)
IF NOT EXISTS (SELECT 1 FROM [dbo].[Status] WHERE [Key] = 're_awaiting_pto_signature')
    INSERT INTO [dbo].[Status] ([Name], [Key], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime]) 
    VALUES ('Awaiting PTO Signature', 're_awaiting_pto_signature', 20, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Status] WHERE [Key] = 're_awaiting_agreement_conclusion_outcome')
    INSERT INTO [dbo].[Status] ([Name], [Key], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime]) 
    VALUES ('Awaiting Lease/User Agreement Conclusion Outcome', 're_awaiting_agreement_conclusion_outcome', 20, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Status] WHERE [Key] = 're_pending_activation')
    INSERT INTO [dbo].[Status] ([Name], [Key], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime]) 
    VALUES ('Pending Activation', 're_pending_activation', 20, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Status] WHERE [Key] = 're_active_occupancy')
    INSERT INTO [dbo].[Status] ([Name], [Key], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime]) 
    VALUES ('Active Occupancy', 're_active_occupancy', 20, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Status] WHERE [Key] = 're_active')
    INSERT INTO [dbo].[Status] ([Name], [Key], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime]) 
    VALUES ('Active', 're_active', 20, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Status] WHERE [Key] = 're_revoked_pending_review')
    INSERT INTO [dbo].[Status] ([Name], [Key], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime]) 
    VALUES ('Revoked, Pending Review', 're_revoked_pending_review', 20, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Status] WHERE [Key] = 're_expired')
    INSERT INTO [dbo].[Status] ([Name], [Key], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime]) 
    VALUES ('Expired', 're_expired', 20, 1, 0, GETDATE());

PRINT '=== CONSOLIDATED SETUP SCRIPT EXECUTION FINISHED ===';
GO
