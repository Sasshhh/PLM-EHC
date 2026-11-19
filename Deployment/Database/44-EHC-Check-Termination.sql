/* =====================================================================
   44-EHC-Check-Termination.sql
   Why does a tenant's termination notice not show on the CSO's
   Termination > Terminations screen?

   How the screen picks rows (PropertyLeaseApplicationTerminations, CSO):
     LeaseDetails.IsDeleted = 0 AND LeaseDetails.IsNew = 1 AND status is
     s_awaiting_cso_termination_review (or eviction / exit inspection).
     The CSO's own queue is NOT needed: every CSO sees every notice.
   Other roles (Revenue Manager, Property Manager, Director) do NOT see
   s_awaiting_cso_termination_review. The Revenue Officer only sees it
   after the CSO supports it (s_awaiting_termination_appraisal + an open
   r_termination_validation queue row).

   Part 1 (always): read-only report + a one-line diagnosis.
   Part 2 (@Fix = 1): if the notice landed on an OLD lease row (IsNew = 0,
     replaced by a renewal), move it to the current lease row so the CSO
     sees it. @Commit = 0 previews inside a transaction and rolls back.
   ===================================================================== */
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Ref    nvarchar(50) = N'EHC2026111600001';  -- application OR lease reference
DECLARE @Fix    bit = 0;   -- 1 = move a notice stuck on an old lease row to the current row
DECLARE @Commit bit = 0;   -- 0 = dry run (rollback), 1 = commit

DECLARE @AppId int, @StCso int, @StDeact int, @StSubmitted int, @RespTerm int, @RespVal int;
SELECT TOP 1 @AppId = a.Id FROM dbo.PropertyLeaseApplications a
 WHERE a.ApplicationReferenceNumber = @Ref
    OR a.Id IN (SELECT PropertyLeaseApplicationId FROM dbo.LeaseDetails WHERE LeaseReferenceNo = @Ref)
 ORDER BY a.Id DESC;
SELECT TOP 1 @StCso       = Id FROM dbo.Status WHERE [Key] = N's_awaiting_cso_termination_review' ORDER BY Id;
SELECT TOP 1 @StDeact     = Id FROM dbo.Status WHERE [Key] = N's_deactivated_lease' ORDER BY Id;
SELECT TOP 1 @StSubmitted = Id FROM dbo.Status WHERE [Key] = N's_rcs_Submitted' ORDER BY Id;
SELECT TOP 1 @RespTerm    = Id FROM dbo.ResponsibilityTypes WHERE [Key] = N'r_terminations' ORDER BY Id;
SELECT TOP 1 @RespVal     = Id FROM dbo.ResponsibilityTypes WHERE [Key] = N'r_termination_validation' ORDER BY Id;

IF @AppId IS NULL BEGIN PRINT 'No application/lease found for ' + @Ref; RETURN; END
IF @StCso IS NULL PRINT 'MISSING status s_awaiting_cso_termination_review: run PLM-Upgrade-ALL.sql first.';

PRINT '=== 1. Application ===';
SELECT a.Id AS AppId, a.ApplicationReferenceNumber, a.CustomerId, a.ServeNoticeDate, s.[Key] AS AppStatus
FROM dbo.PropertyLeaseApplications a LEFT JOIN dbo.Status s ON s.Id = a.StatusId WHERE a.Id = @AppId;

PRINT '=== 2. Lease rows (the CSO screen only shows IsNew = 1, IsDeleted = 0) ===';
SELECT l.Id AS LeaseId, l.LeaseReferenceNo, l.IsNew, l.IsActive, l.IsDeleted, l.NoticeDate,
       s.[Key] AS LeaseStatus, s.Name AS LeaseStatusName, l.StartDate, l.EndDate, l.CreatedDateTime,
       CASE WHEN l.IsDeleted = 0 AND l.IsNew = 1 AND l.StatusId = @StCso THEN 'YES' ELSE 'no' END AS OnCsoScreen
FROM dbo.LeaseDetails l LEFT JOIN dbo.Status s ON s.Id = l.StatusId
WHERE l.PropertyLeaseApplicationId = @AppId ORDER BY l.Id;

PRINT '=== 3. Termination records ===';
SELECT t.Id, t.LeaseDetailsId, t.TerminationDate, t.ReasonForTermination, t.TerminationReferenceNumber, t.CreatedDateTime, t.IsDeleted
FROM dbo.LeaseTerminations t WHERE t.PropertyLeaseApplicationId = @AppId ORDER BY t.Id;

PRINT '=== 4. Termination work queue rows (who it was routed to) ===';
SELECT q.Id, rt.[Key] AS Responsibility, q.LeaseDetailsId, q.ClerkId AS ClerkCustomerId,
       su.FirstName + ' ' + su.LastName AS ClerkName, qs.[Key] AS QueueStatus, q.CreatedDateTime
FROM dbo.RoundRobinQueues q
JOIN dbo.ResponsibilityTypes rt ON rt.Id = q.ResponsibilityTypeId
LEFT JOIN dbo.Status qs ON qs.Id = q.StatusId
LEFT JOIN dbo.Customers c ON c.Id = q.ClerkId
LEFT JOIN dbo.SystemUsers su ON su.Id = c.SystemUserId
WHERE q.PropertyLeaseApplicationId = @AppId AND q.ResponsibilityTypeId IN (@RespTerm, @RespVal)
ORDER BY q.Id;

PRINT '=== 5. Last history entries ===';
SELECT TOP 10 h.CreatedDateTime, h.AuditAction FROM dbo.PLMApplicationHistortyLogs h
WHERE h.PropertyLeaseApplicationId = @AppId ORDER BY h.Id DESC;

PRINT '=== 6. Diagnosis ===';
DECLARE @OldRow int, @CurRow int;
SELECT TOP 1 @OldRow = Id FROM dbo.LeaseDetails
 WHERE PropertyLeaseApplicationId = @AppId AND IsDeleted = 0 AND IsNew = 0 AND (NoticeDate IS NOT NULL OR StatusId = @StCso) ORDER BY Id DESC;
SELECT TOP 1 @CurRow = Id FROM dbo.LeaseDetails
 WHERE PropertyLeaseApplicationId = @AppId AND IsDeleted = 0 AND IsNew = 1 AND StatusId <> ISNULL(@StDeact, -1) ORDER BY Id DESC;

IF EXISTS (SELECT 1 FROM dbo.LeaseDetails WHERE PropertyLeaseApplicationId = @AppId AND IsDeleted = 0 AND IsNew = 1 AND StatusId = @StCso)
    PRINT 'OK: the notice is waiting for the CSO. Log in as a Client Services Officer (e.g. zanelem) > Termination > Terminations > Review Notice.';
ELSE IF @OldRow IS NOT NULL
    PRINT 'CAUSE: the notice was served on an OLD lease row (LeaseId ' + CAST(@OldRow AS varchar) + ', IsNew = 0). The CSO screen only shows the current row'
        + ISNULL(' (LeaseId ' + CAST(@CurRow AS varchar) + ')', '') + '. Run again with @Fix = 1 to move it.';
ELSE IF NOT EXISTS (SELECT 1 FROM dbo.LeaseDetails WHERE PropertyLeaseApplicationId = @AppId AND NoticeDate IS NOT NULL)
    PRINT 'CAUSE: the notice was never saved (no NoticeDate on any lease row). The tenant submit failed: check the message on the tenant''s Serve Notice screen and the error log, then submit again.';
ELSE
    PRINT 'The notice is past the CSO step already (see LeaseStatus above): it sits with the next user, not on the CSO screen.';

/* ---------------- Part 2: move a stuck notice to the current lease row ---------------- */
IF @Fix = 1
BEGIN
    IF @OldRow IS NULL OR @CurRow IS NULL OR @OldRow = @CurRow
    BEGIN PRINT 'Nothing to fix (no old row with a notice, or no current row).'; RETURN; END
    IF EXISTS (SELECT 1 FROM dbo.LeaseDetails WHERE Id = @CurRow AND NoticeDate IS NOT NULL)
    BEGIN PRINT 'Current row already has a notice: not touching it.'; RETURN; END

    IF OBJECT_ID('dbo.Bak_TermMove_LeaseDetails') IS NULL
        SELECT TOP 0 Id, StatusId, NoticeDate, GETDATE() AS BackedUpAt INTO dbo.Bak_TermMove_LeaseDetails FROM dbo.LeaseDetails;

    BEGIN TRAN;
        INSERT dbo.Bak_TermMove_LeaseDetails SELECT Id, StatusId, NoticeDate, GETDATE() FROM dbo.LeaseDetails WHERE Id IN (@OldRow, @CurRow);

        DECLARE @Notice datetime, @OldStatus int;
        SELECT @Notice = NoticeDate, @OldStatus = StatusId FROM dbo.LeaseDetails WHERE Id = @OldRow;

        UPDATE dbo.LeaseDetails SET NoticeDate = @Notice, StatusId = @StCso WHERE Id = @CurRow;
        UPDATE dbo.LeaseDetails SET NoticeDate = NULL, StatusId = ISNULL(@StDeact, StatusId) WHERE Id = @OldRow;
        UPDATE dbo.LeaseTerminations SET LeaseDetailsId = @CurRow,
               LeaseReferenceNumber = (SELECT LeaseReferenceNo FROM dbo.LeaseDetails WHERE Id = @CurRow)
         WHERE PropertyLeaseApplicationId = @AppId AND LeaseDetailsId = @OldRow;
        UPDATE dbo.RoundRobinQueues SET LeaseDetailsId = @CurRow
         WHERE PropertyLeaseApplicationId = @AppId AND LeaseDetailsId = @OldRow AND ResponsibilityTypeId = @RespTerm;

        PRINT 'After:';
        SELECT l.Id, l.IsNew, l.NoticeDate, s.[Key] AS LeaseStatus FROM dbo.LeaseDetails l LEFT JOIN dbo.Status s ON s.Id = l.StatusId
         WHERE l.Id IN (@OldRow, @CurRow);
    IF @Commit = 1 BEGIN COMMIT; PRINT 'COMMITTED. The CSO now sees it under Termination > Terminations.'; END
    ELSE BEGIN ROLLBACK; PRINT 'DRY RUN rolled back. Set @Commit = 1 to apply.'; END
END

/* ROLLBACK of a committed fix (run by hand):
UPDATE l SET l.StatusId = b.StatusId, l.NoticeDate = b.NoticeDate
FROM dbo.LeaseDetails l JOIN (SELECT Id, StatusId, NoticeDate, ROW_NUMBER() OVER (PARTITION BY Id ORDER BY BackedUpAt DESC) rn
                             FROM dbo.Bak_TermMove_LeaseDetails) b ON b.Id = l.Id AND b.rn = 1;
*/
