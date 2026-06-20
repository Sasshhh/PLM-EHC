-- ==============================================================
-- PLM DIAGNOSTIC: Lease Agreement CEO Approval Investigator
-- Flow: Generate Agreement → Tenant Signs → RM Signs → CEO Signs
--       → Status should move to Active Lease
-- Usage: Replace @Ref with the real reference number
-- Tested on: CRMPLMDEV_2025 (dev) 2026-05-20
-- ==============================================================

DECLARE @Ref NVARCHAR(50) = 'EHC2026041000009'   -- ← CHANGE THIS

-- ==============================================================
-- 1. CURRENT APPLICATION STATUS
-- ==============================================================
PRINT '=== 1. Current Application Status ==='
SELECT
    PA.ApplicationReferenceNumber   AS [Ref Number],
    PA.ApplicantFullName            AS [Applicant],
    S.[Name]                        AS [Current Status],
    S.[Key]                         AS [Status Key],
    PA.ModifiedDateTime             AS [Last Modified]
FROM PropertyLeaseApplications PA
JOIN Status S ON PA.StatusId = S.Id
WHERE PA.ApplicationReferenceNumber = @Ref

-- ==============================================================
-- 2. ALL ROUNDROBIN QUEUES (full history)
-- Look for: any Submitted queue blocking progress
-- Expected at CEO approval stage: r_lease_agreement_validation
--   should be Submitted to the CEO/Property Manager
-- ==============================================================
PRINT ''
PRINT '=== 2. RoundRobin Queue History ==='
SELECT
    RRQ.Id                                      AS [RRQ Id],
    RT.[Name]                                   AS [Queue],
    RT.[Key]                                    AS [Queue Key],
    CONCAT(SU.FirstName, ' ', SU.LastName)      AS [Assigned To],
    SU.EmailAddress                             AS [User Email],
    S.[Name]                                    AS [RRQ Status],
    RRQ.IsActive,
    RRQ.IsDeleted,
    RRQ.CreatedDateTime                         AS [Created],
    RRQ.EndTaskDateTime                         AS [Closed]
FROM RoundRobinQueues RRQ
JOIN PropertyLeaseApplications PA  ON RRQ.PropertyLeaseApplicationId = PA.Id
LEFT JOIN ResponsibilityTypes RT   ON RRQ.ResponsibilityTypeId = RT.Id
LEFT JOIN SystemUsers SU           ON RRQ.ClerkId = SU.Id
LEFT JOIN Status S                 ON RRQ.StatusId = S.Id
WHERE PA.ApplicationReferenceNumber = @Ref
ORDER BY RRQ.CreatedDateTime DESC

-- ==============================================================
-- 3. LEASE DETAILS
-- ==============================================================
PRINT ''
PRINT '=== 3. Lease Details ==='
SELECT
    LD.Id               AS [Lease Id],
    LD.LeaseReferenceNo,
    LD.StartDate,
    LD.EndDate,
    LD.PeriodInMonths,
    LD.IsNew,
    LD.Completed,
    LD.IsActive,
    S.[Name]            AS [Lease Status],
    S.[Key]             AS [Lease Status Key],
    LD.ModifiedDateTime AS [Last Modified]
FROM LeaseDetails LD
JOIN PropertyLeaseApplications PA ON LD.PropertyLeaseApplicationId = PA.Id
LEFT JOIN Status S ON LD.StatusId = S.Id
WHERE PA.ApplicationReferenceNumber = @Ref

-- ==============================================================
-- 4. LEASE AGREEMENT MASTER — SIGNATURE STATE
-- Shows who has signed and who hasn't.
-- All three (Tenant, RM, CEO/PM) must be signed before
-- the status advances to Active Lease.
-- ==============================================================
PRINT ''
PRINT '=== 4. Lease Agreement Master - Signature State ==='
SELECT
    LAM.Id                                  AS [Agreement Master Id],
    LAM.TenantSigned,
    LAM.TenantSignDate                      AS [Tenant Sign Date],
    LAM.RevenueManagerSigned,
    LAM.RevenueManagerSignatureDate         AS [RM Sign Date],
    LAM.PropertyManagerSigned              AS [CEO/PM Signed],
    LAM.PropertyManagerSignatureDate        AS [CEO Sign Date],
    LAM.RenewalPropertyManagerSigned,
    LAM.IsActive,
    LAM.IsDeleted,
    LAM.CreatedDateTime,
    LAM.ModifiedDateTime
FROM PropertyLeaseAgreementMasters LAM
JOIN PropertyLeaseApplications PA ON LAM.PropertyLeaseApplicationId = PA.Id
WHERE PA.ApplicationReferenceNumber = @Ref
ORDER BY LAM.Id DESC

-- ==============================================================
-- 5. DIAGNOSIS SUMMARY
-- ==============================================================
PRINT ''
PRINT '=== 5. Diagnosis Summary ==='
SELECT
    PA.ApplicationReferenceNumber               AS [Ref],
    AppStatus.[Name]                            AS [App Status],
    AppStatus.[Key]                             AS [App Status Key],
    LAM.TenantSigned                            AS [Tenant Signed],
    LAM.RevenueManagerSigned                    AS [RM Signed],
    LAM.PropertyManagerSigned                   AS [CEO/PM Signed],
    LAM.PropertyManagerSignatureDate            AS [CEO Signed Date],
    (SELECT COUNT(*) FROM RoundRobinQueues RRQ2
     JOIN Status RS ON RRQ2.StatusId = RS.Id
     WHERE RRQ2.PropertyLeaseApplicationId = PA.Id
       AND RRQ2.IsActive = 1
       AND RS.[Key] = 'rs_submitted')           AS [Open Submitted RRQs],
    CASE
        WHEN LAM.PropertyManagerSigned = 1
         AND AppStatus.[Key] NOT IN ('l_active_lease', 's_awaiting_debit_order', 's_lease_active')
        THEN 'CEO signed but status not advanced to Active Lease — was ApprovalStatusddl null on post?'
        WHEN LAM.PropertyManagerSigned = 0 OR LAM.PropertyManagerSigned IS NULL
        THEN 'CEO has NOT signed yet'
        WHEN AppStatus.[Key] = 'l_active_lease'
        THEN 'Looks correct — status is Active Lease'
        ELSE 'Review manually'
    END                                         AS [Diagnosis]
FROM PropertyLeaseApplications PA
JOIN Status AppStatus ON PA.StatusId = AppStatus.Id
LEFT JOIN PropertyLeaseAgreementMasters LAM
    ON LAM.PropertyLeaseApplicationId = PA.Id
    AND LAM.IsActive = 1 AND LAM.IsDeleted = 0
WHERE PA.ApplicationReferenceNumber = @Ref
