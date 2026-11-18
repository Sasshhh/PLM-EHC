/* =====================================================================
   42-EHC-Clear-Renewal-Blockers.sql
   Clears everything that stops the CSO from recommending a lease
   renewal for ONE application reference.

   What blocks a CSO renewal in the code (LeaseDetailsController
   .RecommendForRenewal POST + MatchingHelper.IsEligibleForRenewal_*):
     B1  PaymentTransgressions where TenancyReferenceNumber = ref and the
         status key is not 'payment_transgression_status_closed'.
     B2  TenantComplaints where OfficialNumber = ref and IsActive = 1 and
         IsDeleted = 0. ANY such row blocks, even a resolved one.
     B3  72-month cap: first active LeaseDetails.StartDate is 72+ months ago.
     B4  Migrated record not completed (IsMigrated = 1, IsFullyMigrated = 0)
         hides the Submit button on the RecommendForRenewal screen.

   What this script does (soft changes only, nothing is deleted):
     B1  Sets StatusId to Closed on every transgression for the ref
         (TenancyReferenceNumber OR OfficialNumber, so the CSO panel is clean).
     B2  Sets StatusId to Resolved and IsActive = 0 on every complaint for
         the ref.
     B4  Sets IsFullyMigrated = 1 when the app is a half-finished migration.
     B3  Only reported. It is a business rule, the script does not move dates.
     Optional: closes open service requests (display only, they do not
     block). Off by default, see @CloseServiceRequests.

   Original values are copied to backup tables first, so the change can be
   undone with the ROLLBACK section at the bottom.

   HOW TO RUN
     1. Run as is: @Commit = 0 shows the preview, applies the changes inside
        a transaction, shows the after picture, then ROLLS BACK.
     2. Happy? Set @Commit = 1 and run again. Safe to run more than once.
   ===================================================================== */

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Ref                  nvarchar(50) = N'EHC2026111600001';
DECLARE @Commit               bit          = 0;   -- 0 = dry run (rollback), 1 = commit
DECLARE @CloseServiceRequests bit          = 0;   -- 1 = also close open service requests (not a blocker)
DECLARE @ActorUserName        nvarchar(100)= N'susans';

DECLARE @AppId int, @CustomerId int, @ActorId int, @Now datetime = GETDATE();
DECLARE @PtClosed int, @CmpResolved int, @SrClosed int;

SELECT TOP 1 @AppId = Id, @CustomerId = CustomerId
FROM dbo.PropertyLeaseApplications
WHERE ApplicationReferenceNumber = @Ref AND IsDeleted = 0
ORDER BY Id DESC;

IF @AppId IS NULL
BEGIN
    RAISERROR('Application %s not found. Nothing changed.', 16, 1, @Ref);
    RETURN;
END

SELECT TOP 1 @ActorId     = Id FROM dbo.SystemUsers WHERE UserName = @ActorUserName ORDER BY Id;
SELECT TOP 1 @PtClosed    = Id FROM dbo.Status WHERE [Key] = N'payment_transgression_status_closed' ORDER BY Id;
SELECT TOP 1 @CmpResolved = Id FROM dbo.Status WHERE [Key] = N'complaint_status_resolved' ORDER BY Id;
SELECT TOP 1 @SrClosed    = Id FROM dbo.Status WHERE [Key] = N'sr_status_closed' ORDER BY Id;

IF @PtClosed IS NULL OR @CmpResolved IS NULL
BEGIN
    RAISERROR('Status rows payment_transgression_status_closed / complaint_status_resolved missing. Run the upgrade master data first.', 16, 1);
    RETURN;
END

/* ---------------------------------------------------------------------
   PREVIEW (before)
   --------------------------------------------------------------------- */
PRINT '=== Application ' + @Ref + '  (PropertyLeaseApplicationId ' + CAST(@AppId AS varchar(20)) + ') ===';

SELECT 'Application' AS Section, a.Id, a.ApplicationReferenceNumber, a.CustomerId,
       s.[Key] AS AppStatus, a.IsMigrated, a.IsFullyMigrated,
       CASE WHEN a.IsMigrated = 1 AND a.IsFullyMigrated = 0 THEN 'BLOCKS (B4)' ELSE 'ok' END AS MigrationCheck
FROM dbo.PropertyLeaseApplications a
LEFT JOIN dbo.Status s ON s.Id = a.StatusId
WHERE a.Id = @AppId;

SELECT 'Leases' AS Section, l.Id AS LeaseDetailsId, l.LeaseReferenceNo, s.[Key] AS LeaseStatus,
       l.StartDate, l.EndDate, l.RenewalNotice, l.IsActive, l.IsDeleted,
       DATEDIFF(MONTH, l.StartDate, @Now) AS MonthsSinceStart
FROM dbo.LeaseDetails l
LEFT JOIN dbo.Status s ON s.Id = l.StatusId
WHERE l.PropertyLeaseApplicationId = @AppId
ORDER BY l.Id;

;WITH firstLease AS (
    SELECT TOP 1 StartDate FROM dbo.LeaseDetails
    WHERE PropertyLeaseApplicationId = @AppId AND IsActive = 1 AND IsDeleted = 0
    ORDER BY Id)
SELECT 'B3 72-month cap' AS Section, StartDate,
       ((YEAR(@Now) - YEAR(StartDate)) * 12 + MONTH(@Now) - MONTH(StartDate)) AS TotalMonths,
       CASE WHEN ((YEAR(@Now) - YEAR(StartDate)) * 12 + MONTH(@Now) - MONTH(StartDate)) < 72
            THEN 'ok' ELSE 'BLOCKS (not changed by this script)' END AS CapCheck
FROM firstLease;

SELECT 'B1 Transgressions' AS Section, p.Id, p.CaseReferenceNumber, p.TenancyReferenceNumber, p.OfficialNumber,
       s.[Key] AS StatusKey, p.TotalAmountDue, p.IsActive, p.IsDeleted,
       CASE WHEN p.TenancyReferenceNumber = @Ref AND ISNULL(s.[Key], '') <> N'payment_transgression_status_closed'
            THEN 'BLOCKS' ELSE 'shown only' END AS Effect
FROM dbo.PaymentTransgressions p
LEFT JOIN dbo.Status s ON s.Id = p.StatusId
WHERE p.TenancyReferenceNumber = @Ref OR p.OfficialNumber = @Ref
ORDER BY p.Id;

SELECT 'B2 Complaints' AS Section, c.Id, c.CaseReferenceNumber, c.OfficialNumber, s.[Key] AS StatusKey,
       c.IsActive, c.IsDeleted,
       CASE WHEN c.IsActive = 1 AND c.IsDeleted = 0 THEN 'BLOCKS' ELSE 'ok' END AS Effect
FROM dbo.TenantComplaints c
LEFT JOIN dbo.Status s ON s.Id = c.StatusId
WHERE c.OfficialNumber = @Ref
ORDER BY c.Id;

SELECT 'Service requests (not a blocker)' AS Section, r.Id, r.RequestReferenceNumber, s.[Key] AS StatusKey, r.IsActive
FROM dbo.ServiceRequests r
LEFT JOIN dbo.Status s ON s.Id = r.StatusId
WHERE r.CreatedByCustomerId = @CustomerId AND r.IsActive = 1 AND r.IsDeleted = 0
ORDER BY r.Id;

-- Not a data blocker, but the CSO only sees the lease on Renewals if this exists
SELECT 'CSO renewal queue' AS Section, q.Id, q.ClerkId, rt.[Key] AS Responsibility, s.[Key] AS QueueStatus,
       q.LeaseDetailsId, ls.[Key] AS LeaseStatusNow
FROM dbo.RoundRobinQueues q
JOIN dbo.ResponsibilityTypes rt ON rt.Id = q.ResponsibilityTypeId
LEFT JOIN dbo.Status s  ON s.Id = q.StatusId
LEFT JOIN dbo.LeaseDetails l ON l.Id = q.LeaseDetailsId
LEFT JOIN dbo.Status ls ON ls.Id = l.StatusId
WHERE q.PropertyLeaseApplicationId = @AppId
  AND rt.[Key] IN (N'r_lease_renewals', N'r_renewal_tenant_sign')
ORDER BY q.Id;

/* ---------------------------------------------------------------------
   BACKUP TABLES (created once, rows added once per record)
   --------------------------------------------------------------------- */
IF OBJECT_ID('dbo.Bak_RenewalUnblock_Transgressions') IS NULL
    CREATE TABLE dbo.Bak_RenewalUnblock_Transgressions (
        Id int NOT NULL PRIMARY KEY, AppRef nvarchar(50), StatusId int, ModifiedDateTime datetime,
        ModifiedBySystemUserId int NULL, BackedUpAt datetime NOT NULL DEFAULT GETDATE());
IF OBJECT_ID('dbo.Bak_RenewalUnblock_Complaints') IS NULL
    CREATE TABLE dbo.Bak_RenewalUnblock_Complaints (
        Id int NOT NULL PRIMARY KEY, AppRef nvarchar(50), StatusId int, IsActive bit, ModifiedDateTime datetime,
        ModifiedBySystemUserId int NULL, BackedUpAt datetime NOT NULL DEFAULT GETDATE());
IF OBJECT_ID('dbo.Bak_RenewalUnblock_ServiceRequests') IS NULL
    CREATE TABLE dbo.Bak_RenewalUnblock_ServiceRequests (
        Id int NOT NULL PRIMARY KEY, AppRef nvarchar(50), StatusId int, ModifiedDateTime datetime,
        ModifiedBySystemUserId int NULL, BackedUpAt datetime NOT NULL DEFAULT GETDATE());
IF OBJECT_ID('dbo.Bak_RenewalUnblock_Applications') IS NULL
    CREATE TABLE dbo.Bak_RenewalUnblock_Applications (
        Id int NOT NULL PRIMARY KEY, AppRef nvarchar(50), IsFullyMigrated bit,
        BackedUpAt datetime NOT NULL DEFAULT GETDATE());

/* ---------------------------------------------------------------------
   APPLY
   --------------------------------------------------------------------- */
BEGIN TRANSACTION;

    -- B1 transgressions -> Closed
    INSERT INTO dbo.Bak_RenewalUnblock_Transgressions (Id, AppRef, StatusId, ModifiedDateTime, ModifiedBySystemUserId)
    SELECT p.Id, @Ref, p.StatusId, p.ModifiedDateTime, p.ModifiedBySystemUserId
    FROM dbo.PaymentTransgressions p
    WHERE (p.TenancyReferenceNumber = @Ref OR p.OfficialNumber = @Ref)
      AND p.StatusId <> @PtClosed
      AND NOT EXISTS (SELECT 1 FROM dbo.Bak_RenewalUnblock_Transgressions b WHERE b.Id = p.Id);

    UPDATE p SET p.StatusId = @PtClosed, p.ModifiedDateTime = @Now,
                 p.ModifiedBySystemUserId = ISNULL(@ActorId, p.ModifiedBySystemUserId)
    FROM dbo.PaymentTransgressions p
    WHERE (p.TenancyReferenceNumber = @Ref OR p.OfficialNumber = @Ref)
      AND p.StatusId <> @PtClosed;
    PRINT 'B1 transgressions closed: ' + CAST(@@ROWCOUNT AS varchar(20));

    -- B2 complaints -> Resolved + inactive
    INSERT INTO dbo.Bak_RenewalUnblock_Complaints (Id, AppRef, StatusId, IsActive, ModifiedDateTime, ModifiedBySystemUserId)
    SELECT c.Id, @Ref, c.StatusId, c.IsActive, c.ModifiedDateTime, c.ModifiedBySystemUserId
    FROM dbo.TenantComplaints c
    WHERE c.OfficialNumber = @Ref
      AND (c.IsActive = 1 OR c.StatusId <> @CmpResolved)
      AND NOT EXISTS (SELECT 1 FROM dbo.Bak_RenewalUnblock_Complaints b WHERE b.Id = c.Id);

    UPDATE c SET c.StatusId = @CmpResolved, c.IsActive = 0, c.ModifiedDateTime = @Now,
                 c.ModifiedBySystemUserId = ISNULL(@ActorId, c.ModifiedBySystemUserId)
    FROM dbo.TenantComplaints c
    WHERE c.OfficialNumber = @Ref
      AND (c.IsActive = 1 OR c.StatusId <> @CmpResolved);
    PRINT 'B2 complaints resolved/deactivated: ' + CAST(@@ROWCOUNT AS varchar(20));

    -- B4 half-finished migration
    INSERT INTO dbo.Bak_RenewalUnblock_Applications (Id, AppRef, IsFullyMigrated)
    SELECT a.Id, @Ref, a.IsFullyMigrated
    FROM dbo.PropertyLeaseApplications a
    WHERE a.Id = @AppId AND a.IsMigrated = 1 AND a.IsFullyMigrated = 0
      AND NOT EXISTS (SELECT 1 FROM dbo.Bak_RenewalUnblock_Applications b WHERE b.Id = a.Id);

    UPDATE dbo.PropertyLeaseApplications SET IsFullyMigrated = 1
    WHERE Id = @AppId AND IsMigrated = 1 AND IsFullyMigrated = 0;
    PRINT 'B4 migration flag fixed: ' + CAST(@@ROWCOUNT AS varchar(20));

    -- Optional: open service requests -> Closed
    IF @CloseServiceRequests = 1 AND @SrClosed IS NOT NULL
    BEGIN
        INSERT INTO dbo.Bak_RenewalUnblock_ServiceRequests (Id, AppRef, StatusId, ModifiedDateTime, ModifiedBySystemUserId)
        SELECT r.Id, @Ref, r.StatusId, r.ModifiedDateTime, r.ModifiedBySystemUserId
        FROM dbo.ServiceRequests r
        JOIN dbo.Status s ON s.Id = r.StatusId
        WHERE r.CreatedByCustomerId = @CustomerId AND r.IsActive = 1 AND r.IsDeleted = 0
          AND s.[Key] NOT IN (N'sr_status_closed', N'sr_status_resolved')
          AND NOT EXISTS (SELECT 1 FROM dbo.Bak_RenewalUnblock_ServiceRequests b WHERE b.Id = r.Id);

        UPDATE r SET r.StatusId = @SrClosed, r.DateClosed = ISNULL(r.DateClosed, @Now), r.ModifiedDateTime = @Now,
                     r.ModifiedBySystemUserId = ISNULL(@ActorId, r.ModifiedBySystemUserId)
        FROM dbo.ServiceRequests r
        JOIN dbo.Status s ON s.Id = r.StatusId
        WHERE r.CreatedByCustomerId = @CustomerId AND r.IsActive = 1 AND r.IsDeleted = 0
          AND s.[Key] NOT IN (N'sr_status_closed', N'sr_status_resolved');
        PRINT 'Service requests closed: ' + CAST(@@ROWCOUNT AS varchar(20));
    END

    /* -------- AFTER: the same checks the app runs -------- */
    SELECT 'AFTER' AS Section,
        (SELECT COUNT(*) FROM dbo.PaymentTransgressions p JOIN dbo.Status s ON s.Id = p.StatusId
          WHERE p.TenancyReferenceNumber = @Ref AND s.[Key] <> N'payment_transgression_status_closed') AS ActiveTransgressions_mustBe0,
        (SELECT COUNT(*) FROM dbo.TenantComplaints c
          WHERE c.OfficialNumber = @Ref AND c.IsActive = 1 AND c.IsDeleted = 0) AS ActiveComplaints_mustBe0,
        (SELECT COUNT(*) FROM dbo.PropertyLeaseApplications a
          WHERE a.Id = @AppId AND a.IsMigrated = 1 AND a.IsFullyMigrated = 0) AS MigrationBlock_mustBe0;

IF @Commit = 1
BEGIN
    COMMIT TRANSACTION;
    PRINT '*** COMMITTED. The CSO can now open Recommend For Renewal for ' + @Ref + '. ***';
END
ELSE
BEGIN
    ROLLBACK TRANSACTION;
    PRINT '*** DRY RUN: rolled back. Set @Commit = 1 to apply. ***';
END
GO

/* =====================================================================
   ROLLBACK (undo a committed run). Select from here down and execute.
   Restores the original values from the backup tables for this ref.
   =====================================================================
SET XACT_ABORT ON;
DECLARE @Ref nvarchar(50) = N'EHC2026111600001';
BEGIN TRANSACTION;
    UPDATE p SET p.StatusId = b.StatusId, p.ModifiedDateTime = b.ModifiedDateTime,
                 p.ModifiedBySystemUserId = b.ModifiedBySystemUserId
    FROM dbo.PaymentTransgressions p JOIN dbo.Bak_RenewalUnblock_Transgressions b ON b.Id = p.Id
    WHERE b.AppRef = @Ref;

    UPDATE c SET c.StatusId = b.StatusId, c.IsActive = b.IsActive, c.ModifiedDateTime = b.ModifiedDateTime,
                 c.ModifiedBySystemUserId = b.ModifiedBySystemUserId
    FROM dbo.TenantComplaints c JOIN dbo.Bak_RenewalUnblock_Complaints b ON b.Id = c.Id
    WHERE b.AppRef = @Ref;

    UPDATE r SET r.StatusId = b.StatusId, r.ModifiedDateTime = b.ModifiedDateTime,
                 r.ModifiedBySystemUserId = b.ModifiedBySystemUserId
    FROM dbo.ServiceRequests r JOIN dbo.Bak_RenewalUnblock_ServiceRequests b ON b.Id = r.Id
    WHERE b.AppRef = @Ref;

    UPDATE a SET a.IsFullyMigrated = b.IsFullyMigrated
    FROM dbo.PropertyLeaseApplications a JOIN dbo.Bak_RenewalUnblock_Applications b ON b.Id = a.Id
    WHERE b.AppRef = @Ref;

    DELETE FROM dbo.Bak_RenewalUnblock_Transgressions  WHERE AppRef = @Ref;
    DELETE FROM dbo.Bak_RenewalUnblock_Complaints      WHERE AppRef = @Ref;
    DELETE FROM dbo.Bak_RenewalUnblock_ServiceRequests WHERE AppRef = @Ref;
    DELETE FROM dbo.Bak_RenewalUnblock_Applications    WHERE AppRef = @Ref;
COMMIT TRANSACTION;
PRINT 'Rollback done for ' + @Ref;
   ===================================================================== */
