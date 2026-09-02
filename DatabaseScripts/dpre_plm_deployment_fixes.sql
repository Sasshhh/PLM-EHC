USE [PropertyLeaseManagementRealEstate]
GO

PRINT '--- STARTING REAL ESTATE DEPLOYMENT SCHEMA FIXES ---';

-- 1. Ensure SelectedUnitsJson exists on RE_Applications
IF OBJECT_ID('dbo.RE_Applications', 'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.RE_Applications', 'SelectedUnitsJson') IS NULL
    BEGIN
        ALTER TABLE dbo.RE_Applications ADD SelectedUnitsJson NVARCHAR(MAX) NULL;
        PRINT 'Altered Table: RE_Applications - Added SelectedUnitsJson';
    END
END
GO

-- 2. Ensure SelectedUnitsJson exists on RE_ApplicationsAudit
IF OBJECT_ID('dbo.RE_ApplicationsAudit', 'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.RE_ApplicationsAudit', 'SelectedUnitsJson') IS NULL
    BEGIN
        ALTER TABLE dbo.RE_ApplicationsAudit ADD SelectedUnitsJson NVARCHAR(MAX) NULL;
        PRINT 'Altered Table: RE_ApplicationsAudit - Added SelectedUnitsJson';
    END
END
GO

-- 3. Ensure RealEstateApplicationId exists on Documents
IF OBJECT_ID('dbo.Documents', 'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.Documents', 'RealEstateApplicationId') IS NULL
    BEGIN
        ALTER TABLE dbo.Documents ADD RealEstateApplicationId INT NULL;
        IF OBJECT_ID('dbo.RE_Applications', 'U') IS NOT NULL
        BEGIN
            ALTER TABLE dbo.Documents ADD CONSTRAINT FK_Documents_RE_Applications FOREIGN KEY (RealEstateApplicationId) REFERENCES dbo.RE_Applications (Id);
        END
        PRINT 'Altered Table: Documents - Added RealEstateApplicationId';
    END
END
GO

-- 4. Ensure RealEstateApplicationId exists on DocumentAudits
IF OBJECT_ID('dbo.DocumentAudits', 'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.DocumentAudits', 'RealEstateApplicationId') IS NULL
    BEGIN
        ALTER TABLE dbo.DocumentAudits ADD RealEstateApplicationId INT NULL;
        PRINT 'Altered Table: DocumentAudits - Added RealEstateApplicationId';
    END
END
GO

-- 5. Ensure RealEstateApplicationId exists on PLMApplicationHistortyLogs
IF OBJECT_ID('dbo.PLMApplicationHistortyLogs', 'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.PLMApplicationHistortyLogs', 'RealEstateApplicationId') IS NULL
    BEGIN
        ALTER TABLE dbo.PLMApplicationHistortyLogs ADD RealEstateApplicationId INT NULL;
        IF OBJECT_ID('dbo.RE_Applications', 'U') IS NOT NULL
        BEGIN
            ALTER TABLE dbo.PLMApplicationHistortyLogs ADD CONSTRAINT FK_PLMApplicationHistortyLogs_RE_Applications FOREIGN KEY (RealEstateApplicationId) REFERENCES dbo.RE_Applications (Id);
        END
        PRINT 'Altered Table: PLMApplicationHistortyLogs - Added RealEstateApplicationId';
    END
END
GO

-- 6. Ensure RealEstateApplicationId exists on PLMApplicationHistortyLogAudits
IF OBJECT_ID('dbo.PLMApplicationHistortyLogAudits', 'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.PLMApplicationHistortyLogAudits', 'RealEstateApplicationId') IS NULL
    BEGIN
        ALTER TABLE dbo.PLMApplicationHistortyLogAudits ADD RealEstateApplicationId INT NULL;
        PRINT 'Altered Table: PLMApplicationHistortyLogAudits - Added RealEstateApplicationId';
    END
END
GO

-- 7. Ensure RealEstateApplicationId exists on RoundRobinQueues
IF OBJECT_ID('dbo.RoundRobinQueues', 'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.RoundRobinQueues', 'RealEstateApplicationId') IS NULL
    BEGIN
        ALTER TABLE dbo.RoundRobinQueues ADD RealEstateApplicationId INT NULL;
        IF OBJECT_ID('dbo.RE_Applications', 'U') IS NOT NULL
        BEGIN
            ALTER TABLE dbo.RoundRobinQueues ADD CONSTRAINT FK_RoundRobinQueues_RE_Applications FOREIGN KEY (RealEstateApplicationId) REFERENCES dbo.RE_Applications (Id);
        END
        PRINT 'Altered Table: RoundRobinQueues - Added RealEstateApplicationId';
    END
END
GO

-- 8. Ensure RealEstateApplicationId exists on RoundRobinQueueAudits
IF OBJECT_ID('dbo.RoundRobinQueueAudits', 'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.RoundRobinQueueAudits', 'RealEstateApplicationId') IS NULL
    BEGIN
        ALTER TABLE dbo.RoundRobinQueueAudits ADD RealEstateApplicationId INT NULL;
        PRINT 'Altered Table: RoundRobinQueueAudits - Added RealEstateApplicationId';
    END
END
GO

PRINT '--- REAL ESTATE DEPLOYMENT SCHEMA FIXES COMPLETE ---';
GO
