-- =====================================================
-- Clear Complaint Test Data
-- Deletes all complaint + evidence records in FK-safe order.
-- Does NOT touch master/lookup data (ComplaintCategories, ComplaintTypes).
-- Run against eServicesDb in SSMS.
-- =====================================================

USE [eServicesDb]
GO

PRINT 'Clearing complaint test data...'

-- 1. Audit logs (FK -> TenantComplaints)
DELETE FROM ComplaintAuditLogs
PRINT '  - Cleared: ComplaintAuditLogs (' + CAST(@@ROWCOUNT AS VARCHAR) + ' rows)'

-- 2. Investigation documents (FK -> ComplaintInvestigations)
DELETE FROM ComplaintInvestigationDocuments
PRINT '  - Cleared: ComplaintInvestigationDocuments (' + CAST(@@ROWCOUNT AS VARCHAR) + ' rows)'

-- 3. External referrals (FK -> ComplaintInvestigations)
DELETE FROM ComplaintExternalReferrals
PRINT '  - Cleared: ComplaintExternalReferrals (' + CAST(@@ROWCOUNT AS VARCHAR) + ' rows)'

-- 4. Investigations (FK -> TenantComplaints)
DELETE FROM ComplaintInvestigations
PRINT '  - Cleared: ComplaintInvestigations (' + CAST(@@ROWCOUNT AS VARCHAR) + ' rows)'

-- 5. Evidence files (FK -> TenantComplaints)
DELETE FROM ComplaintEvidences
PRINT '  - Cleared: ComplaintEvidences (' + CAST(@@ROWCOUNT AS VARCHAR) + ' rows)'

-- 6. RoundRobin entries created for complaints (TenantComplaintId IS NOT NULL)
DELETE FROM RoundRobinQueues WHERE TenantComplaintId IS NOT NULL
PRINT '  - Cleared: RoundRobinQueues complaint entries (' + CAST(@@ROWCOUNT AS VARCHAR) + ' rows)'

-- 7. The complaints themselves
DELETE FROM TenantComplaints
PRINT '  - Cleared: TenantComplaints (' + CAST(@@ROWCOUNT AS VARCHAR) + ' rows)'

PRINT ''
PRINT 'Done. All complaint test data cleared.'
GO
