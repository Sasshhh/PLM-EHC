USE [CRMPLMDEV_2025]
GO

IF OBJECT_ID('dbo.RE_ApplicationsAudit', 'U') IS NOT NULL
    DROP TABLE [dbo].[RE_ApplicationsAudit];
GO

IF OBJECT_ID('dbo.RE_Applications', 'U') IS NOT NULL
    DROP TABLE [dbo].[RE_Applications];
GO

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

    CONSTRAINT [PK_RE_Applications] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_RE_Applications_SystemUsers] FOREIGN KEY ([SystemUserId]) REFERENCES [dbo].[SystemUsers] ([Id]),
    CONSTRAINT [FK_RE_Applications_Customers] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customers] ([Id]),
    CONSTRAINT [FK_RE_Applications_Status] FOREIGN KEY ([StatusId]) REFERENCES [dbo].[Status] ([Id]),
    CONSTRAINT [FK_RE_Applications_CCCs] FOREIGN KEY ([CCCId]) REFERENCES [dbo].[CCCs] ([Id])
);
GO

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

    CONSTRAINT [PK_RE_ApplicationsAudit] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO
