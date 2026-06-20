-- =====================================================
-- Payment Transgressions Module (UC17C) - Master Data Script
-- Purpose: Insert all required master data for Payment Transgressions
-- This script is idempotent and safe to run multiple times
-- =====================================================

PRINT 'Starting Payment Transgressions Master Data Installation...'
GO

-- =====================================================
-- 1. STATUS TYPE FOR PAYMENT TRANSGRESSIONS
-- =====================================================
PRINT 'Installing Status Type for Payment Transgressions...'
GO

IF NOT EXISTS (SELECT 1 FROM StatusTypes WHERE [Key] = 'st_payment_transgressions')
BEGIN
    INSERT INTO StatusTypes ([Key], Name, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('st_payment_transgressions', 'Payment Transgressions', 1, 0, GETDATE(), 1)
    PRINT '  - Added StatusType: Payment Transgressions'
END

GO

-- =====================================================
-- 2. PAYMENT TRANSGRESSION STATUSES
-- =====================================================
PRINT 'Installing Payment Transgression Statuses...'
GO

DECLARE @PaymentTransgressionStatusTypeId INT
SELECT @PaymentTransgressionStatusTypeId = Id FROM StatusTypes WHERE [Key] = 'st_payment_transgressions'

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 'payment_transgression_status_submitted')
BEGIN
    INSERT INTO Status ([Key], Name, Description, StatusTypeId, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('payment_transgression_status_submitted', 'Submitted', 'Payment transgression has been submitted', @PaymentTransgressionStatusTypeId, 1, 0, GETDATE(), 1)
    PRINT '  - Added Status: Submitted'
END

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 'payment_transgression_status_letter_generated')
BEGIN
    INSERT INTO Status ([Key], Name, Description, StatusTypeId, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('payment_transgression_status_letter_generated', 'Letter Generated', 'Letter has been generated', @PaymentTransgressionStatusTypeId, 1, 0, GETDATE(), 1)
    PRINT '  - Added Status: Letter Generated'
END

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 'payment_transgression_status_letter_sent')
BEGIN
    INSERT INTO Status ([Key], Name, Description, StatusTypeId, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('payment_transgression_status_letter_sent', 'Letter Sent', 'Letter has been sent to tenant', @PaymentTransgressionStatusTypeId, 1, 0, GETDATE(), 1)
    PRINT '  - Added Status: Letter Sent'
END

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 'payment_transgression_status_closed')
BEGIN
    INSERT INTO Status ([Key], Name, Description, StatusTypeId, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('payment_transgression_status_closed', 'Closed', 'Payment transgression has been resolved and closed', @PaymentTransgressionStatusTypeId, 1, 0, GETDATE(), 1)
    PRINT '  - Added Status: Closed'
END

GO

-- =====================================================
-- 3. PAYMENT TRANSGRESSION CATEGORIES
-- =====================================================
PRINT 'Installing Payment Transgression Categories...'
GO

IF NOT EXISTS (SELECT 1 FROM PaymentTransgressionCategories WHERE [Key] = 'payment_transgression_cat_financial')
BEGIN
    INSERT INTO PaymentTransgressionCategories ([Key], Name, Description, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('payment_transgression_cat_financial', 'Financial', 'Payment-related transgressions including late payments, arrears, and lease violations', 1, 1, 0, GETDATE(), 1)
    PRINT '  - Added Category: Financial'
END

IF NOT EXISTS (SELECT 1 FROM PaymentTransgressionCategories WHERE [Key] = 'payment_transgression_cat_other')
BEGIN
    INSERT INTO PaymentTransgressionCategories ([Key], Name, Description, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('payment_transgression_cat_other', 'Other', 'Other payment-related transgressions not covered by standard categories', 99, 1, 0, GETDATE(), 1)
    PRINT '  - Added Category: Other'
END

GO

-- =====================================================
-- 4. PAYMENT TRANSGRESSION TYPES
-- =====================================================
PRINT 'Installing Payment Transgression Types...'
GO

DECLARE @FinancialCatId INT = (SELECT Id FROM PaymentTransgressionCategories WHERE [Key] = 'payment_transgression_cat_financial')
DECLARE @OtherCatId INT = (SELECT Id FROM PaymentTransgressionCategories WHERE [Key] = 'payment_transgression_cat_other')

-- Financial Types
IF NOT EXISTS (SELECT 1 FROM PaymentTransgressionTypes WHERE [Key] = 'payment_transgression_type_payment_misconduct')
BEGIN
    INSERT INTO PaymentTransgressionTypes ([Key], Name, Description, PaymentTransgressionCategoryId, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('payment_transgression_type_payment_misconduct', 'Payment Misconduct', 'Late or missed payments, payment default', @FinancialCatId, 1, 1, 0, GETDATE(), 1)
    PRINT '  - Added Type: Payment Misconduct'
END

IF NOT EXISTS (SELECT 1 FROM PaymentTransgressionTypes WHERE [Key] = 'payment_transgression_type_levy_arrears')
BEGIN
    INSERT INTO PaymentTransgressionTypes ([Key], Name, Description, PaymentTransgressionCategoryId, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('payment_transgression_type_levy_arrears', 'Levy Arrears', 'Outstanding levy payments accumulating', @FinancialCatId, 2, 1, 0, GETDATE(), 1)
    PRINT '  - Added Type: Levy Arrears'
END

IF NOT EXISTS (SELECT 1 FROM PaymentTransgressionTypes WHERE [Key] = 'payment_transgression_type_violation_lease')
BEGIN
    INSERT INTO PaymentTransgressionTypes ([Key], Name, Description, PaymentTransgressionCategoryId, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('payment_transgression_type_violation_lease', 'In Violation of Lease Agreement', 'Breach of lease financial obligations', @FinancialCatId, 3, 1, 0, GETDATE(), 1)
    PRINT '  - Added Type: In Violation of Lease Agreement'
END

-- Other Type
IF NOT EXISTS (SELECT 1 FROM PaymentTransgressionTypes WHERE [Key] = 'payment_transgression_type_other')
BEGIN
    INSERT INTO PaymentTransgressionTypes ([Key], Name, Description, PaymentTransgressionCategoryId, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('payment_transgression_type_other', 'Other', 'Other payment-related transgressions', @OtherCatId, 1, 1, 0, GETDATE(), 1)
    PRINT '  - Added Type: Other'
END

GO

-- =====================================================
-- 5. PAYMENT TRANSGRESSION SEVERITIES
-- =====================================================
PRINT 'Installing Payment Transgression Severities...'
GO

IF NOT EXISTS (SELECT 1 FROM PaymentTransgressionSeverities WHERE [Key] = 'payment_transgression_severity_level1_minor')
BEGIN
    INSERT INTO PaymentTransgressionSeverities ([Key], Name, Description, Level, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('payment_transgression_severity_level1_minor', 'Level 1: Minor', 'First-time late payment due to negligence', 1, 1, 1, 0, GETDATE(), 1)
    PRINT '  - Added Severity: Level 1 - Minor'
END

IF NOT EXISTS (SELECT 1 FROM PaymentTransgressionSeverities WHERE [Key] = 'payment_transgression_severity_level2_moderate')
BEGIN
    INSERT INTO PaymentTransgressionSeverities ([Key], Name, Description, Level, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('payment_transgression_severity_level2_moderate', 'Level 2: Moderate', 'Repeated late payments', 2, 2, 1, 0, GETDATE(), 1)
    PRINT '  - Added Severity: Level 2 - Moderate'
END

IF NOT EXISTS (SELECT 1 FROM PaymentTransgressionSeverities WHERE [Key] = 'payment_transgression_severity_level3_major')
BEGIN
    INSERT INTO PaymentTransgressionSeverities ([Key], Name, Description, Level, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('payment_transgression_severity_level3_major', 'Level 3: Major', 'Intentional misdirection leading to financial loss', 3, 3, 1, 0, GETDATE(), 1)
    PRINT '  - Added Severity: Level 3 - Major'
END

GO

-- =====================================================
-- VERIFICATION & SUMMARY
-- =====================================================
PRINT ''
PRINT '====================================================='
PRINT 'Master Data Installation Complete!'
PRINT '====================================================='
PRINT ''

DECLARE @StatusCount INT
DECLARE @CategoryCount INT
DECLARE @TypeCount INT
DECLARE @SeverityCount INT

SELECT @StatusCount = COUNT(*) FROM Status WHERE [Key] LIKE 'payment_transgression_status_%'
SELECT @CategoryCount = COUNT(*) FROM PaymentTransgressionCategories WHERE IsActive = 1
SELECT @TypeCount = COUNT(*) FROM PaymentTransgressionTypes WHERE IsActive = 1
SELECT @SeverityCount = COUNT(*) FROM PaymentTransgressionSeverities WHERE IsActive = 1

PRINT 'Summary:'
PRINT '  Statuses Created: ' + CAST(@StatusCount AS VARCHAR)
PRINT '  Categories Created: ' + CAST(@CategoryCount AS VARCHAR)
PRINT '  Types Created: ' + CAST(@TypeCount AS VARCHAR)
PRINT '  Severities Created: ' + CAST(@SeverityCount AS VARCHAR)
PRINT ''
PRINT 'Business Rules Implemented:'
PRINT '  BR22: 7-day action requirement for non-compliance'
PRINT '  BR25: Warning letters sent for breach of contract'
PRINT '  BR27: All actions logged for audit purposes'
PRINT ''
PRINT 'Letter Types Available:'
PRINT '  1. Payment Transgression Notice'
PRINT '  2. Written Warning Letter'
PRINT '  3. Final Written Warning Letter'
PRINT ''
PRINT 'Severity Levels:'
PRINT '  Level 1: Minor (First-time late payment due to negligence)'
PRINT '  Level 2: Moderate (Repeated late payments)'
PRINT '  Level 3: Major (Intentional misdirection leading to financial loss)'
PRINT ''
PRINT 'Next Steps:'
PRINT '  1. Ensure "Client Services Officer" role exists and is assigned'
PRINT '  2. Create upload directories: ~/Uploads/PaymentTransgressions/Documents/ and ~/Uploads/PaymentTransgressions/Letters/'
PRINT '  3. Test payment transgression submission functionality'
PRINT '  4. Verify email/SMS notification configuration'
PRINT ''
GO
