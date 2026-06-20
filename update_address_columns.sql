-- =================================================================================
-- Description: Updates the NVARCHAR column sizes of address and suburb fields in
--              both main tables and audit tables to match the new Model definitions
--              (ResAddress -> NVARCHAR(500), ResSuburb -> NVARCHAR(250))
-- =================================================================================

BEGIN TRANSACTION;
BEGIN TRY
    -- 1. Update PropertyLeaseApplications (Main Table)
    PRINT 'Updating PropertyLeaseApplications...';
    ALTER TABLE [dbo].[PropertyLeaseApplications] ALTER COLUMN [ResAddress] NVARCHAR(500) NULL;
    ALTER TABLE [dbo].[PropertyLeaseApplications] ALTER COLUMN [ResSuburb] NVARCHAR(250) NULL;

    -- 2. Update PropertyLeaseApplicationAudits (Audit Table)
    PRINT 'Updating PropertyLeaseApplicationAudits...';
    ALTER TABLE [dbo].[PropertyLeaseApplicationAudits] ALTER COLUMN [ResAddress] NVARCHAR(500) NULL;
    ALTER TABLE [dbo].[PropertyLeaseApplicationAudits] ALTER COLUMN [ResSuburb] NVARCHAR(250) NULL;

    -- 3. Update HumanSettlementApplications (Main Table)
    PRINT 'Updating HumanSettlementApplications...';
    ALTER TABLE [dbo].[HumanSettlementApplications] ALTER COLUMN [ResAddress] NVARCHAR(500) NULL;
    ALTER TABLE [dbo].[HumanSettlementApplications] ALTER COLUMN [ResSuburb] NVARCHAR(250) NULL;

    -- 4. Update HumanSettlementApplicationAudits (Audit Table)
    PRINT 'Updating HumanSettlementApplicationAudits...';
    ALTER TABLE [dbo].[HumanSettlementApplicationAudits] ALTER COLUMN [ResAddress] NVARCHAR(500) NULL;
    ALTER TABLE [dbo].[HumanSettlementApplicationAudits] ALTER COLUMN [ResSuburb] NVARCHAR(250) NULL;

    COMMIT TRANSACTION;
    PRINT 'All address column alterations executed successfully!';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'An error occurred during alterations. Transaction was rolled back.';
    THROW;
END CATCH;
