-- ============================================================
-- PLM TERMINATION FLOW — E2E TEST DIAGNOSTIC & SETUP
-- ============================================================
-- Run this AFTER executing PLM_UC025_UC026_MIGRATION.sql
-- This will:
--   1. Verify all required Status/AT/RT rows exist
--   2. Find an active lease you can test with
--   3. Optionally set that lease to a testable status
-- ============================================================

PRINT '=== 1. STATUS KEY VERIFICATION ==='
SELECT [Key], [Name], Id FROM Status WHERE [Key] IN (
    -- UC023/024 statuses
    's_awaiting_cso_termination_review',
    's_awaiting_termination_appraisal',
    's_termination_not_supported',
    's_awaiting_termination_approval',
    's_legal_referral_pending',
    's_awaiting_eviction_ceo_auth',
    -- UC025 statuses
    's_awaiting_eviction_service',
    's_eviction_notice_served',
    's_awaiting_proof_of_service',
    's_proof_of_service_captured',
    -- UC026 statuses
    's_dispute_open_awaiting_review',
    's_dispute_referred',
    's_dispute_resolved',
    's_dispute_closed',
    -- Downstream statuses
    's_awaiting_exit_inspec',
    's_awaiting_vacating_confirm',
    's_applicant_vacated'
) ORDER BY [Key]

PRINT ''
PRINT '=== 2. ACTIVITY TRACKER MESSAGE VERIFICATION ==='
SELECT [Key], [Name] FROM ActivityTrackerMessages WHERE [Key] IN (
    'at_eviction_notice_served', 'at_proof_of_service_captured',
    'at_dispute_registered', 'at_dispute_reviewed', 'at_dispute_referred_legal',
    'at_dispute_resolved', 'at_dispute_not_resolved',
    'at_dispute_closed_ceo', 'at_dispute_rejected_ceo',
    'at_termination_rm_approved', 'at_termination_rm_rejected',
    'at_termination_rm_referred_legal', 'at_eviction_ceo_approved',
    'at_eviction_ceo_rejected'
) ORDER BY [Key]

PRINT ''
PRINT '=== 3. RESPONSIBILITY TYPE VERIFICATION ==='
SELECT [Key], [Name] FROM ResponsibilityTypes WHERE [Key] IN (
    'r_eviction_service', 'r_proof_of_service',
    'r_dispute_review', 'r_dispute_resolution', 'r_dispute_closure',
    'r_termination_validation'
) ORDER BY [Key]

PRINT ''
PRINT '=== 4. ACTIVE LEASES WITH TERMINATION RECORDS ==='
-- Find leases that have a LeaseTermination record (candidates to test termination flow)
SELECT TOP 10
    ld.Id AS LeaseDetailsId,
    ld.LeaseReferenceNo,
    ld.FirstNames + ' ' + ld.LastName AS TenantName,
    s.[Name] AS CurrentStatus,
    s.[Key] AS StatusKey,
    lt.Id AS TerminationId,
    lt.ReasonForTermination,
    lt.TerminationDate,
    pla.Id AS ApplicationId,
    pla.ApplicationReferenceNumber
FROM LeaseDetails ld
INNER JOIN PropertyLeaseApplications pla ON pla.Id = ld.PropertyLeaseApplicationId
INNER JOIN Status s ON s.Id = ld.StatusId
LEFT JOIN LeaseTerminations lt ON lt.PropertyLeaseApplicationId = pla.Id AND lt.IsDeleted = 0
WHERE ld.IsDeleted = 0
    AND ld.IsNew = 1
    AND lt.Id IS NOT NULL
ORDER BY lt.Id DESC

PRINT ''
PRINT '=== 5. ALL ACTIVE LEASES (TOP 10, ANY STATUS) ==='
SELECT TOP 10
    ld.Id AS LeaseDetailsId,
    ld.LeaseReferenceNo,
    ld.FirstNames + ' ' + ld.LastName AS TenantName,
    s.[Name] AS CurrentStatus,
    s.[Key] AS StatusKey,
    pla.Id AS ApplicationId,
    pla.ApplicationReferenceNumber
FROM LeaseDetails ld
INNER JOIN PropertyLeaseApplications pla ON pla.Id = ld.PropertyLeaseApplicationId
INNER JOIN Status s ON s.Id = ld.StatusId
WHERE ld.IsDeleted = 0 AND ld.IsNew = 1
ORDER BY ld.Id DESC

-- ============================================================
-- 6. OPTIONAL: Set a specific lease to AwaitingTerminationAppraisal
--    so you can test the RM → UC025 → Exit Inspection flow
--    UNCOMMENT the block below and replace @TestLeaseId with the
--    LeaseDetailsId from the results above.
-- ============================================================
/*
DECLARE @TestLeaseId INT = ???  -- <-- replace with LeaseDetailsId from above
DECLARE @TargetStatusId INT = (SELECT Id FROM Status WHERE [Key] = 's_awaiting_termination_appraisal')

UPDATE LeaseDetails SET StatusId = @TargetStatusId WHERE Id = @TestLeaseId
PRINT 'Lease ' + CAST(@TestLeaseId AS VARCHAR) + ' set to AwaitingTerminationAppraisal'
*/

-- ============================================================
-- 7. OPTIONAL: Set a lease directly to AwaitingEvictionService
--    to test UC025 in isolation.
-- ============================================================
/*
DECLARE @TestLeaseId2 INT = ???  -- <-- replace with LeaseDetailsId from above
DECLARE @EvicServiceStatusId INT = (SELECT Id FROM Status WHERE [Key] = 's_awaiting_eviction_service')

UPDATE LeaseDetails SET StatusId = @EvicServiceStatusId WHERE Id = @TestLeaseId2
PRINT 'Lease ' + CAST(@TestLeaseId2 AS VARCHAR) + ' set to AwaitingEvictionService'
*/
