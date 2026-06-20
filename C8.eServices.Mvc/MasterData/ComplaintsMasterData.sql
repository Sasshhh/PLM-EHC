-- =====================================================
-- Complaints/Transgressions Module - Master Data Script
-- Purpose: Insert all required master data for the Complaints module
-- This script is idempotent and safe to run multiple times
-- =====================================================

-- Note: This script should be run against your active database
-- The connection should already be established to the correct database

PRINT 'Starting Complaints Master Data Installation...'
GO

-- =====================================================
-- 0. STATUS TYPE FOR COMPLAINTS
-- =====================================================
PRINT 'Installing Status Type for Complaints Module...'
GO

IF NOT EXISTS (SELECT 1 FROM StatusTypes WHERE [Key] = 'st_complaints')
BEGIN
    INSERT INTO StatusTypes ([Key], Name, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('st_complaints', 'Complaints', 1, 0, GETDATE(), 1)
    PRINT '  - Added StatusType: Complaints'
END

GO

-- =====================================================
-- 1. COMPLAINT STATUSES
-- =====================================================
PRINT 'Installing Complaint Statuses...'
GO

-- Get the StatusTypeId for Complaints
DECLARE @ComplaintsStatusTypeId INT
SELECT @ComplaintsStatusTypeId = Id FROM StatusTypes WHERE [Key] = 'st_complaints'

-- Use StatusTypeId for Complaints module statuses
IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 'complaint_status_submitted')
BEGIN
    INSERT INTO Status ([Key], Name, Description, StatusTypeId, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('complaint_status_submitted', 'Submitted', 'Complaint has been submitted and awaiting assignment', @ComplaintsStatusTypeId, 1, 0, GETDATE(), 1)
    PRINT '  - Added Status: Submitted'
END

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 'complaint_status_awaiting_appointment')
BEGIN
    INSERT INTO Status ([Key], Name, Description, StatusTypeId, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('complaint_status_awaiting_appointment', 'Awaiting Appointment', 'Investigation appointment needs to be scheduled', @ComplaintsStatusTypeId, 1, 0, GETDATE(), 1)
    PRINT '  - Added Status: Awaiting Appointment'
END

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 'complaint_status_awaiting_investigation')
BEGIN
    INSERT INTO Status ([Key], Name, Description, StatusTypeId, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('complaint_status_awaiting_investigation', 'Awaiting Investigation', 'Investigation appointment scheduled, awaiting execution', @ComplaintsStatusTypeId, 1, 0, GETDATE(), 1)
    PRINT '  - Added Status: Awaiting Investigation'
END

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 'complaint_status_resolved')
BEGIN
    INSERT INTO Status ([Key], Name, Description, StatusTypeId, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('complaint_status_resolved', 'Resolved', 'Complaint has been investigated and resolved', @ComplaintsStatusTypeId, 1, 0, GETDATE(), 1)
    PRINT '  - Added Status: Resolved'
END

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 'complaint_status_referred')
BEGIN
    INSERT INTO Status ([Key], Name, Description, StatusTypeId, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('complaint_status_referred', 'Referred', 'Complaint referred to external agency', @ComplaintsStatusTypeId, 1, 0, GETDATE(), 1)
    PRINT '  - Added Status: Referred'
END

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 'complaint_status_unresolved')
BEGIN
    INSERT INTO Status ([Key], Name, Description, StatusTypeId, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('complaint_status_unresolved', 'Unresolved', 'Complaint could not be resolved - lease termination may be triggered', @ComplaintsStatusTypeId, 1, 0, GETDATE(), 1)
    PRINT '  - Added Status: Unresolved'
END

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 'lease_termination_pending')
BEGIN
    INSERT INTO Status ([Key], Name, Description, StatusTypeId, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('lease_termination_pending', 'Lease Termination Pending', 'Lease termination process initiated due to complaint', @ComplaintsStatusTypeId, 1, 0, GETDATE(), 1)
    PRINT '  - Added Status: Lease Termination Pending'
END

GO

-- =====================================================
-- 2. COMPLAINT CATEGORIES
-- =====================================================
PRINT 'Installing Complaint Categories...'
GO

IF NOT EXISTS (SELECT 1 FROM ComplaintCategories WHERE [Key] = 'complaint_cat_administration')
BEGIN
    INSERT INTO ComplaintCategories ([Key], Name, Description, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('complaint_cat_administration', 'Administration', 'Administrative violations including sub-letting and rule violations', 1, 1, 0, GETDATE(), 1)
    PRINT '  - Added Category: Administration'
END

IF NOT EXISTS (SELECT 1 FROM ComplaintCategories WHERE [Key] = 'complaint_cat_nuisance_behavioural')
BEGIN
    INSERT INTO ComplaintCategories ([Key], Name, Description, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('complaint_cat_nuisance_behavioural', 'Nuisance and Behavioural', 'Noise, odors, aggressive behaviour, and children-related complaints', 2, 1, 0, GETDATE(), 1)
    PRINT '  - Added Category: Nuisance and Behavioural'
END

IF NOT EXISTS (SELECT 1 FROM ComplaintCategories WHERE [Key] = 'complaint_cat_parking_vehicle')
BEGIN
    INSERT INTO ComplaintCategories ([Key], Name, Description, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('complaint_cat_parking_vehicle', 'Parking and Vehicle', 'Parking violations and vehicle-related complaints', 3, 1, 0, GETDATE(), 1)
    PRINT '  - Added Category: Parking and Vehicle'
END

IF NOT EXISTS (SELECT 1 FROM ComplaintCategories WHERE [Key] = 'complaint_cat_pet_animals')
BEGIN
    INSERT INTO ComplaintCategories ([Key], Name, Description, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('complaint_cat_pet_animals', 'Pet and Animals', 'Complaints related to pets and animals', 4, 1, 0, GETDATE(), 1)
    PRINT '  - Added Category: Pet and Animals'
END

IF NOT EXISTS (SELECT 1 FROM ComplaintCategories WHERE [Key] = 'complaint_cat_property_usage')
BEGIN
    INSERT INTO ComplaintCategories ([Key], Name, Description, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('complaint_cat_property_usage', 'Property Usage', 'Property maintenance, alterations, and usage complaints', 5, 1, 0, GETDATE(), 1)
    PRINT '  - Added Category: Property Usage'
END

IF NOT EXISTS (SELECT 1 FROM ComplaintCategories WHERE [Key] = 'complaint_cat_safety_security')
BEGIN
    INSERT INTO ComplaintCategories ([Key], Name, Description, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('complaint_cat_safety_security', 'Safety & Security', 'Safety and security-related complaints', 6, 1, 0, GETDATE(), 1)
    PRINT '  - Added Category: Safety & Security'
END

IF NOT EXISTS (SELECT 1 FROM ComplaintCategories WHERE [Key] = 'complaint_cat_other')
BEGIN
    INSERT INTO ComplaintCategories ([Key], Name, Description, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('complaint_cat_other', 'Other', 'Other complaints not covered by standard categories', 99, 1, 0, GETDATE(), 1)
    PRINT '  - Added Category: Other'
END

GO

-- =====================================================
-- 3. COMPLAINT TYPES (grouped by category)
-- =====================================================
PRINT 'Installing Complaint Types...'
GO

DECLARE @AdminCatId INT = (SELECT Id FROM ComplaintCategories WHERE [Key] = 'complaint_cat_administration')
DECLARE @NuisanceCatId INT = (SELECT Id FROM ComplaintCategories WHERE [Key] = 'complaint_cat_nuisance_behavioural')
DECLARE @ParkingCatId INT = (SELECT Id FROM ComplaintCategories WHERE [Key] = 'complaint_cat_parking_vehicle')
DECLARE @PetCatId INT = (SELECT Id FROM ComplaintCategories WHERE [Key] = 'complaint_cat_pet_animals')
DECLARE @PropertyCatId INT = (SELECT Id FROM ComplaintCategories WHERE [Key] = 'complaint_cat_property_usage')
DECLARE @SafetyCatId INT = (SELECT Id FROM ComplaintCategories WHERE [Key] = 'complaint_cat_safety_security')
DECLARE @OtherCatId INT = (SELECT Id FROM ComplaintCategories WHERE [Key] = 'complaint_cat_other')

-- Administration Types
IF NOT EXISTS (SELECT 1 FROM ComplaintTypes WHERE [Key] = 'complaint_type_subletting')
BEGIN
    INSERT INTO ComplaintTypes ([Key], Name, Description, ComplaintCategoryId, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('complaint_type_subletting', 'Sub-letting', 'Tenant is sub-letting the property without authorization', @AdminCatId, 1, 1, 0, GETDATE(), 1)
    PRINT '  - Added Type: Sub-letting'
END

IF NOT EXISTS (SELECT 1 FROM ComplaintTypes WHERE [Key] = 'complaint_type_violation_rules')
BEGIN
    INSERT INTO ComplaintTypes ([Key], Name, Description, ComplaintCategoryId, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('complaint_type_violation_rules', 'Violation of Complex Rules', 'Tenant is violating complex or lease rules', @AdminCatId, 2, 1, 0, GETDATE(), 1)
    PRINT '  - Added Type: Violation of Complex Rules'
END

-- Nuisance and Behavioural Types
IF NOT EXISTS (SELECT 1 FROM ComplaintTypes WHERE [Key] = 'complaint_type_noise')
BEGIN
    INSERT INTO ComplaintTypes ([Key], Name, Description, ComplaintCategoryId, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('complaint_type_noise', 'Noise Disturbance', 'Excessive noise causing disturbance', @NuisanceCatId, 1, 1, 0, GETDATE(), 1)
    PRINT '  - Added Type: Noise Disturbance'
END

IF NOT EXISTS (SELECT 1 FROM ComplaintTypes WHERE [Key] = 'complaint_type_odors')
BEGIN
    INSERT INTO ComplaintTypes ([Key], Name, Description, ComplaintCategoryId, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('complaint_type_odors', 'Odors and Fumes', 'Unpleasant odors or fumes', @NuisanceCatId, 2, 1, 0, GETDATE(), 1)
    PRINT '  - Added Type: Odors and Fumes'
END

IF NOT EXISTS (SELECT 1 FROM ComplaintTypes WHERE [Key] = 'complaint_type_aggressive')
BEGIN
    INSERT INTO ComplaintTypes ([Key], Name, Description, ComplaintCategoryId, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('complaint_type_aggressive', 'Aggressive Behaviour', 'Aggressive or threatening behaviour', @NuisanceCatId, 3, 1, 0, GETDATE(), 1)
    PRINT '  - Added Type: Aggressive Behaviour'
END

IF NOT EXISTS (SELECT 1 FROM ComplaintTypes WHERE [Key] = 'complaint_type_children')
BEGIN
    INSERT INTO ComplaintTypes ([Key], Name, Description, ComplaintCategoryId, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('complaint_type_children', 'Children-related Issues', 'Unsupervised children or child-related disturbances', @NuisanceCatId, 4, 1, 0, GETDATE(), 1)
    PRINT '  - Added Type: Children-related Issues'
END

-- Parking and Vehicle Types
IF NOT EXISTS (SELECT 1 FROM ComplaintTypes WHERE [Key] = 'complaint_type_obstructive_parking')
BEGIN
    INSERT INTO ComplaintTypes ([Key], Name, Description, ComplaintCategoryId, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('complaint_type_obstructive_parking', 'Obstructive Parking', 'Vehicle blocking access or driveway', @ParkingCatId, 1, 1, 0, GETDATE(), 1)
    PRINT '  - Added Type: Obstructive Parking'
END

IF NOT EXISTS (SELECT 1 FROM ComplaintTypes WHERE [Key] = 'complaint_type_unauthorised_parking')
BEGIN
    INSERT INTO ComplaintTypes ([Key], Name, Description, ComplaintCategoryId, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('complaint_type_unauthorised_parking', 'Unauthorised Parking', 'Parking in reserved or unauthorized areas', @ParkingCatId, 2, 1, 0, GETDATE(), 1)
    PRINT '  - Added Type: Unauthorised Parking'
END

IF NOT EXISTS (SELECT 1 FROM ComplaintTypes WHERE [Key] = 'complaint_type_car_wash')
BEGIN
    INSERT INTO ComplaintTypes ([Key], Name, Description, ComplaintCategoryId, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('complaint_type_car_wash', 'Unauthorized Car Wash', 'Washing vehicles in unauthorized areas', @ParkingCatId, 3, 1, 0, GETDATE(), 1)
    PRINT '  - Added Type: Unauthorized Car Wash'
END

-- Pet and Animals Types
IF NOT EXISTS (SELECT 1 FROM ComplaintTypes WHERE [Key] = 'complaint_type_nuisance_pets')
BEGIN
    INSERT INTO ComplaintTypes ([Key], Name, Description, ComplaintCategoryId, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('complaint_type_nuisance_pets', 'Nuisance Pets', 'Pets causing disturbance or safety concerns', @PetCatId, 1, 1, 0, GETDATE(), 1)
    PRINT '  - Added Type: Nuisance Pets'
END

IF NOT EXISTS (SELECT 1 FROM ComplaintTypes WHERE [Key] = 'complaint_type_unapproved_pets')
BEGIN
    INSERT INTO ComplaintTypes ([Key], Name, Description, ComplaintCategoryId, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('complaint_type_unapproved_pets', 'Unapproved Pets', 'Keeping pets without approval', @PetCatId, 2, 1, 0, GETDATE(), 1)
    PRINT '  - Added Type: Unapproved Pets'
END

-- Property Usage Types
IF NOT EXISTS (SELECT 1 FROM ComplaintTypes WHERE [Key] = 'complaint_type_untidy')
BEGIN
    INSERT INTO ComplaintTypes ([Key], Name, Description, ComplaintCategoryId, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('complaint_type_untidy', 'Untidy Areas', 'Property or common areas kept in untidy condition', @PropertyCatId, 1, 1, 0, GETDATE(), 1)
    PRINT '  - Added Type: Untidy Areas'
END

IF NOT EXISTS (SELECT 1 FROM ComplaintTypes WHERE [Key] = 'complaint_type_neglected')
BEGIN
    INSERT INTO ComplaintTypes ([Key], Name, Description, ComplaintCategoryId, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('complaint_type_neglected', 'Neglected Property', 'Property maintenance being neglected', @PropertyCatId, 2, 1, 0, GETDATE(), 1)
    PRINT '  - Added Type: Neglected Property'
END

IF NOT EXISTS (SELECT 1 FROM ComplaintTypes WHERE [Key] = 'complaint_type_unapproved_alterations')
BEGIN
    INSERT INTO ComplaintTypes ([Key], Name, Description, ComplaintCategoryId, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('complaint_type_unapproved_alterations', 'Unapproved Alterations', 'Unauthorized property alterations or modifications', @PropertyCatId, 3, 1, 0, GETDATE(), 1)
    PRINT '  - Added Type: Unapproved Alterations'
END

IF NOT EXISTS (SELECT 1 FROM ComplaintTypes WHERE [Key] = 'complaint_type_misuse')
BEGIN
    INSERT INTO ComplaintTypes ([Key], Name, Description, ComplaintCategoryId, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('complaint_type_misuse', 'Property Misuse', 'Using property for unauthorized purposes', @PropertyCatId, 4, 1, 0, GETDATE(), 1)
    PRINT '  - Added Type: Property Misuse'
END

-- Safety & Security Types
IF NOT EXISTS (SELECT 1 FROM ComplaintTypes WHERE [Key] = 'complaint_type_security_neglect')
BEGIN
    INSERT INTO ComplaintTypes ([Key], Name, Description, ComplaintCategoryId, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('complaint_type_security_neglect', 'Neglecting Security Rules', 'Failing to follow security protocols', @SafetyCatId, 1, 1, 0, GETDATE(), 1)
    PRINT '  - Added Type: Neglecting Security Rules'
END

IF NOT EXISTS (SELECT 1 FROM ComplaintTypes WHERE [Key] = 'complaint_type_other_safety')
BEGIN
    INSERT INTO ComplaintTypes ([Key], Name, Description, ComplaintCategoryId, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('complaint_type_other_safety', 'Other Safety/Security Issue', 'Other safety or security concerns', @SafetyCatId, 2, 1, 0, GETDATE(), 1)
    PRINT '  - Added Type: Other Safety/Security Issue'
END

-- Other Types
IF NOT EXISTS (SELECT 1 FROM ComplaintTypes WHERE [Key] = 'complaint_type_other')
BEGIN
    INSERT INTO ComplaintTypes ([Key], Name, Description, ComplaintCategoryId, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('complaint_type_other', 'Other Complaint', 'Complaint not covered by other categories', @OtherCatId, 1, 1, 0, GETDATE(), 1)
    PRINT '  - Added Type: Other Complaint'
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

-- Use variables for summary counts
DECLARE @StatusCount INT
DECLARE @CategoryCount INT
DECLARE @TypeCount INT

SELECT @StatusCount = COUNT(*) FROM Status WHERE [Key] LIKE 'complaint_status_%' OR [Key] = 'lease_termination_pending'
SELECT @CategoryCount = COUNT(*) FROM ComplaintCategories WHERE IsActive = 1
SELECT @TypeCount = COUNT(*) FROM ComplaintTypes WHERE IsActive = 1

PRINT 'Summary:'
PRINT '  Statuses Created: ' + CAST(@StatusCount AS VARCHAR)
PRINT '  Categories Created: ' + CAST(@CategoryCount AS VARCHAR)
PRINT '  Types Created: ' + CAST(@TypeCount AS VARCHAR)
PRINT ''
PRINT 'Next Steps:'
PRINT '  1. Ensure "Client Services Officer" role exists in AspNetRoles table'
PRINT '  2. Ensure at least one user is assigned to the Client Services Officer role'
PRINT '  3. Test complaint submission functionality'
PRINT '  4. Verify email/SMS notification configuration'
PRINT ''
PRINT 'Configuration Notes:'
PRINT '  - SLA for non-compliance: 7 working days (configured in ComplaintSLAEngine.cs)'
PRINT '  - Warning letter threshold: 3 warnings before escalation'
PRINT '  - Sub-letting complaints automatically trigger lease termination on resolution'
PRINT '  - Unresolved complaints trigger lease termination process'
PRINT ''
GO
