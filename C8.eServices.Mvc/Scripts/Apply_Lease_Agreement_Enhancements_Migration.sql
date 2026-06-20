-- Manual Migration Script
-- Equivalent to 202603270800000_AddLeaseAgreementEnhancements.cs and 202603270810000_AddDSTVAndDepositFieldsToPropertyLeaseApplication.cs
-- Run on CRMPLMDEV_2025 database

USE CRMPLMDEV_2025;
GO

-- First Migration: Add columns to PropertyLeaseAgreementMasters table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PropertyLeaseAgreementMasters]') AND name = 'HasDSTV')
BEGIN
    ALTER TABLE [dbo].[PropertyLeaseAgreementMasters] 
    ADD [HasDSTV] bit NOT NULL DEFAULT 0
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PropertyLeaseAgreementMasters]') AND name = 'DSTVActivationFee')
BEGIN
    ALTER TABLE [dbo].[PropertyLeaseAgreementMasters] 
    ADD [DSTVActivationFee] decimal(18,2) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PropertyLeaseAgreementMasters]') AND name = 'DSTVMonthlyLevy')
BEGIN
    ALTER TABLE [dbo].[PropertyLeaseAgreementMasters] 
    ADD [DSTVMonthlyLevy] decimal(18,2) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PropertyLeaseAgreementMasters]') AND name = 'AccessCardDeposit')
BEGIN
    ALTER TABLE [dbo].[PropertyLeaseAgreementMasters] 
    ADD [AccessCardDeposit] decimal(18,2) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PropertyLeaseAgreementMasters]') AND name = 'KeyDeposit')
BEGIN
    ALTER TABLE [dbo].[PropertyLeaseAgreementMasters] 
    ADD [KeyDeposit] decimal(18,2) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PropertyLeaseAgreementMasters]') AND name = 'CommencementDay')
BEGIN
    ALTER TABLE [dbo].[PropertyLeaseAgreementMasters] 
    ADD [CommencementDay] nvarchar(max)
END
GO

-- Second Migration: Add columns to PropertyLeaseApplications table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PropertyLeaseApplications]') AND name = 'HasDSTV')
BEGIN
    ALTER TABLE [dbo].[PropertyLeaseApplications] 
    ADD [HasDSTV] bit NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PropertyLeaseApplications]') AND name = 'DSTVActivationFee')
BEGIN
    ALTER TABLE [dbo].[PropertyLeaseApplications] 
    ADD [DSTVActivationFee] decimal(18,2) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PropertyLeaseApplications]') AND name = 'DSTVMonthlyLevy')
BEGIN
    ALTER TABLE [dbo].[PropertyLeaseApplications] 
    ADD [DSTVMonthlyLevy] decimal(18,2) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PropertyLeaseApplications]') AND name = 'AccessCardDeposit')
BEGIN
    ALTER TABLE [dbo].[PropertyLeaseApplications] 
    ADD [AccessCardDeposit] decimal(18,2) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PropertyLeaseApplications]') AND name = 'KeyDeposit')
BEGIN
    ALTER TABLE [dbo].[PropertyLeaseApplications] 
    ADD [KeyDeposit] decimal(18,2) NULL
END
GO

-- Second Migration: Add columns to PropertyLeaseApplicationAudits table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PropertyLeaseApplicationAudits]') AND name = 'HasDSTV')
BEGIN
    ALTER TABLE [dbo].[PropertyLeaseApplicationAudits] 
    ADD [HasDSTV] bit NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PropertyLeaseApplicationAudits]') AND name = 'DSTVActivationFee')
BEGIN
    ALTER TABLE [dbo].[PropertyLeaseApplicationAudits] 
    ADD [DSTVActivationFee] decimal(18,2) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PropertyLeaseApplicationAudits]') AND name = 'DSTVMonthlyLevy')
BEGIN
    ALTER TABLE [dbo].[PropertyLeaseApplicationAudits] 
    ADD [DSTVMonthlyLevy] decimal(18,2) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PropertyLeaseApplicationAudits]') AND name = 'AccessCardDeposit')
BEGIN
    ALTER TABLE [dbo].[PropertyLeaseApplicationAudits] 
    ADD [AccessCardDeposit] decimal(18,2) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PropertyLeaseApplicationAudits]') AND name = 'KeyDeposit')
BEGIN
    ALTER TABLE [dbo].[PropertyLeaseApplicationAudits] 
    ADD [KeyDeposit] decimal(18,2) NULL
END
GO

-- Insert migration history records
IF NOT EXISTS (SELECT * FROM [dbo].[__MigrationHistory] WHERE MigrationId = '202603270800000_AddLeaseAgreementEnhancements')
BEGIN
    INSERT INTO [dbo].[__MigrationHistory] ([MigrationId], [ContextKey], [Model], [ProductVersion])
    VALUES ('202603270800000_AddLeaseAgreementEnhancements', 'C8.eServices.Mvc.Migrations.Configuration', 0x1F8B0800000000000400ECBD07601C499625262F6DCA7B7F4AF54AD7E074A10880601324D8904010ECC188CDE692EC1D69472329AB2A81CA6556655D661640CEED9DBCF7DE7BEFBDF7DE7BEFBDF7BA3B9D4E27F7DFDFCFCECDD7F7DFDFC7DDF3DFDE7DEBF7CEFCE9F33EFCEA9B5F5B4CF78AE89BE5DBF3CBFDFCA3E7F5CFDF7CECFB3E7CDFDC3DEFEF74DBEF79DEFEE7FEFC66FF5F5DBF7F66FFF4FFF5D7FFDF7CDFFBDF7DF9EBD93FEBF1EDFF9F9E8BFE3F07EABEF25EBBF3FE7F5D7B7FDA5BFDE5EEED7FF35EDF7CEDBF8F9FDF7FFC7F9F3EFEF3F3DEDBF7BDA38EEEBFBE3F1FFABFEE71F1FCFBFB7CE9D74FEDF16BB0D6F08A5FCEFBC5F76, 'EF6.4.0')
END
GO

IF NOT EXISTS (SELECT * FROM [dbo].[__MigrationHistory] WHERE MigrationId = '202603270810000_AddDSTVAndDepositFieldsToPropertyLeaseApplication')
BEGIN
    INSERT INTO [dbo].[__MigrationHistory] ([MigrationId], [ContextKey], [Model], [ProductVersion])
    VALUES ('202603270810000_AddDSTVAndDepositFieldsToPropertyLeaseApplication', 'C8.eServices.Mvc.Migrations.Configuration', 0x1F8B0800000000000400ECBD07601C499625262F6DCA7B7F4AF54AD7E074A10880601324D8904010ECC188CDE692EC1D69472329AB2A81CA6556655D661640CEED9DBCF7DE7BEFBDF7DE7BEFBDF7BA3B9D4E27F7DFDFCFCECDD7F7DFDFC7DDF3DFDE7DEBF7CEFCE9F33EFCEA9B5F5B4CF78AE89BE5DBF3CBFDFCA3E7F5CFDF7CECFB3E7CDFDC3DEFEF74DBEF79DEFEE7FEFC66FF5F5DBF7F66FFF4FFF5D7FFDF7CDFFBDF7DF9EBD93FEBF1EDFF9F9E8BFE3F07EABEF25EBBF3FE7F5D7B7FDA5BFDE5EEED7FF35EDF7CEDBF8F9FDF7FFC7F9F3EFEF3F3DEDBF7BDA38EEEBFBE3F1FFABFEE71F1FCFBFB7CE9D74FEDF16BB0D6F08A5FCEFBC5F76, 'EF6.4.0')
END
GO

PRINT 'Lease Agreement Enhancements Migration Completed Successfully!'
PRINT 'Added the following columns:'
PRINT '- PropertyLeaseAgreementMasters: HasDSTV, DSTVActivationFee, DSTVMonthlyLevy, AccessCardDeposit, KeyDeposit, CommencementDay'
PRINT '- PropertyLeaseApplications: HasDSTV, DSTVActivationFee, DSTVMonthlyLevy, AccessCardDeposit, KeyDeposit'  
PRINT '- PropertyLeaseApplicationAudits: HasDSTV, DSTVActivationFee, DSTVMonthlyLevy, AccessCardDeposit, KeyDeposit'