-- ==============================================================
-- MANUAL FIX: EHC2026041000009 — Ms L Mjika
-- Problem: CEO signed (2026-05-14) but ApprovalStatusddl was null
--          due to Razor DropDownList name binding bug. Status never
--          advanced to Active Lease.
-- Fix: Advance application to Active Lease, activate lease details,
--      mark correct agreement master complete, close open RRQs.
-- Run on: PROD — review each step before executing
-- Date: 2026-05-20
-- ==============================================================
BEGIN TRANSACTION

DECLARE @AppId   INT
DECLARE @LeaseId INT
DECLARE @ActiveLeaseStatusId INT
DECLARE @ArchivedRRQStatusId INT

SELECT @AppId = Id FROM PropertyLeaseApplications WHERE ApplicationReferenceNumber = 'EHC2026041000009'
SELECT @LeaseId = Id FROM LeaseDetails WHERE PropertyLeaseApplicationId = @AppId AND IsNew = 1 AND IsDeleted = 0
SELECT @ActiveLeaseStatusId = Id FROM Status WHERE [Key] = 'l_active_lease'
SELECT @ArchivedRRQStatusId = Id FROM Status WHERE [Key] = 'rs_archived'

PRINT 'AppId:              ' + CAST(ISNULL(@AppId,0) AS VARCHAR)
PRINT 'LeaseId:            ' + CAST(ISNULL(@LeaseId,0) AS VARCHAR)
PRINT 'ActiveLeaseStatus:  ' + CAST(ISNULL(@ActiveLeaseStatusId,0) AS VARCHAR)
PRINT 'ArchivedRRQStatus:  ' + CAST(ISNULL(@ArchivedRRQStatusId,0) AS VARCHAR)

-- Safety check
IF @AppId IS NULL OR @LeaseId IS NULL OR @ActiveLeaseStatusId IS NULL
BEGIN
    PRINT 'ERROR: One or more IDs not found. Rolling back.'
    ROLLBACK TRANSACTION
    RETURN
END

-- STEP 1: Advance application status to Active Lease
UPDATE PropertyLeaseApplications
SET StatusId = @ActiveLeaseStatusId, ModifiedDateTime = GETDATE()
WHERE Id = @AppId
PRINT 'Step 1: Application status set to Active Lease.'

-- STEP 2: Activate and complete the lease details record
UPDATE LeaseDetails
SET Completed = 1, IsActive = 1, ModifiedDateTime = GETDATE()
WHERE Id = @LeaseId
PRINT 'Step 2: LeaseDetails Completed and IsActive set.'

-- STEP 3: Mark the CEO-signed agreement master as the active one
--         (Agreement Master Id=5 has CEO signature from 2026-05-14)
UPDATE PropertyLeaseAgreementMasters
SET IsActive = 1, ModifiedDateTime = GETDATE()
WHERE PropertyLeaseApplicationId = @AppId
  AND PropertyManagerSigned = 1
  AND PropertyManagerSignatureDate IS NOT NULL
PRINT 'Step 3: CEO-signed agreement master marked active.'

-- STEP 4: Close any remaining open Submitted RRQs for this application
UPDATE RoundRobinQueues
SET StatusId = @ArchivedRRQStatusId,
    EndTaskDateTime = GETDATE(),
    ModifiedDateTime = GETDATE()
WHERE PropertyLeaseApplicationId = @AppId
  AND IsActive = 1
  AND IsDeleted = 0
  AND StatusId IN (SELECT Id FROM Status WHERE [Key] = 'rs_submitted')
PRINT 'Step 4: Open Submitted RRQs closed.'

-- VERIFY before committing
SELECT
    PA.ApplicationReferenceNumber,
    S.[Name]  AS [New App Status],
    LD.Completed,
    LD.IsActive AS [Lease Active],
    LAM.PropertyManagerSigned AS [CEO Signed],
    LAM.PropertyManagerSignatureDate AS [CEO Sign Date]
FROM PropertyLeaseApplications PA
JOIN Status S ON PA.StatusId = S.Id
JOIN LeaseDetails LD ON LD.PropertyLeaseApplicationId = PA.Id AND LD.IsNew = 1 AND LD.IsDeleted = 0
LEFT JOIN PropertyLeaseAgreementMasters LAM ON LAM.PropertyLeaseApplicationId = PA.Id AND LAM.IsActive = 1 AND LAM.IsDeleted = 0 AND LAM.PropertyManagerSigned = 1
WHERE PA.Id = @AppId

-- If results above look correct, COMMIT. Otherwise ROLLBACK.
COMMIT TRANSACTION
-- ROLLBACK TRANSACTION
PRINT 'Done. Review results above before confirming.'
