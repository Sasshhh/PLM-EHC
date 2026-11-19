/* =====================================================================
   45-EHC-Fix-Stuck-Lease-Status.sql
   Puts ONE lease back on the right step after two old-build dead ends:

   A) s_awiting_property_eviction  ->  s_awaiting_exit_inspec
      The old "Conduct Exit Inspection" button on Termination > Terminations
      (leaseDetails screen) approved into "Awaiting Property Eviction". The
      Property Manager / CEO "Property Eviction" step then crashed. With the
      new build the CSO redoes the exit inspection on the proper screen
      (Habitable / Minor defects / Not habitable -> move-out confirmation).

   B) s_lease_renewal_agreement_concluded  ->  l_active_lease
      Renewals concluded before build a73d465 left the lease on "Lease
      Renewal Agreement Concluded" instead of Active, so the CSO's
      Terminate screen (active leases only) did not list it.

   HOW TO RUN: set @Ref. @Commit = 0 previews and rolls back; 1 applies.
   Old values go to dbo.Bak_StuckLeaseStatus (rollback block at the end).
   ===================================================================== */
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Ref    nvarchar(50) = N'EHC2026111600001';  -- application OR lease reference
DECLARE @Commit bit = 0;

DECLARE @AppId int, @LeaseId int, @Cur nvarchar(100), @Target int, @TargetKey nvarchar(100);
SELECT TOP 1 @AppId = a.Id FROM dbo.PropertyLeaseApplications a
 WHERE a.ApplicationReferenceNumber = @Ref
    OR a.Id IN (SELECT PropertyLeaseApplicationId FROM dbo.LeaseDetails WHERE LeaseReferenceNo = @Ref)
 ORDER BY a.Id DESC;
SELECT TOP 1 @LeaseId = l.Id, @Cur = s.[Key]
FROM dbo.LeaseDetails l JOIN dbo.Status s ON s.Id = l.StatusId
WHERE l.PropertyLeaseApplicationId = @AppId AND l.IsNew = 1 AND l.IsDeleted = 0 ORDER BY l.Id DESC;

IF @LeaseId IS NULL BEGIN PRINT 'No current lease row for ' + @Ref; RETURN; END
PRINT 'Lease ' + CAST(@LeaseId AS varchar) + ' is on ' + @Cur;

SET @TargetKey = CASE @Cur WHEN N's_awiting_property_eviction' THEN N's_awaiting_exit_inspec'
                           WHEN N's_lease_renewal_agreement_concluded' THEN N'l_active_lease' END;
IF @TargetKey IS NULL BEGIN PRINT 'Nothing to do: this script only fixes s_awiting_property_eviction and s_lease_renewal_agreement_concluded.'; RETURN; END
SELECT TOP 1 @Target = Id FROM dbo.Status WHERE [Key] = @TargetKey ORDER BY Id;
IF @Target IS NULL BEGIN PRINT 'Missing status ' + @TargetKey; RETURN; END

IF OBJECT_ID('dbo.Bak_StuckLeaseStatus') IS NULL
    CREATE TABLE dbo.Bak_StuckLeaseStatus (LeaseId int, AppId int, OldLeaseStatusId int, OldAppStatusId int, BackedUpAt datetime DEFAULT GETDATE());

BEGIN TRAN;
    INSERT dbo.Bak_StuckLeaseStatus (LeaseId, AppId, OldLeaseStatusId, OldAppStatusId)
    SELECT l.Id, a.Id, l.StatusId, a.StatusId FROM dbo.LeaseDetails l JOIN dbo.PropertyLeaseApplications a ON a.Id = l.PropertyLeaseApplicationId WHERE l.Id = @LeaseId;

    UPDATE dbo.LeaseDetails SET StatusId = @Target WHERE Id = @LeaseId;

    SELECT l.Id AS LeaseId, s.[Key] AS LeaseStatusNow FROM dbo.LeaseDetails l JOIN dbo.Status s ON s.Id = l.StatusId WHERE l.Id = @LeaseId;
IF @Commit = 1 BEGIN COMMIT; PRINT 'COMMITTED.'; END
ELSE BEGIN ROLLBACK; PRINT 'DRY RUN rolled back. Set @Commit = 1 to apply.'; END

/* ROLLBACK (run by hand):
UPDATE l SET l.StatusId = b.OldLeaseStatusId
FROM dbo.LeaseDetails l JOIN (SELECT *, ROW_NUMBER() OVER (PARTITION BY LeaseId ORDER BY BackedUpAt DESC) rn FROM dbo.Bak_StuckLeaseStatus) b
  ON b.LeaseId = l.Id AND b.rn = 1;
*/
