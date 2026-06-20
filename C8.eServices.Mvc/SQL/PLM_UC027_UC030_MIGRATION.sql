-- ============================================================
-- PLM UC027 - UC030 MIGRATION SCRIPT
-- Exit Inspection, Vacating & Refund Authorization (CEO Sign-off)
-- ============================================================
-- Run against: eServicesDbContext database (CRMPLMDEV_2025)
-- Date: 2026-06-04
-- ============================================================

-- 1. ADD NEW COLUMNS TO LeaseTerminations TABLE
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'LeaseTerminations' AND type = 'U')
BEGIN
    -- Vacating Details (UC028)
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.LeaseTerminations') AND name = 'AccessCardNumber')
        ALTER TABLE dbo.LeaseTerminations ADD AccessCardNumber NVARCHAR(MAX) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.LeaseTerminations') AND name = 'KeyNumber')
        ALTER TABLE dbo.LeaseTerminations ADD KeyNumber NVARCHAR(MAX) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.LeaseTerminations') AND name = 'OtherPossessions')
        ALTER TABLE dbo.LeaseTerminations ADD OtherPossessions NVARCHAR(MAX) NULL;

    -- Refund capturing by Financial Officer (UC029)
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.LeaseTerminations') AND name = 'MaintenanceCost')
        ALTER TABLE dbo.LeaseTerminations ADD MaintenanceCost DECIMAL(18, 2) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.LeaseTerminations') AND name = 'ApprovedDeductions')
        ALTER TABLE dbo.LeaseTerminations ADD ApprovedDeductions DECIMAL(18, 2) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.LeaseTerminations') AND name = 'NettRefundAmount')
        ALTER TABLE dbo.LeaseTerminations ADD NettRefundAmount DECIMAL(18, 2) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.LeaseTerminations') AND name = 'FinancialOfficerOfficialNumber')
        ALTER TABLE dbo.LeaseTerminations ADD FinancialOfficerOfficialNumber NVARCHAR(MAX) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.LeaseTerminations') AND name = 'DepositRefundRecommended')
        ALTER TABLE dbo.LeaseTerminations ADD DepositRefundRecommended NVARCHAR(MAX) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.LeaseTerminations') AND name = 'FinancialOfficerReason')
        ALTER TABLE dbo.LeaseTerminations ADD FinancialOfficerReason NVARCHAR(MAX) NULL;

    -- Revenue Manager Refund support (UC030)
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.LeaseTerminations') AND name = 'RevenueManagerRefundSupport')
        ALTER TABLE dbo.LeaseTerminations ADD RevenueManagerRefundSupport BIT NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.LeaseTerminations') AND name = 'RevenueManagerRefundOfficialNumber')
        ALTER TABLE dbo.LeaseTerminations ADD RevenueManagerRefundOfficialNumber NVARCHAR(MAX) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.LeaseTerminations') AND name = 'RevenueManagerRefundReason')
        ALTER TABLE dbo.LeaseTerminations ADD RevenueManagerRefundReason NVARCHAR(MAX) NULL;

    -- CEO final authorization details (UC030)
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.LeaseTerminations') AND name = 'CEORefundResponse')
        ALTER TABLE dbo.LeaseTerminations ADD CEORefundResponse NVARCHAR(MAX) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.LeaseTerminations') AND name = 'CEORefundOfficialNumber')
        ALTER TABLE dbo.LeaseTerminations ADD CEORefundOfficialNumber NVARCHAR(MAX) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.LeaseTerminations') AND name = 'CEORefundSignature')
        ALTER TABLE dbo.LeaseTerminations ADD CEORefundSignature NVARCHAR(MAX) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.LeaseTerminations') AND name = 'CEORefundSignDate')
        ALTER TABLE dbo.LeaseTerminations ADD CEORefundSignDate DATETIME NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.LeaseTerminations') AND name = 'CEORefundReason')
        ALTER TABLE dbo.LeaseTerminations ADD CEORefundReason NVARCHAR(MAX) NULL;

    PRINT 'LeaseTerminations columns updated successfully.';
END
ELSE
BEGIN
    PRINT 'ERROR: LeaseTerminations table not found.';
END

-- 2. SEED WORKFLOW STATUSES
-- Seed 's_awaiting_refund_authorisation' under 'st_customer_account'
IF NOT EXISTS (SELECT 1 FROM [dbo].[Status] WHERE [Key] = 's_awaiting_refund_authorisation')
BEGIN
    INSERT INTO [dbo].[Status] ([Name],[Description],[Key],[StatusTypeId],[IsActive],[IsDeleted],[IsLocked],[CreatedDateTime])
    VALUES ('Awaiting Refund Authorisation', 'Refund reviewed by RM — awaiting CEO approval', 's_awaiting_refund_authorisation',
    (SELECT TOP 1 Id FROM StatusTypes WHERE [Key]='st_customer_account'), 1, 0, 0, GETDATE());
    PRINT 'Status s_awaiting_refund_authorisation seeded.';
END

-- Seed 's_former_tenant' under 'st_customer_account'
IF NOT EXISTS (SELECT 1 FROM [dbo].[Status] WHERE [Key] = 's_former_tenant')
BEGIN
    INSERT INTO [dbo].[Status] ([Name],[Description],[Key],[StatusTypeId],[IsActive],[IsDeleted],[IsLocked],[CreatedDateTime])
    VALUES ('Former Tenant', 'Tenant who has successfully vacated and completed final processes', 's_former_tenant',
    (SELECT TOP 1 Id FROM StatusTypes WHERE [Key]='st_customer_account'), 1, 0, 0, GETDATE());
    PRINT 'Status s_former_tenant seeded.';
END

PRINT '── UC027-UC030 Migration Script Completed ──';
GO
