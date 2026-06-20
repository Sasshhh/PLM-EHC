-- ==============================================================
-- PLM DIAGNOSTIC: Renewal Case Investigator
-- Usage: Replace @Ref value with the real reference number
-- Run on: production DB to diagnose stuck renewal cases
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
-- 2. OPEN ROUNDROBIN QUEUES  (IsActive=1)
-- If anything here has a Submitted status it means someone still
-- has an outstanding queue item blocking the workflow.
-- ==============================================================
PRINT ''
PRINT '=== 2. All RoundRobin Queues (open + history) ==='
SELECT
    RRQ.Id                                          AS [RRQ Id],
    RT.[Name]                                       AS [Queue],
    RT.[Key]                                        AS [Queue Key],
    CONCAT(SU.FirstName, ' ', SU.LastName)          AS [Assigned To],
    SU.EmailAddress                                 AS [Email],
    S.[Name]                                        AS [RRQ Status],
    RRQ.IsActive,
    RRQ.IsDeleted,
    RRQ.CreatedDateTime                             AS [Created],
    RRQ.EndTaskDateTime                             AS [Closed]
FROM RoundRobinQueues RRQ
JOIN PropertyLeaseApplications PA  ON RRQ.PropertyLeaseApplicationId = PA.Id
LEFT JOIN ResponsibilityTypes RT   ON RRQ.ResponsibilityTypeId = RT.Id
LEFT JOIN SystemUsers SU           ON RRQ.ClerkId = SU.Id
LEFT JOIN Status S                 ON RRQ.StatusId = S.Id
WHERE PA.ApplicationReferenceNumber = @Ref
ORDER BY RRQ.CreatedDateTime DESC

-- ==============================================================
-- 3. LEASE DETAILS STATUS
-- ==============================================================
PRINT ''
PRINT '=== 3. Lease Details ==='
SELECT
    LD.Id               AS [Lease Id],
    LD.LeaseReferenceNo,
    LD.StartDate,
    LD.EndDate,
    LD.PeriodInMonths,
    LD.MonthsOffered,
    S.[Name]            AS [Lease Status],
    S.[Key]             AS [Lease Status Key],
    LD.ModifiedDateTime AS [Last Modified]
FROM LeaseDetails LD
JOIN PropertyLeaseApplications PA ON LD.PropertyLeaseApplicationId = PA.Id
LEFT JOIN Status S ON LD.StatusId = S.Id
WHERE PA.ApplicationReferenceNumber = @Ref

-- ==============================================================
-- 4. RENEWAL OFFER — CEO SIGNATURE STATE
-- CEO_Outcome populated + CEO_Date set = CEO has signed.
-- If signed but app status still shows old status = bug in
-- post-sign status transition. Use Section 5 fix below.
-- ==============================================================
PRINT ''
PRINT '=== 4. Renewal Offer & CEO Signature ==='
SELECT
    RO.Id               AS [Offer Id],
    RO.MonthsOffer,
    RO.ProposedEndDate,
    RO.CSO_Outcome,
    RO.CSO_Date,
    RO.RM_Outcome,
    RO.RM_Date,
    RO.CEO_Outcome,
    RO.CEO_Date,
    RO.CEO_SystemUserId,
    CONCAT(SU.FirstName,' ',SU.LastName) AS [CEO Signed By],
    RO.IsAccepted       AS [Tenant Accepted?],
    RO.CustomerResponseDate,
    RO.IsActive,
    RO.ModifiedDateTime
FROM PropertyLeaseRenewalOffers RO
JOIN PropertyLeaseApplications PA  ON RO.PropertyLeaseApplicationId = PA.Id
LEFT JOIN SystemUsers SU           ON RO.CEO_SystemUserId = SU.Id
WHERE PA.ApplicationReferenceNumber = @Ref
ORDER BY RO.CreatedDateTime DESC

-- ==============================================================
-- 5. DIAGNOSIS SUMMARY
-- Tells you exactly what is wrong and what to fix.
-- ==============================================================
PRINT ''
PRINT '=== 5. Diagnosis ==='
SELECT
    PA.ApplicationReferenceNumber                       AS [Ref],
    AppStatus.[Name]                                    AS [App Status],
    AppStatus.[Key]                                     AS [App Status Key],
    RO.CEO_Outcome                                      AS [CEO Outcome],
    RO.CEO_Date                                         AS [CEO Signed Date],
    RO.IsAccepted                                       AS [Tenant Accepted],
    (SELECT COUNT(*) FROM RoundRobinQueues RRQ2
     JOIN Status RS ON RRQ2.StatusId = RS.Id
     WHERE RRQ2.PropertyLeaseApplicationId = PA.Id
       AND RRQ2.IsActive = 1
       AND RS.[Key] = 'rs_submitted')                   AS [Open Submitted RRQs],
    CASE
        WHEN RO.CEO_Outcome IS NOT NULL
         AND AppStatus.[Key] NOT IN (
             's_awaiting_renewal_tenant_signature',
             's_renewal_agree_success',
             's_awaiting_renewal_rm_signature',
             's_awaiting_renewal_ceo_signature',
             's_renewal_agreement_all_signed',
             's_awaiting_renewal_docs'
         )
        THEN '⚠ CEO signed but app status not advanced — status transition bug'
        WHEN RO.CEO_Outcome IS NULL
        THEN 'CEO has NOT signed yet'
        ELSE 'Status looks correct'
    END                                                 AS [Diagnosis]
FROM PropertyLeaseApplications PA
JOIN Status AppStatus ON PA.StatusId = AppStatus.Id
LEFT JOIN PropertyLeaseRenewalOffers RO ON RO.PropertyLeaseApplicationId = PA.Id
    AND RO.IsActive = 1 AND RO.IsDeleted = 0
WHERE PA.ApplicationReferenceNumber = @Ref
