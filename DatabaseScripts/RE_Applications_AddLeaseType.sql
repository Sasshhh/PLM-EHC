-- =====================================================================
-- DPRE UC05: lease type on lease applications (Business Hub, Commercial, ...)
-- Safe to re-run.
-- =====================================================================
IF COL_LENGTH('dbo.RE_Applications', 'LeaseType') IS NULL
BEGIN
    ALTER TABLE dbo.RE_Applications ADD LeaseType NVARCHAR(50) NULL;
    PRINT 'Added RE_Applications.LeaseType';
END
GO

IF OBJECT_ID('dbo.RE_ApplicationAudits', 'U') IS NOT NULL AND COL_LENGTH('dbo.RE_ApplicationAudits', 'LeaseType') IS NULL
BEGIN
    ALTER TABLE dbo.RE_ApplicationAudits ADD LeaseType NVARCHAR(50) NULL;
    PRINT 'Added RE_ApplicationAudits.LeaseType';
END
GO

-- Existing applications were all captured on the Business/Commercial Hub form
UPDATE dbo.RE_Applications SET LeaseType = 'Business Hub' WHERE LeaseType IS NULL;
GO
