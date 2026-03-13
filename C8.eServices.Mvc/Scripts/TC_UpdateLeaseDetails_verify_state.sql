-- ============================================================
-- TC: Update Lease Details - State Verification Query
-- ============================================================
-- Purpose  : Run at any point during testing to see the full
--            state of the TC-ULD test application.
--            Does NOT modify any data - read-only.
--
-- Run after: TC_UpdateLeaseDetails_seed_and_reset.sql
-- ============================================================

SET NOCOUNT ON;

DECLARE @AppId INT = (
    SELECT TOP 1 Id FROM PropertyLeaseApplications
    WHERE ApplicationReferenceNumber LIKE 'TC-ULD-%'
      AND IsDeleted = 0
    ORDER BY Id DESC
)

IF @AppId IS NULL
BEGIN
    PRINT 'No TC-ULD application found. Run TC_UpdateLeaseDetails_seed_and_reset.sql first.'
    RETURN
END

PRINT 'Checking application Id: ' + CAST(@AppId AS VARCHAR)
PRINT ''

-- --------------------------------------------------------
-- 1. Application + Status
-- --------------------------------------------------------
PRINT '=== APPLICATION ==='
SELECT
    p.Id                                AS AppId,
    p.ApplicationReferenceNumber,
    s.Name                              AS [Status],
    s.[Key]                             AS StatusKey,
    p.FirstName + ' ' + p.LastName      AS Applicant,
    p.PurEmail,
    p.IDNo,
    p.CellNo,
    p.GrossIncome,
    p.NetIncome,
    p.IsActive,
    p.IsDeleted,
    p.CreatedDateTime,
    p.ModifiedDateTime
FROM PropertyLeaseApplications p
INNER JOIN Status s ON s.Id = p.StatusId
WHERE p.Id = @AppId

-- --------------------------------------------------------
-- 2. Lease Agreement Master (the PDF data store)
-- --------------------------------------------------------
PRINT '=== LEASE AGREEMENT MASTER ==='
SELECT
    lam.Id,
    lam.ApplicantFullName,
    lam.ApplicantIdentityNumber,
    lam.CommencementDate,
    lam.EndDate,
    lam.MonthlyUnitRental,
    lam.LeaseAdministrationFee,
    lam.PreparationFee,
    lam.CreditCheckFee,
    lam.InitialDepositAmonunt,
    lam.Electricity,     lam.ELEC,
    lam.Water,           lam.WTR,
    lam.Refuse,
    lam.Sewerage,
    lam.SecurityFee,     lam.SEC,
    lam.ShadePortParking,lam.SPP,
    lam.OpenParking,     lam.OPP,
    lam.StoreRooms,      lam.STR,
    lam.UnitNumber,
    lam.FloorNumber,
    lam.BlockNumber,
    lam.BedRooms,
    lam.OccupantONE,     lam.OccupantONEIdentityNo,
    lam.OccupantTWO,     lam.OccupantTWOIdentityNo,
    lam.TenantSigned,
    lam.PropertyManagerSigned,
    lam.RevenueManagerSigned
FROM PropertyLeaseAgreementMasters lam
WHERE lam.PropertyLeaseApplicationId = @AppId

IF @@ROWCOUNT = 0
    PRINT '  (no master record yet - expected before form submit)'

-- --------------------------------------------------------
-- 3. LeaseDetails
-- --------------------------------------------------------
PRINT '=== LEASE DETAILS ==='
SELECT
    ld.Id,
    ld.LeaseReferenceNo,
    s.Name          AS [Status],
    ld.StartDate,
    ld.EndDate,
    ld.RentalAmount,
    ld.DepositeAmount,
    ld.TotalIncludingVAT,
    ld.DetailsUpdated,
    ld.IsDeleted
FROM LeaseDetails ld
INNER JOIN Status s ON s.Id = ld.StatusId
WHERE ld.PropertyLeaseApplicationId = @AppId
ORDER BY ld.Id DESC

IF @@ROWCOUNT = 0
    PRINT '  (no LeaseDetails yet)'

-- --------------------------------------------------------
-- 4. Matched Unit (unit linked via MatchedUnits)
-- --------------------------------------------------------
PRINT '=== MATCHED UNIT ==='
SELECT
    mu.Id                   AS MatchedUnitId,
    mu.IsAccepted,
    mu.IsDeleted            AS MatchDeleted,
    aap.Id                  AS UnitId,
    aap.SpaceUnitNumber,
    aap.StreetName,
    aap.Township,
    aap.BuildingName,
    aap.NumOfBeds,
    aap.MonthlyRentalAmount,
    aap.OfferedComplexId
FROM MatchedUnits mu
INNER JOIN ApplicationAllocatedProperties aap ON aap.Id = mu.ApplicationAllocatedPropertyId
WHERE mu.PropertyLeaseApplicationId = @AppId

IF @@ROWCOUNT = 0
    PRINT '  (no matched unit)'

-- --------------------------------------------------------
-- 5. Round Robin Queue entries
-- --------------------------------------------------------
PRINT '=== ROUND ROBIN QUEUE ==='
SELECT
    rrq.Id                          AS QueueId,
    rt.Name                         AS ResponsibilityType,
    rt.[Key]                        AS ResponsibilityKey,
    qs.Name                         AS QueueStatus,
    rrq.CurrentTaskDateTime         AS AssignedDate,
    rrq.EndTaskDateTime             AS CompletedDate,
    su.FirstName + ' ' + su.LastName        AS AssignedTo
FROM RoundRobinQueues rrq
INNER JOIN ResponsibilityTypes rt ON rt.Id = rrq.ResponsibilityTypeId
LEFT  JOIN Status qs ON qs.Id = rrq.StatusId
LEFT  JOIN SystemUsers su ON su.Id = rrq.ClerkId
WHERE rrq.PropertyLeaseApplicationId = @AppId
  AND rrq.PropertyLeaseApplicationId IS NOT NULL
ORDER BY rrq.CreatedDateTime DESC

IF @@ROWCOUNT = 0
    PRINT '  (no RR queue entries)'

-- --------------------------------------------------------
-- 6. PLM History (last 5 entries)
-- --------------------------------------------------------
PRINT '=== PLM HISTORY (last 5) ==='
SELECT TOP 5
    h.CreatedDateTime,
    h.AuditAction
FROM PLMApplicationHistortyLogs h
WHERE h.PropertyLeaseApplicationId = @AppId
ORDER BY h.Id DESC

-- --------------------------------------------------------
-- 7. WORKFLOW STATUS INDICATOR
-- --------------------------------------------------------
PRINT ''
PRINT '=== WORKFLOW STATUS ==='
SELECT
    CASE s.[Key]
        WHEN 's_awaiting_tenant_update_record'        THEN '? Ready for UpdateLeaseDetails'
        WHEN 's_awaiting_inspection_schedule_slots'   THEN '?? Wrong status - was inspection slots'
        WHEN 'a_awaiting_lease_agreement'             THEN '?? Past lease details - awaiting agreement'
        WHEN 'a_lease_generated'                      THEN '?? Lease agreement generated'
        WHEN 's_awaiting_agreement_review_'           THEN '?? Awaiting review'
        WHEN 's_awaiting_agreement_approval_'         THEN '?? Awaiting approval'
        ELSE '? Status: ' + s.[Key]
    END AS WorkflowPosition,
    ISNULL(
        (SELECT TOP 1 '? Master exists'
         FROM PropertyLeaseAgreementMasters
         WHERE PropertyLeaseApplicationId = @AppId),
        '? No master yet'
    ) AS MasterData,
    ISNULL(
        (SELECT TOP 1 '? LeaseDetails exists'
         FROM LeaseDetails
         WHERE PropertyLeaseApplicationId = @AppId AND IsDeleted = 0),
        '? No LeaseDetails'
    ) AS LeaseData
FROM PropertyLeaseApplications p
INNER JOIN Status s ON s.Id = p.StatusId
WHERE p.Id = @AppId
