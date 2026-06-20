-- =====================================================
-- Service Requests Module (UC17D/UC17E) - Master Data Script
-- Purpose: Insert all required master data for Service Requests
-- This script is idempotent and safe to run multiple times
-- =====================================================

PRINT 'Starting Service Requests Master Data Installation...'
GO

-- =====================================================
-- 1. STATUS TYPE FOR SERVICE REQUESTS
-- =====================================================
PRINT 'Installing Status Type for Service Requests...'
GO

IF NOT EXISTS (SELECT 1 FROM StatusTypes WHERE [Key] = 'st_service_requests')
BEGIN
    INSERT INTO StatusTypes ([Key], Name, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('st_service_requests', 'Service Requests', 1, 0, GETDATE(), 1)
    PRINT '  - Added StatusType: Service Requests'
END

GO

-- =====================================================
-- 2. SERVICE REQUEST STATUSES (BR36)
-- Open, Deleted, In Progress, Resolved, Closed
-- =====================================================
PRINT 'Installing Service Request Statuses...'
GO

DECLARE @ServiceRequestStatusTypeId INT
SELECT @ServiceRequestStatusTypeId = Id FROM StatusTypes WHERE [Key] = 'st_service_requests'

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 'sr_status_open')
BEGIN
    INSERT INTO Status ([Key], Name, Description, StatusTypeId, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('sr_status_open', 'Open', 'Service request has been submitted and is awaiting action', @ServiceRequestStatusTypeId, 1, 0, GETDATE(), 1)
    PRINT '  - Added Status: Open'
END

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 'sr_status_in_progress')
BEGIN
    INSERT INTO Status ([Key], Name, Description, StatusTypeId, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('sr_status_in_progress', 'In Progress', 'Service request is currently being worked on', @ServiceRequestStatusTypeId, 1, 0, GETDATE(), 1)
    PRINT '  - Added Status: In Progress'
END

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 'sr_status_resolved')
BEGIN
    INSERT INTO Status ([Key], Name, Description, StatusTypeId, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('sr_status_resolved', 'Resolved', 'Service request has been resolved', @ServiceRequestStatusTypeId, 1, 0, GETDATE(), 1)
    PRINT '  - Added Status: Resolved'
END

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 'sr_status_closed')
BEGIN
    INSERT INTO Status ([Key], Name, Description, StatusTypeId, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('sr_status_closed', 'Closed', 'Service request has been closed', @ServiceRequestStatusTypeId, 1, 0, GETDATE(), 1)
    PRINT '  - Added Status: Closed'
END

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 'sr_status_deleted')
BEGIN
    INSERT INTO Status ([Key], Name, Description, StatusTypeId, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('sr_status_deleted', 'Deleted', 'Service request has been deleted by the creator', @ServiceRequestStatusTypeId, 1, 0, GETDATE(), 1)
    PRINT '  - Added Status: Deleted'
END

GO

-- =====================================================
-- 3. SERVICE REQUEST CATEGORIES (UC17D Step 7)
-- Maintenance Request, Specialised Care and Support Request, Compensation and Disputes Request
-- =====================================================
PRINT 'Installing Service Request Categories...'
GO

IF NOT EXISTS (SELECT 1 FROM ServiceRequestCategories WHERE [Key] = 'sr_cat_maintenance')
BEGIN
    INSERT INTO ServiceRequestCategories ([Key], Name, Description, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('sr_cat_maintenance', 'Maintenance Request', 'Maintenance-related service requests (BR23: 30 working day resolution)', 1, 1, 0, GETDATE(), 1)
    PRINT '  - Added Category: Maintenance Request'
END

IF NOT EXISTS (SELECT 1 FROM ServiceRequestCategories WHERE [Key] = 'sr_cat_specialised_care')
BEGIN
    INSERT INTO ServiceRequestCategories ([Key], Name, Description, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('sr_cat_specialised_care', 'Specialised Care and Support Request', 'Specialised care and support service requests (BR24: 7 working day resolution)', 2, 1, 0, GETDATE(), 1)
    PRINT '  - Added Category: Specialised Care and Support Request'
END

IF NOT EXISTS (SELECT 1 FROM ServiceRequestCategories WHERE [Key] = 'sr_cat_compensation_disputes')
BEGIN
    INSERT INTO ServiceRequestCategories ([Key], Name, Description, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('sr_cat_compensation_disputes', 'Compensation and Disputes Request', 'Compensation and disputes service requests (BR24: 7 working day resolution)', 3, 1, 0, GETDATE(), 1)
    PRINT '  - Added Category: Compensation and Disputes Request'
END

GO

-- =====================================================
-- 4. SERVICE REQUEST PRIORITIES (UC17D Step 9, BR34)
-- 1=Emergency/Critical, 2=High, 3=Medium, 4=Low
-- With SLA response and resolution times
-- =====================================================
PRINT 'Installing Service Request Priorities...'
GO

-- Priority 1: Emergency/Critical - Respond 30min, Resolve 24hr
IF NOT EXISTS (SELECT 1 FROM ServiceRequestPriorities WHERE [Key] = 'sr_priority_emergency_critical')
BEGIN
    INSERT INTO ServiceRequestPriorities ([Key], Name, Description, [Level], ResponseTimeMinutes, ResolutionTimeHours, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('sr_priority_emergency_critical', '1 - Emergency/Critical', 'Emergency/Critical priority: Respond within 30 minutes, resolve within 24 hours', 1, 30, 24, 1, 1, 0, GETDATE(), 1)
    PRINT '  - Added Priority: 1 - Emergency/Critical (30min response / 24hr resolution)'
END

-- Priority 2: High - Respond 1hr, Resolve 72hr
IF NOT EXISTS (SELECT 1 FROM ServiceRequestPriorities WHERE [Key] = 'sr_priority_high')
BEGIN
    INSERT INTO ServiceRequestPriorities ([Key], Name, Description, [Level], ResponseTimeMinutes, ResolutionTimeHours, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('sr_priority_high', '2 - High', 'High priority: Respond within 1 hour, resolve within 72 hours', 2, 60, 72, 2, 1, 0, GETDATE(), 1)
    PRINT '  - Added Priority: 2 - High (1hr response / 72hr resolution)'
END

-- Priority 3: Medium - Respond 24hr, Resolve 5 business days (120hr)
IF NOT EXISTS (SELECT 1 FROM ServiceRequestPriorities WHERE [Key] = 'sr_priority_medium')
BEGIN
    INSERT INTO ServiceRequestPriorities ([Key], Name, Description, [Level], ResponseTimeMinutes, ResolutionTimeHours, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('sr_priority_medium', '3 - Medium', 'Medium priority: Respond within 24 hours, resolve within 5 business days', 3, 1440, 120, 3, 1, 0, GETDATE(), 1)
    PRINT '  - Added Priority: 3 - Medium (24hr response / 5 business day resolution)'
END

-- Priority 4: Low - Respond 48hr, Resolve 30 business days (720hr)
IF NOT EXISTS (SELECT 1 FROM ServiceRequestPriorities WHERE [Key] = 'sr_priority_low')
BEGIN
    INSERT INTO ServiceRequestPriorities ([Key], Name, Description, [Level], ResponseTimeMinutes, ResolutionTimeHours, DisplayOrder, IsActive, IsDeleted, CreatedDateTime, DepartmentId)
    VALUES ('sr_priority_low', '4 - Low', 'Low priority: Respond within 48 hours, resolve within 30 business days', 4, 2880, 720, 4, 1, 0, GETDATE(), 1)
    PRINT '  - Added Priority: 4 - Low (48hr response / 30 business day resolution)'
END

GO

-- =====================================================
-- VERIFICATION QUERIES
-- =====================================================
PRINT ''
PRINT '====================================================='
PRINT 'Service Requests Master Data Installation Complete!'
PRINT '====================================================='
PRINT ''

PRINT 'StatusType:'
SELECT Id, [Key], Name FROM StatusTypes WHERE [Key] = 'st_service_requests'

PRINT 'Statuses:'
SELECT Id, [Key], Name FROM Status WHERE [Key] LIKE 'sr_status_%'

PRINT 'Categories:'
SELECT Id, [Key], Name, DisplayOrder FROM ServiceRequestCategories ORDER BY DisplayOrder

PRINT 'Priorities:'
SELECT Id, [Key], Name, [Level], ResponseTimeMinutes, ResolutionTimeHours FROM ServiceRequestPriorities ORDER BY [Level]

PRINT ''
PRINT 'Service Requests module ready for use!'
PRINT ''
GO
