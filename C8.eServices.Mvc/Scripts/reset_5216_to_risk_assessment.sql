-- ============================================================
-- Reset Application 5216 back to Risk Assessment
-- ============================================================
-- Application 5216: TC-ULD-20260306
-- Current status:   140 (Lease Agreement Generated)
-- Target status:    137 (Awaiting Risk Assessment Outcome)
--
-- RoundRobinQueue:  No existing RR queue for Risk Assessment
--                   on this app - insert a new one for ClerkId=190 (AshKay)
-- ============================================================

BEGIN TRANSACTION;

-- Step 1: Move application status back to Risk Assessment
UPDATE dbo.PropertyLeaseApplications
SET    StatusId            = 137,   -- Awaiting Risk Assessment Outcome
       ModifiedDateTime    = GETDATE()
WHERE  Id = 5216
  AND  StatusId = 140;              -- safety check: only if currently at Lease Agreement Generated

-- Step 2: Archive all existing open RR queues for this application
--         so the workflow is clean before inserting the new one
UPDATE dbo.RoundRobinQueues
SET    StatusId         = 95,       -- Archived
       IsActive         = 1,
       IsDeleted        = 0,
       ModifiedDateTime = GETDATE()
WHERE  PropertyLeaseApplicationId = 5216
  AND  StatusId = 99;              -- only close ones still Submitted/open

-- Step 3: Insert a fresh Risk Assessment RR queue entry for AshKay (ClerkId=190)
INSERT INTO dbo.RoundRobinQueues
(
    ClerkId,
    RCSApplicationStatusId,
    ResponsibilityTypeId,
    StatusId,
    IsActive,
    IsDeleted,
    IsLocked,
    CreatedBySystemUserId,
    CreatedDateTime,
    ModifiedBySystemUserId,
    ModifiedDateTime,
    PropertyLeaseApplicationId,
    LeaseDetailsId,
    DepartmentId
)
SELECT
    190,                                        -- ClerkId = AshKay
    137,                                        -- RCSApplicationStatusId = Awaiting Risk Assessment
    11,                                         -- ResponsibilityTypeId = Risk Assessment
    99,                                         -- StatusId = Submitted (open/active)
    1,                                          -- IsActive
    0,                                          -- IsDeleted
    0,                                          -- IsLocked
    1214,                                       -- CreatedBySystemUserId = AshKay SystemUser
    GETDATE(),
    1214,
    GETDATE(),
    5216,                                       -- PropertyLeaseApplicationId
    LeaseDetailsId,                             -- carry over from latest existing queue entry
    DepartmentId
FROM dbo.RoundRobinQueues
WHERE PropertyLeaseApplicationId = 5216
ORDER BY Id DESC
OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY;

-- Step 4: Verify
SELECT
    a.Id,
    a.ApplicationReferenceNumber,
    s.Name  AS ApplicationStatus,
    s.[Key] AS ApplicationStatusKey
FROM dbo.PropertyLeaseApplications a
JOIN dbo.Status s ON s.Id = a.StatusId
WHERE a.Id = 5216;

SELECT
    rq.Id,
    rq.PropertyLeaseApplicationId,
    rt.Name  AS ResponsibilityName,
    st.Name  AS QueueStatus,
    rq.ClerkId,
    rq.IsActive,
    rq.IsDeleted
FROM dbo.RoundRobinQueues rq
LEFT JOIN dbo.ResponsibilityTypes rt ON rt.Id = rq.ResponsibilityTypeId
LEFT JOIN dbo.Status             st ON st.Id  = rq.StatusId
WHERE rq.PropertyLeaseApplicationId = 5216
ORDER BY rq.Id DESC;

-- If everything looks correct, COMMIT. Otherwise ROLLBACK.
COMMIT TRANSACTION;
-- ROLLBACK TRANSACTION;
