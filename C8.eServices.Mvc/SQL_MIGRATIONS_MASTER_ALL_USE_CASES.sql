-- =====================================================================================================
-- CITY OF EKURHULENI (CoE) - REAL ESTATE & LAND MANAGEMENT SYSTEM (PLM V1)
-- MASTER CONSOLIDATED DATABASE MIGRATIONS & MASTER DATA SCRIPT FOR ALL USE CASES (UC01 - UC25)
-- =====================================================================================================
-- Author: Antigravity AI Engineering
-- Date: 2026-07-29
-- Target Database: eServices_DB / CoE_RealEstate_DB
-- Execution Mode: Idempotent (Safe to run multiple times on Staging/Production hosted SQL Server)
-- =====================================================================================================

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

PRINT '=====================================================================================================';
PRINT 'STARTING CONSOLIDATED MIGRATIONS AND MASTER DATA SEEDING...';
PRINT '=====================================================================================================';

-- -----------------------------------------------------------------------------------------------------
-- SECTION 1: STATUS LOOKUP TABLE UPDATES & LIFECYCLE TRANSITION STATUSES
-- -----------------------------------------------------------------------------------------------------
PRINT 'Seeding Application Statuses into Statuses table...';

IF NOT EXISTS (SELECT 1 FROM [dbo].[Statuses] WHERE [StatusKey] = 'pto_cert_generated' OR [StatusName] = 'PTO Certificate Generated')
BEGIN
    INSERT INTO [dbo].[Statuses] ([StatusKey], [StatusName], [Description], [IsActive], [CreatedDateTime])
    VALUES ('pto_cert_generated', 'PTO Certificate Generated', 'Permission to Occupy (PTO) 12-Month Certificate generated and signed.', 1, GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Statuses] WHERE [StatusKey] = 'pto_cert_active' OR [StatusName] = 'PTO Certificate Active (12 Months)')
BEGIN
    INSERT INTO [dbo].[Statuses] ([StatusKey], [StatusName], [Description], [IsActive], [CreatedDateTime])
    VALUES ('pto_cert_active', 'PTO Certificate Active (12 Months)', 'PTO Certificate is active and valid for up to 12 months.', 1, GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Statuses] WHERE [StatusKey] = 'lease_drafted' OR [StatusName] = 'Lease Agreement Drafted')
BEGIN
    INSERT INTO [dbo].[Statuses] ([StatusKey], [StatusName], [Description], [IsActive], [CreatedDateTime])
    VALUES ('lease_drafted', 'Lease Agreement Drafted', '36-Month Full Lease Agreement drafted from approved back-office template.', 1, GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Statuses] WHERE [StatusKey] = 'lease_active' OR [StatusName] = 'Lease Agreement Active (36 Months)')
BEGIN
    INSERT INTO [dbo].[Statuses] ([StatusKey], [StatusName], [Description], [IsActive], [CreatedDateTime])
    VALUES ('lease_active', 'Lease Agreement Active (36 Months)', 'Full 3-Year (36 Months) Lease Agreement active.', 1, GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Statuses] WHERE [StatusKey] = 'pto_superseded' OR [StatusName] = 'PTO Superseded / Inactive')
BEGIN
    INSERT INTO [dbo].[Statuses] ([StatusKey], [StatusName], [Description], [IsActive], [CreatedDateTime])
    VALUES ('pto_superseded', 'PTO Superseded / Inactive', '12-Month PTO Certificate made inactive upon Back-Office activation of 36-Month Lease.', 1, GETDATE());
END

-- -----------------------------------------------------------------------------------------------------
-- SECTION 2: EVALUATION CRITERIA SCHEMA & MASTER TABLES (UC21 / UC25 3-TAB UI NAVIGATION)
-- -----------------------------------------------------------------------------------------------------

-- Table 1: Pre-Qualification Document Checklist
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RE_EvaluationCriteria_PreQualDoc')
BEGIN
    CREATE TABLE [dbo].[RE_EvaluationCriteria_PreQualDoc] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [DocumentName] NVARCHAR(250) NOT NULL,
        [IsMandatory] BIT NOT NULL DEFAULT 1,
        [Description] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedDateTime] DATETIME NOT NULL DEFAULT GETDATE()
    );
    PRINT 'Created table: RE_EvaluationCriteria_PreQualDoc';
END

-- Table 2: Evaluation Criteria & Scoring Matrix
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RE_EvaluationCriteria_PreQualEval')
BEGIN
    CREATE TABLE [dbo].[RE_EvaluationCriteria_PreQualEval] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [CriteriaCategory] NVARCHAR(200) NOT NULL,
        [CriteriaDescription] NVARCHAR(500) NOT NULL,
        [WeightScore] DECIMAL(5,2) NOT NULL DEFAULT 0.00,
        [PassingThreshold] DECIMAL(5,2) NOT NULL DEFAULT 0.00,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedDateTime] DATETIME NOT NULL DEFAULT GETDATE()
    );
    PRINT 'Created table: RE_EvaluationCriteria_PreQualEval';
END

-- Table 3: Committee Compliance Checklist
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RE_EvaluationCriteria_CommitteeChecklist')
BEGIN
    CREATE TABLE [dbo].[RE_EvaluationCriteria_CommitteeChecklist] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [ChecklistItem] NVARCHAR(250) NOT NULL,
        [ChecklistCategory] NVARCHAR(100) NOT NULL,
        [RequiredStatus] NVARCHAR(50) NOT NULL DEFAULT 'Compliant',
        [IsMandatory] BIT NOT NULL DEFAULT 1,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedDateTime] DATETIME NOT NULL DEFAULT GETDATE()
    );
    PRINT 'Created table: RE_EvaluationCriteria_CommitteeChecklist';
END

-- -----------------------------------------------------------------------------------------------------
-- SECTION 3: SEEDING MASTER DATA FOR 3-TAB EVALUATION CRITERIA VIEW
-- -----------------------------------------------------------------------------------------------------

-- Seed Tab 1: Pre-Qualification Document Checklist Data
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_EvaluationCriteria_PreQualDoc])
BEGIN
    INSERT INTO [dbo].[RE_EvaluationCriteria_PreQualDoc] ([DocumentName], [IsMandatory], [Description], [IsActive])
    VALUES 
    ('Identity Document (Certified Copy)', 1, 'Certified South African ID of Applicant / Director', 1),
    ('Tax Clearance Pin Certificate (SARS)', 1, 'Valid SARS Tax Compliance Status Pin', 1),
    ('CIPC Company Registration Certificate', 1, 'Official CIPC Registration Documents (CoR14.3)', 1),
    ('Proof of Residence / Municipal Account', 1, 'Recent Municipal Utility Statement (not older than 3 months)', 1),
    ('Detailed Business Plan & Cashflow Projection', 1, 'Operational Plan & 12-Month Financial Forecast', 1),
    ('3-Month Bank Statements', 1, 'Bank stamped financial statements', 1);
    PRINT 'Seeded RE_EvaluationCriteria_PreQualDoc master records.';
END

-- Seed Tab 2: Evaluation Criteria & Scoring Matrix Data
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_EvaluationCriteria_PreQualEval])
BEGIN
    INSERT INTO [dbo].[RE_EvaluationCriteria_PreQualEval] ([CriteriaCategory], [CriteriaDescription], [WeightScore], [PassingThreshold], [IsActive])
    VALUES 
    ('Financial Capability', 'Verification of sufficient operating capital and liquidity for municipal lease payments', 30.00, 60.00, 1),
    ('Business Experience & Track Record', 'Demonstrated expertise in sector and commercial operational readiness', 25.00, 50.00, 1),
    ('Local Enterprise & Job Creation', 'Empowerment of Ekurhuleni residents, youth, women, and PWDs', 25.00, 50.00, 1),
    ('Operational & Risk Compliance', 'Alignment with municipal by-laws, environmental, and safety standards', 20.00, 60.00, 1);
    PRINT 'Seeded RE_EvaluationCriteria_PreQualEval master records.';
END

-- Seed Tab 3: Committee Compliance Checklist Data
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_EvaluationCriteria_CommitteeChecklist])
BEGIN
    INSERT INTO [dbo].[RE_EvaluationCriteria_CommitteeChecklist] ([ChecklistItem], [ChecklistCategory], [RequiredStatus], [IsMandatory], [IsActive])
    VALUES 
    ('Zoning & Town Planning Approval', 'Land Use', 'Approved', 1, 1),
    ('Municipal Utility Accounts Good Standing', 'Finance', 'Cleared', 1, 1),
    ('Fire & Health Safety Clearance', 'Public Safety', 'Certified', 1, 1),
    ('Environmental Management Plan', 'Environment', 'Compliant', 1, 1),
    ('Back-Office Committee Resolution Sign-off', 'Governance', 'Approved', 1, 1);
    PRINT 'Seeded RE_EvaluationCriteria_CommitteeChecklist master records.';
END

-- -----------------------------------------------------------------------------------------------------
-- SECTION 4: REAL ESTATE FACILITIES & UNITS MASTER DATA SEEDING
-- -----------------------------------------------------------------------------------------------------

IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RE_Facilities')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE [Name] = 'Tsakane Business Park')
    BEGIN
        INSERT INTO [dbo].[RE_Facilities] ([Name], [Address], [Region], [FacilityType], [IsActive], [CreatedDateTime])
        VALUES ('Tsakane Business Park', '7522 Hlakwana Street, Tsakane', 'Region E - Ekurhuleni East', 'Business Park', 1, GETDATE());
    END

    IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE [Name] = 'Kwa Thema Business Hub')
    BEGIN
        INSERT INTO [dbo].[RE_Facilities] ([Name], [Address], [Region], [FacilityType], [IsActive], [CreatedDateTime])
        VALUES ('Kwa Thema Business Hub', 'Corner Thabahadi Road & Rhokana Road, Ext 3, Kwa-Thema', 'Region E - Ekurhuleni East', 'Incubator Centre', 1, GETDATE());
    END
END

PRINT '=====================================================================================================';
PRINT 'ALL MIGRATIONS AND MASTER DATA SEEDING COMPLETED SUCCESSFULLY!';
PRINT '=====================================================================================================';
GO
