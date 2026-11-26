-- =====================================================================
-- DPRE UC05 Scenarios 2-4: Outdoor Advertising, Telecommunications and Concession/Leasehold
-- application details (RE_ApplicationSiteDetails + audit) and the Draft status for Save and Continue.
-- Safe to re-run.
-- =====================================================================

IF OBJECT_ID('dbo.RE_ApplicationSiteDetails', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[RE_ApplicationSiteDetails] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [RE_ApplicationId] INT NOT NULL,
        [FormCode] NVARCHAR(10) NULL,
        [FirstName] NVARCHAR(100) NULL,
        [Surname] NVARCHAR(100) NULL,
        [ApplicantTypeOther] NVARCHAR(100) NULL,
        [IdentificationType] NVARCHAR(60) NULL,
        [PostalAddress] NVARCHAR(500) NULL,
        [SignLocationType] NVARCHAR(60) NULL,
        [SignType] NVARCHAR(60) NULL,
        [SignTypeOther] NVARCHAR(100) NULL,
        [SignIlluminated] BIT NULL,
        [SignElevations] NVARCHAR(40) NULL,
        [SignElevationsOther] NVARCHAR(60) NULL,
        [AdvertisingSignReferenceNumber] NVARCHAR(60) NULL,
        [RealEstateReferenceNumber] NVARCHAR(60) NULL,
        [CityPlanningReferenceNumber] NVARCHAR(60) NULL,
        [FarmPortionDescription] NVARCHAR(250) NULL,
        [Town] NVARCHAR(60) NULL,
        [RoadReserveStreetName] NVARCHAR(150) NULL,
        [RoadReserveNearestIntersection] NVARCHAR(150) NULL,
        [RoadReserveAdjacentProperty] NVARCHAR(250) NULL,
        [SiteName] NVARCHAR(150) NULL,
        [SiteNumber] NVARCHAR(60) NULL,
        [Latitude] DECIMAL(18,6) NULL,
        [Longitude] DECIMAL(18,6) NULL,
        [InstallationType] NVARCHAR(120) NULL,
        [InstallationTypeOther] NVARCHAR(150) NULL,
        [AreaOnStructureSqm] DECIMAL(18,2) NULL,
        [AreaGroundSqm] DECIMAL(18,2) NULL,
        [OptionToRenew] BIT NULL,
        [CurrentlySublet] BIT NULL,
        [CurrentCoLocators] INT NULL,
        [WillBeSublet] BIT NULL,
        [AnticipatedCoLocators] INT NULL,
        [Zoning] NVARCHAR(100) NULL,
        [ProposedLeaseYears] INT NULL,
        [ProposedLeaseMonths] INT NULL,
        [ProposedUse] NVARCHAR(MAX) NULL,
        [TotalExtentSqm] DECIMAL(18,2) NULL,
        [TermsAccepted] BIT NOT NULL DEFAULT 0,
        [TermsAcceptedDateTime] DATETIME NULL,
        [SignatoryName] NVARCHAR(150) NULL,
        [SignatoryCapacity] NVARCHAR(100) NULL,
        [SignatureImage] NVARCHAR(MAX) NULL,
        [SubmittedDateTime] DATETIME NULL,
        [DepartmentId] INT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [IsLocked] BIT NULL DEFAULT 0,
        [CreatedBySystemUserId] INT NULL,
        [CreatedDateTime] DATETIME NULL,
        [ModifiedBySystemUserId] INT NULL,
        [ModifiedDateTime] DATETIME NULL,
        CONSTRAINT [PK_RE_ApplicationSiteDetails] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_RE_ApplicationSiteDetails_RE_Applications] FOREIGN KEY ([RE_ApplicationId]) REFERENCES [dbo].[RE_Applications] ([Id])
    );
    CREATE UNIQUE INDEX [UX_RE_ApplicationSiteDetails_Application] ON [dbo].[RE_ApplicationSiteDetails] ([RE_ApplicationId]);
    PRINT 'Created Table: RE_ApplicationSiteDetails';
END
GO

IF OBJECT_ID('dbo.RE_ApplicationSiteDetailAudits', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[RE_ApplicationSiteDetailAudits] (
        [AuditId] INT IDENTITY(1,1) NOT NULL,
        [Action] NVARCHAR(10) NOT NULL,
        [Id] INT NOT NULL,
        [RE_ApplicationId] INT NOT NULL,
        [FormCode] NVARCHAR(10) NULL,
        [FirstName] NVARCHAR(100) NULL,
        [Surname] NVARCHAR(100) NULL,
        [ApplicantTypeOther] NVARCHAR(100) NULL,
        [IdentificationType] NVARCHAR(60) NULL,
        [PostalAddress] NVARCHAR(500) NULL,
        [SignLocationType] NVARCHAR(60) NULL,
        [SignType] NVARCHAR(60) NULL,
        [SignTypeOther] NVARCHAR(100) NULL,
        [SignIlluminated] BIT NULL,
        [SignElevations] NVARCHAR(40) NULL,
        [SignElevationsOther] NVARCHAR(60) NULL,
        [AdvertisingSignReferenceNumber] NVARCHAR(60) NULL,
        [RealEstateReferenceNumber] NVARCHAR(60) NULL,
        [CityPlanningReferenceNumber] NVARCHAR(60) NULL,
        [FarmPortionDescription] NVARCHAR(250) NULL,
        [Town] NVARCHAR(60) NULL,
        [RoadReserveStreetName] NVARCHAR(150) NULL,
        [RoadReserveNearestIntersection] NVARCHAR(150) NULL,
        [RoadReserveAdjacentProperty] NVARCHAR(250) NULL,
        [SiteName] NVARCHAR(150) NULL,
        [SiteNumber] NVARCHAR(60) NULL,
        [Latitude] DECIMAL(18,6) NULL,
        [Longitude] DECIMAL(18,6) NULL,
        [InstallationType] NVARCHAR(120) NULL,
        [InstallationTypeOther] NVARCHAR(150) NULL,
        [AreaOnStructureSqm] DECIMAL(18,2) NULL,
        [AreaGroundSqm] DECIMAL(18,2) NULL,
        [OptionToRenew] BIT NULL,
        [CurrentlySublet] BIT NULL,
        [CurrentCoLocators] INT NULL,
        [WillBeSublet] BIT NULL,
        [AnticipatedCoLocators] INT NULL,
        [Zoning] NVARCHAR(100) NULL,
        [ProposedLeaseYears] INT NULL,
        [ProposedLeaseMonths] INT NULL,
        [ProposedUse] NVARCHAR(MAX) NULL,
        [TotalExtentSqm] DECIMAL(18,2) NULL,
        [TermsAccepted] BIT NOT NULL DEFAULT 0,
        [TermsAcceptedDateTime] DATETIME NULL,
        [SignatoryName] NVARCHAR(150) NULL,
        [SignatoryCapacity] NVARCHAR(100) NULL,
        [SignatureImage] NVARCHAR(MAX) NULL,
        [SubmittedDateTime] DATETIME NULL,
        [DepartmentId] INT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [IsLocked] BIT NULL DEFAULT 0,
        [CreatedBySystemUserId] INT NULL,
        [CreatedDateTime] DATETIME NULL,
        [ModifiedBySystemUserId] INT NULL,
        [ModifiedDateTime] DATETIME NULL,
        CONSTRAINT [PK_RE_ApplicationSiteDetailAudits] PRIMARY KEY CLUSTERED ([AuditId] ASC)
    );
    PRINT 'Created Table: RE_ApplicationSiteDetailAudits';
END
GO

-- Draft status for Save and Continue (not picked up by any back-office queue)
DECLARE @RcsStatusTypeId INT = (SELECT TOP 1 Id FROM dbo.StatusTypes WHERE [Key] = 'st_rcs');
IF NOT EXISTS (SELECT 1 FROM dbo.Status WHERE [Key] = 're_application_draft')
    INSERT INTO dbo.Status ([Key], [Name], [Description], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES ('re_application_draft', 'Draft (Not Submitted)', 'Real Estate: application saved by the applicant but not yet submitted', @RcsStatusTypeId, 1, 0, GETDATE());
GO
