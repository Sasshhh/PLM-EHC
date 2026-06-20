-- =============================================
-- Complaints Module Seed Data
-- Script: seed_complaint_lookup_data.sql
-- Purpose: Insert initial lookup data for complaint categories, types, and statuses
-- Date: 2025-03-14
-- =============================================

USE [CRMPLMDEV_2025]
GO

-- =============================================
-- 1. Insert Complaint Status Entries
-- =============================================
PRINT 'Inserting Complaint Status entries...'

-- Check if statuses already exist
IF NOT EXISTS (SELECT 1 FROM [dbo].[Status] WHERE [Key] = 'complaint_status_submitted')
BEGIN
    INSERT INTO [dbo].[Status] ([Key], [Status], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('complaint_status_submitted', 'Submitted', 1, 0, GETDATE())
    PRINT 'Inserted: complaint_status_submitted'
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Status] WHERE [Key] = 'complaint_status_awaiting_appointment')
BEGIN
    INSERT INTO [dbo].[Status] ([Key], [Status], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('complaint_status_awaiting_appointment', 'Awaiting Appointment', 1, 0, GETDATE())
    PRINT 'Inserted: complaint_status_awaiting_appointment'
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Status] WHERE [Key] = 'complaint_status_awaiting_investigation')
BEGIN
    INSERT INTO [dbo].[Status] ([Key], [Status], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('complaint_status_awaiting_investigation', 'Awaiting Investigation', 1, 0, GETDATE())
    PRINT 'Inserted: complaint_status_awaiting_investigation'
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Status] WHERE [Key] = 'complaint_status_resolved')
BEGIN
    INSERT INTO [dbo].[Status] ([Key], [Status], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('complaint_status_resolved', 'Resolved', 1, 0, GETDATE())
    PRINT 'Inserted: complaint_status_resolved'
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Status] WHERE [Key] = 'complaint_status_referred')
BEGIN
    INSERT INTO [dbo].[Status] ([Key], [Status], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('complaint_status_referred', 'Referred', 1, 0, GETDATE())
    PRINT 'Inserted: complaint_status_referred'
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Status] WHERE [Key] = 'complaint_status_unresolved')
BEGIN
    INSERT INTO [dbo].[Status] ([Key], [Status], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('complaint_status_unresolved', 'Unresolved', 1, 0, GETDATE())
    PRINT 'Inserted: complaint_status_unresolved'
END

PRINT 'Complaint statuses seeded successfully!'
PRINT ''

-- =============================================
-- 2. Insert Complaint Categories
-- =============================================
PRINT 'Inserting Complaint Categories...'

DECLARE @CategoryId INT

-- Administration
IF NOT EXISTS (SELECT 1 FROM [dbo].[ComplaintCategories] WHERE [Key] = 'Administration')
BEGIN
    INSERT INTO [dbo].[ComplaintCategories] ([Name], [Key], [Description], [DisplayOrder], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('Administration', 'Administration', 'Administrative complaints including subletting and rule violations', 1, 1, 0, GETDATE())
    SET @CategoryId = SCOPE_IDENTITY()
    PRINT 'Inserted Category: Administration (ID: ' + CAST(@CategoryId AS VARCHAR) + ')'
    
    -- Insert Types for Administration
    INSERT INTO [dbo].[ComplaintTypes] ([ComplaintCategoryId], [Name], [Key], [Description], [DisplayOrder], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES 
        (@CategoryId, 'Sub-letting', 'SubLetting', 'Tenant subletting the unit to another party', 1, 1, 0, GETDATE()),
        (@CategoryId, 'In violation of Complex Rules', 'ViolationOfRules', 'Violation of complex rules and regulations', 2, 1, 0, GETDATE())
    PRINT '  - Inserted 2 types for Administration'
END

-- Nuisance and Behavioural
IF NOT EXISTS (SELECT 1 FROM [dbo].[ComplaintCategories] WHERE [Key] = 'NuisanceAndBehavioural')
BEGIN
    INSERT INTO [dbo].[ComplaintCategories] ([Name], [Key], [Description], [DisplayOrder], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('Nuisance and Behavioural', 'NuisanceAndBehavioural', 'Complaints about noise, odors, and behavioral issues', 2, 1, 0, GETDATE())
    SET @CategoryId = SCOPE_IDENTITY()
    PRINT 'Inserted Category: Nuisance and Behavioural (ID: ' + CAST(@CategoryId AS VARCHAR) + ')'
    
    -- Insert Types for Nuisance and Behavioural
    INSERT INTO [dbo].[ComplaintTypes] ([ComplaintCategoryId], [Name], [Key], [Description], [DisplayOrder], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES 
        (@CategoryId, 'Noise Disturbance', 'NoiseDisturbance', 'Excessive noise from neighboring unit', 1, 1, 0, GETDATE()),
        (@CategoryId, 'Odors and Fumes', 'OdorsAndFumes', 'Unpleasant odors or harmful fumes', 2, 1, 0, GETDATE()),
        (@CategoryId, 'Aggressive Behaviour', 'AggressiveBehaviour', 'Threatening or aggressive behavior by tenant', 3, 1, 0, GETDATE()),
        (@CategoryId, 'Children', 'Children', 'Issues related to unsupervised children', 4, 1, 0, GETDATE())
    PRINT '  - Inserted 4 types for Nuisance and Behavioural'
END

-- Parking and Vehicle
IF NOT EXISTS (SELECT 1 FROM [dbo].[ComplaintCategories] WHERE [Key] = 'ParkingAndVehicle')
BEGIN
    INSERT INTO [dbo].[ComplaintCategories] ([Name], [Key], [Description], [DisplayOrder], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('Parking and Vehicle', 'ParkingAndVehicle', 'Complaints about parking violations and vehicle issues', 3, 1, 0, GETDATE())
    SET @CategoryId = SCOPE_IDENTITY()
    PRINT 'Inserted Category: Parking and Vehicle (ID: ' + CAST(@CategoryId AS VARCHAR) + ')'
    
    -- Insert Types for Parking and Vehicle
    INSERT INTO [dbo].[ComplaintTypes] ([ComplaintCategoryId], [Name], [Key], [Description], [DisplayOrder], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES 
        (@CategoryId, 'Obstructive Parking', 'ObstructiveParking', 'Vehicle blocking access or parking improperly', 1, 1, 0, GETDATE()),
        (@CategoryId, 'Unauthorised Parking', 'UnauthorisedParking', 'Parking in unauthorized areas', 2, 1, 0, GETDATE()),
        (@CategoryId, 'Car Wash', 'CarWash', 'Washing vehicles in unauthorized areas', 3, 1, 0, GETDATE())
    PRINT '  - Inserted 3 types for Parking and Vehicle'
END

-- Pet and Animals
IF NOT EXISTS (SELECT 1 FROM [dbo].[ComplaintCategories] WHERE [Key] = 'PetAndAnimals')
BEGIN
    INSERT INTO [dbo].[ComplaintCategories] ([Name], [Key], [Description], [DisplayOrder], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('Pet and Animals', 'PetAndAnimals', 'Complaints about pets and animal-related issues', 4, 1, 0, GETDATE())
    SET @CategoryId = SCOPE_IDENTITY()
    PRINT 'Inserted Category: Pet and Animals (ID: ' + CAST(@CategoryId AS VARCHAR) + ')'
    
    -- Insert Types for Pet and Animals
    INSERT INTO [dbo].[ComplaintTypes] ([ComplaintCategoryId], [Name], [Key], [Description], [DisplayOrder], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES 
        (@CategoryId, 'Nuisance Pets', 'NuisancePets', 'Pets causing disturbance (barking, aggression)', 1, 1, 0, GETDATE()),
        (@CategoryId, 'Unapproved Pets', 'UnapprovedPets', 'Keeping pets without approval', 2, 1, 0, GETDATE())
    PRINT '  - Inserted 2 types for Pet and Animals'
END

-- Property Usage
IF NOT EXISTS (SELECT 1 FROM [dbo].[ComplaintCategories] WHERE [Key] = 'PropertyUsage')
BEGIN
    INSERT INTO [dbo].[ComplaintCategories] ([Name], [Key], [Description], [DisplayOrder], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('Property Usage', 'PropertyUsage', 'Complaints about improper property usage and maintenance', 5, 1, 0, GETDATE())
    SET @CategoryId = SCOPE_IDENTITY()
    PRINT 'Inserted Category: Property Usage (ID: ' + CAST(@CategoryId AS VARCHAR) + ')'
    
    -- Insert Types for Property Usage
    INSERT INTO [dbo].[ComplaintTypes] ([ComplaintCategoryId], [Name], [Key], [Description], [DisplayOrder], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES 
        (@CategoryId, 'Untidy Areas', 'UntidyAreas', 'Common areas or unit kept in untidy condition', 1, 1, 0, GETDATE()),
        (@CategoryId, 'Neglected Property', 'Neglected', 'Property not maintained properly', 2, 1, 0, GETDATE()),
        (@CategoryId, 'Unapproved Alterations', 'UnapprovedAlterations', 'Unauthorized modifications to property', 3, 1, 0, GETDATE()),
        (@CategoryId, 'Misuse of Property', 'Misuse', 'Using property for unauthorized purposes', 4, 1, 0, GETDATE())
    PRINT '  - Inserted 4 types for Property Usage'
END

-- Safety & Security
IF NOT EXISTS (SELECT 1 FROM [dbo].[ComplaintCategories] WHERE [Key] = 'SafetyAndSecurity')
BEGIN
    INSERT INTO [dbo].[ComplaintCategories] ([Name], [Key], [Description], [DisplayOrder], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('Safety & Security', 'SafetyAndSecurity', 'Complaints about safety and security violations', 6, 1, 0, GETDATE())
    SET @CategoryId = SCOPE_IDENTITY()
    PRINT 'Inserted Category: Safety & Security (ID: ' + CAST(@CategoryId AS VARCHAR) + ')'
    
    -- Insert Types for Safety & Security
    INSERT INTO [dbo].[ComplaintTypes] ([ComplaintCategoryId], [Name], [Key], [Description], [DisplayOrder], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES 
        (@CategoryId, 'Neglecting Security Rules', 'NeglectingSecurityRules', 'Not following security protocols', 1, 1, 0, GETDATE()),
        (@CategoryId, 'Other Safety Concerns', 'OtherSafety', 'Other safety-related issues', 2, 1, 0, GETDATE())
    PRINT '  - Inserted 2 types for Safety & Security'
END

-- Other
IF NOT EXISTS (SELECT 1 FROM [dbo].[ComplaintCategories] WHERE [Key] = 'Other')
BEGIN
    INSERT INTO [dbo].[ComplaintCategories] ([Name], [Key], [Description], [DisplayOrder], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('Other', 'Other', 'Other complaints not covered by specific categories', 7, 1, 0, GETDATE())
    SET @CategoryId = SCOPE_IDENTITY()
    PRINT 'Inserted Category: Other (ID: ' + CAST(@CategoryId AS VARCHAR) + ')'
    
    -- Insert Types for Other
    INSERT INTO [dbo].[ComplaintTypes] ([ComplaintCategoryId], [Name], [Key], [Description], [DisplayOrder], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES 
        (@CategoryId, 'Other (please specify)', 'OtherType', 'Other complaint not categorized', 1, 1, 0, GETDATE())
    PRINT '  - Inserted 1 type for Other'
END

PRINT ''
PRINT 'Complaint Categories and Types seeded successfully!'
PRINT ''

-- =============================================
-- 3. Verify Seed Data
-- =============================================
PRINT '=============================================
PRINT 'VERIFICATION SUMMARY'
PRINT '============================================='
PRINT ''
PRINT 'Status Entries:'
SELECT [Key], [Status] FROM [dbo].[Status] WHERE [Key] LIKE 'complaint_status_%' ORDER BY [Key]
PRINT ''
PRINT 'Categories:'
SELECT Id, [Name], [Key], DisplayOrder FROM [dbo].[ComplaintCategories] ORDER BY DisplayOrder
PRINT ''
PRINT 'Types (with Category Names):'
SELECT 
    ct.[Name] AS TypeName,
    ct.[Key] AS TypeKey,
    cc.[Name] AS CategoryName,
    ct.DisplayOrder
FROM [dbo].[ComplaintTypes] ct
INNER JOIN [dbo].[ComplaintCategories] cc ON ct.ComplaintCategoryId = cc.Id
ORDER BY cc.DisplayOrder, ct.DisplayOrder
PRINT ''
PRINT '============================================='
PRINT 'Seed data script completed successfully!'
PRINT '============================================='
GO
