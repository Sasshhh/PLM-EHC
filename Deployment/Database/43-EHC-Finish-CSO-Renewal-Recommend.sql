/* =====================================================================
   43-EHC-Finish-CSO-Renewal-Recommend.sql
   Finishes a CSO "Recommend for renewal" that crashed half way with
     FK_dbo.PLMApplicationHistortyLogs_dbo.SystemUsers_CreatedBySystemUserId

   What the crash leaves behind: the CSO comment and the renewal offer
   (PropertyLeaseRenewalOffers, CSO_Outcome = 12/24 months) are saved, the
   CSO queue item may already be archived, but the lease status was not
   moved on and no Revenue Manager review task was created. So the lease
   can drop off the CSO's Renewals list and never reach the RM.

   This script does what the app would have done after the crash point:
     1. Lease status -> s_awaiting_renewal_review_outcome
     2. Archive the CSO renewal queue item (r_lease_renewals)
     3. Create the Revenue Manager review task (r_lease_renewal) for the
        customer in AppSettings 'PLM RevenueManager', if none is open
     4. Write the history log line the app failed to write

   Only runs if a CSO outcome is recorded on the renewal offer.
   @Commit = 0 is a dry run (preview, apply, show after, roll back).
   Set @Commit = 1 to keep it. Safe to run more than once.
   ===================================================================== */

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Ref    nvarchar(50) = N'EHC2026111600001';
DECLARE @Commit bit          = 0;   -- 0 = dry run (rollback), 1 = commit

DECLARE @AppId int, @LeaseId int, @OfferId int, @Months int, @CsoSu int, @CsoCustomerId int;
DECLARE @StReview int, @StArchived int, @StSubmitted int, @RespCso int, @RespRm int, @RmClerk int;
DECLARE @Now datetime = GETDATE();

SELECT TOP 1 @AppId = Id FROM dbo.PropertyLeaseApplications
WHERE ApplicationReferenceNumber = @Ref AND IsDeleted = 0 ORDER BY Id DESC;
IF @AppId IS NULL BEGIN RAISERROR('Application %s not found.', 16, 1, @Ref); RETURN; END

SELECT TOP 1 @OfferId = o.Id, @Months = o.MonthsOffer, @CsoSu = o.CSO_SystemUserId
FROM dbo.PropertyLeaseRenewalOffers o
WHERE o.PropertyLeaseApplicationId = @AppId AND o.IsDeleted = 0 AND o.CSO_Outcome IS NOT NULL
ORDER BY o.Id DESC;

SELECT TOP 1 @LeaseId = Id FROM dbo.LeaseDetails
    WHERE PropertyLeaseApplicationId = @AppId AND IsDeleted = 0 ORDER BY Id DESC;

SELECT TOP 1 @StReview    = Id FROM dbo.Status WHERE [Key] = N's_awaiting_renewal_review_outcome' ORDER BY Id;
SELECT TOP 1 @StArchived  = Id FROM dbo.Status WHERE [Key] = N's_rcs_Archived'  ORDER BY Id;
SELECT TOP 1 @StSubmitted = Id FROM dbo.Status WHERE [Key] = N's_rcs_Submitted' ORDER BY Id;
SELECT TOP 1 @RespCso = Id FROM dbo.ResponsibilityTypes WHERE [Key] = N'r_lease_renewals' ORDER BY Id;
SELECT TOP 1 @RespRm  = Id FROM dbo.ResponsibilityTypes WHERE [Key] = N'r_lease_renewal'  ORDER BY Id;
SELECT TOP 1 @RmClerk = TRY_CAST([Value] AS int) FROM dbo.AppSettings WHERE [Key] = N'PLM RevenueManager' ORDER BY Id;
SELECT TOP 1 @CsoCustomerId = Id FROM dbo.Customers WHERE SystemUserId = @CsoSu ORDER BY Id;

/* ---------------- PREVIEW ---------------- */
SELECT 'Renewal offer' AS Section, o.Id, o.MonthsOffer, o.CSO_Outcome, o.CSO_Date,
       o.CSO_SystemUserId, o.IsAccepted
FROM dbo.PropertyLeaseRenewalOffers o
WHERE o.PropertyLeaseApplicationId = @AppId ORDER BY o.Id;

SELECT 'Lease' AS Section, l.Id, l.LeaseReferenceNo, s.[Key] AS LeaseStatus
FROM dbo.LeaseDetails l LEFT JOIN dbo.Status s ON s.Id = l.StatusId
WHERE l.PropertyLeaseApplicationId = @AppId ORDER BY l.Id;

SELECT 'Renewal queue' AS Section, q.Id, q.ClerkId, rt.[Key] AS Responsibility, s.[Key] AS QueueStatus,
       q.LeaseDetailsId, q.CurrentTaskDateTime, q.EndTaskDateTime
FROM dbo.RoundRobinQueues q
JOIN dbo.ResponsibilityTypes rt ON rt.Id = q.ResponsibilityTypeId
LEFT JOIN dbo.Status s ON s.Id = q.StatusId
WHERE q.PropertyLeaseApplicationId = @AppId
  AND rt.[Key] IN (N'r_lease_renewals', N'r_renewal_tenant_sign', N'r_lease_renewal')
ORDER BY q.Id;

SELECT 'Settings' AS Section, @RmClerk AS RevenueManagerCustomerId, @CsoSu AS CsoSystemUserId,
       @CsoCustomerId AS CsoCustomerId, @StReview AS StReview, @RespRm AS RespRm;

IF @OfferId IS NULL
BEGIN
    PRINT 'No CSO outcome recorded on a renewal offer for ' + @Ref + '. Nothing to finish: redo the CSO step in the app.';
    RETURN;
END
IF @StReview IS NULL OR @StArchived IS NULL OR @StSubmitted IS NULL OR @RespRm IS NULL OR ISNULL(@RmClerk, 0) = 0
BEGIN
    RAISERROR('Missing status / responsibility / PLM RevenueManager setting. Nothing changed.', 16, 1);
    RETURN;
END

/* ---------------- APPLY ---------------- */
BEGIN TRANSACTION;

    UPDATE dbo.LeaseDetails SET StatusId = @StReview, ModifiedDateTime = @Now
    WHERE Id = @LeaseId AND StatusId <> @StReview;
    PRINT 'Lease status moved: ' + CAST(@@ROWCOUNT AS varchar(10));

    IF @RespCso IS NOT NULL
    BEGIN
        UPDATE dbo.RoundRobinQueues SET StatusId = @StArchived, EndTaskDateTime = ISNULL(EndTaskDateTime, @Now)
        WHERE PropertyLeaseApplicationId = @AppId AND ResponsibilityTypeId = @RespCso AND StatusId <> @StArchived;
        PRINT 'CSO queue items archived: ' + CAST(@@ROWCOUNT AS varchar(10));
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.RoundRobinQueues
                   WHERE PropertyLeaseApplicationId = @AppId AND ResponsibilityTypeId = @RespRm
                     AND StatusId = @StSubmitted)
    BEGIN
        INSERT INTO dbo.RoundRobinQueues
            (PropertyLeaseApplicationId, LeaseDetailsId, ResponsibilityTypeId, CurrentTaskDateTime, ClerkId,
             StatusId, IsActive, IsDeleted, CreatedDateTime)
        VALUES (@AppId, @LeaseId, @RespRm, @Now, @RmClerk, @StSubmitted, 1, 0, @Now);
        PRINT 'Revenue Manager review task created.';
    END
    ELSE PRINT 'Revenue Manager review task already open.';

    IF @CsoCustomerId IS NOT NULL AND NOT EXISTS (
        SELECT 1 FROM dbo.PLMApplicationHistortyLogs
        WHERE PropertyLeaseApplicationId = @AppId AND AuditAction LIKE N'Lease renewal recommended for Revenue Manager review%')
    BEGIN
        INSERT INTO dbo.PLMApplicationHistortyLogs
            (UserId, PropertyLeaseApplicationId, AuditAction, IsActive, IsDeleted, IsLocked, CreatedDateTime, CreatedBySystemUserId)
        SELECT @CsoCustomerId, @AppId,
               N'Lease renewal recommended for Revenue Manager review (' + CAST(@Months AS nvarchar(10)) + N' months): ' + ISNULL(l.LeaseReferenceNo, @Ref),
               1, 0, 0, @Now, @CsoSu
        FROM dbo.LeaseDetails l WHERE l.Id = @LeaseId;
        PRINT 'History log written.';
    END

    /* AFTER */
    SELECT 'AFTER' AS Section, l.Id AS LeaseId, s.[Key] AS LeaseStatus,
           (SELECT COUNT(*) FROM dbo.RoundRobinQueues q WHERE q.PropertyLeaseApplicationId = @AppId
              AND q.ResponsibilityTypeId = @RespRm AND q.StatusId = @StSubmitted AND q.ClerkId = @RmClerk) AS OpenRmTasks_mustBe1
    FROM dbo.LeaseDetails l LEFT JOIN dbo.Status s ON s.Id = l.StatusId WHERE l.Id = @LeaseId;

IF @Commit = 1 BEGIN COMMIT TRANSACTION; PRINT '*** COMMITTED. Log in as the Revenue Manager: Renewals > Revenue Review. ***'; END
ELSE BEGIN ROLLBACK TRANSACTION; PRINT '*** DRY RUN: rolled back. Set @Commit = 1 to apply. ***'; END
GO
