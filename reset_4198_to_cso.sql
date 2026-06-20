-- ====================================================
-- Reset EHC2025103100002 (AppId=4198) back to
-- AwaitingLeaseAgreement so CSO can generate the lease
-- ====================================================

DECLARE @AppId INT = 4198;

-- 1. Delete stale queue items that are beyond the GenerateLeaseAgreement step:
--    - r_lease_agreement_validation (Id=13622, ClerkId=1266 RM, active StatusId=95)
--    - r_agreement_approval          (Id=13624, ClerkId=190, StatusId=99)
-- Keep all older completed items (StatusId=99 before 13622) untouched.
DELETE FROM dbo.RoundRobinQueues
WHERE PropertyLeaseApplicationId = @AppId
  AND Id IN (13622, 13624);

-- 2. Reset the GenerateLeaseAgreement queue item (Id=13615) back to Submitted (95)
--    so CSO can see the "Generate Lease Agreement" task again.
UPDATE dbo.RoundRobinQueues
SET StatusId = 95
WHERE PropertyLeaseApplicationId = @AppId
  AND Id = 13615;

-- 3. Delete PropertyLeaseAgreementMaster records (created when lease was generated)
--    so CSO can re-generate cleanly.
DELETE FROM dbo.propertyLeaseAgreementMasters
WHERE PropertyLeaseApplicationId = @AppId;

-- 4. Reset application status to AwaitingLeaseAgreement (Id=139)
UPDATE dbo.PropertyLeaseApplications
SET StatusId = 139
WHERE Id = @AppId;

-- 5. Verify
SELECT 
    p.Id, 
    p.ApplicationReferenceNumber, 
    s.[Key] AS CurrentStatus
FROM dbo.PropertyLeaseApplications p
INNER JOIN dbo.Status s ON s.Id = p.StatusId
WHERE p.Id = @AppId;

SELECT 
    r.Id, 
    r.ClerkId, 
    r.StatusId,
    rt.[Key] AS ResponsibilityKey
FROM dbo.RoundRobinQueues r
INNER JOIN dbo.ResponsibilityTypes rt ON rt.Id = r.ResponsibilityTypeId
WHERE r.PropertyLeaseApplicationId = @AppId
ORDER BY r.Id DESC;
