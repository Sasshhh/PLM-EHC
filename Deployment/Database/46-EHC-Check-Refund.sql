/* =====================================================================
   46-EHC-Check-Refund.sql
   Why is a moved-out lease not under Terminations > Terminated Leases?

   Refund flow after move-out (UC031 / UC032, PR #8):
     1. CSO confirms move-out "Vacated" (Terminations > Move-Out Confirmation)
          lease -> s_awaiting_deposit_refund_processing, task
          r_deposit_refund_processing -> Client Services Supervisor of the
          complex (PreferredComplexAreas.HousingSuperId, e.g. tshepangm) or
          AppSettings u_housing_super_visor. (No CSO "Start Refund" step.)
     2. Supervisor: Terminations > Terminated Leases > Select > Official
          Number > Deductions > Authorise > recommend / not recommend
          (+reason), proof of banking, Submit
          lease -> s_awaiting_refund_response, task
          r_awaiting_deposit_refund_response -> r_revenue_manager (madimetjak).
     3. Revenue Manager: Terminations > Terminated Leases > Select > Authorise
          Supported -> s_awaiting_refund_authorisation, task
            r_refund_authorisation -> s_ceo (SolarTest05).
          Not Supported (+reason) -> back to step 2 (supervisor).
     4. CEO: Terminations > Terminated Leases > Select > Authorise >
          Approve Full / Approve Partial / No Refund (+reason) + signature
          -> l_terminated_lease, tenant = former tenant.
          Reject (+reason) -> back to step 3 (Revenue Manager).

   A lease left on s_applicant_vacated_unit by the older build (CSO Start
   Refund) is moved to step 1's status by @Fix = 1.

   Part 1 (always): report + diagnosis. Part 2 (@Fix = 1): give the step the
   lease is on an open queue row for the right user, so it shows up.
   @Commit = 0 previews and rolls back.
   ===================================================================== */
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Ref       nvarchar(50)  = N'EHC2026111600001';  -- application OR lease reference
DECLARE @SupUser   nvarchar(100) = N'tshepangm';         -- Client Services Supervisor (step 2)
DECLARE @Fix       bit = 0;
DECLARE @Commit    bit = 0;

DECLARE @AppId int, @LeaseId int, @LeaseStatus nvarchar(100), @Submitted int, @Archived int,
        @RtRefund int, @RtResponse int, @RtAuth int, @RtVacate int,
        @SupCust int, @Processing int, @MoveOld bit = 0, @RtProc int, @RmCust int, @CeoCust int, @NeedRt int, @NeedClerk int, @Step nvarchar(200);

SELECT TOP 1 @AppId = a.Id FROM dbo.PropertyLeaseApplications a
 WHERE a.ApplicationReferenceNumber = @Ref
    OR a.Id IN (SELECT PropertyLeaseApplicationId FROM dbo.LeaseDetails WHERE LeaseReferenceNo = @Ref)
 ORDER BY a.Id DESC;
IF @AppId IS NULL BEGIN PRINT 'No application/lease found for ' + @Ref; RETURN; END

-- the refund queues use the LATEST lease row of the application
SELECT TOP 1 @LeaseId = l.Id, @LeaseStatus = s.[Key]
FROM dbo.LeaseDetails l LEFT JOIN dbo.Status s ON s.Id = l.StatusId
WHERE l.PropertyLeaseApplicationId = @AppId AND l.IsDeleted = 0 ORDER BY l.Id DESC;

SELECT TOP 1 @Submitted = Id FROM dbo.Status WHERE [Key] = N's_rcs_Submitted' ORDER BY Id;
SELECT TOP 1 @Archived  = Id FROM dbo.Status WHERE [Key] = N's_rcs_Archived'  ORDER BY Id;
SELECT TOP 1 @RtRefund   = Id FROM dbo.ResponsibilityTypes WHERE [Key] = N'r_deposit_refund' ORDER BY Id;
SELECT TOP 1 @RtResponse = Id FROM dbo.ResponsibilityTypes WHERE [Key] = N'r_awaiting_deposit_refund_response' ORDER BY Id;
SELECT TOP 1 @RtAuth     = Id FROM dbo.ResponsibilityTypes WHERE [Key] = N'r_refund_authorisation' ORDER BY Id;
SELECT TOP 1 @RtProc     = Id FROM dbo.ResponsibilityTypes WHERE [Key] = N'r_deposit_refund_processing' ORDER BY Id;
SELECT TOP 1 @SupCust = c.Id FROM dbo.Customers c JOIN dbo.SystemUsers su ON su.Id = c.SystemUserId WHERE su.UserName = @SupUser ORDER BY c.Id;
IF @RtProc IS NULL OR @Processing IS NULL PRINT 'NOTE: s_awaiting_deposit_refund_processing / r_deposit_refund_processing missing: run PLM-Upgrade-ALL.sql (the new site also creates them on first use of Terminated Leases).';
SELECT TOP 1 @RtVacate   = Id FROM dbo.ResponsibilityTypes WHERE [Key] = N'r_vacating_confirmation' ORDER BY Id;
SELECT TOP 1 @Processing = Id FROM dbo.Status WHERE [Key] = N's_awaiting_deposit_refund_processing' ORDER BY Id;
SELECT TOP 1 @RmCust  = TRY_CAST([Value] AS int) FROM dbo.AppSettings WHERE [Key] = N'r_revenue_manager' ORDER BY Id;
SELECT TOP 1 @CeoCust = TRY_CAST([Value] AS int) FROM dbo.AppSettings WHERE [Key] = N's_ceo' ORDER BY Id;

PRINT '=== 1. Lease rows (refund screens use the latest row: LeaseId ' + CAST(@LeaseId AS varchar) + ') ===';
SELECT l.Id AS LeaseId, l.LeaseReferenceNo, l.IsNew, l.IsDeleted, s.[Key] AS LeaseStatus, s.Name AS LeaseStatusName,
       l.DepositeAmount, l.ModifiedDateTime
FROM dbo.LeaseDetails l LEFT JOIN dbo.Status s ON s.Id = l.StatusId
WHERE l.PropertyLeaseApplicationId = @AppId ORDER BY l.Id;

PRINT '=== 2. Termination record (refund amounts) ===';
SELECT t.Id, t.LeaseDetailsId, t.TerminationDate, t.MaintenanceCost, t.ApprovedDeductions, t.NettRefundAmount, t.DepositRefundRecommended
FROM dbo.LeaseTerminations t WHERE t.PropertyLeaseApplicationId = @AppId AND t.IsDeleted = 0 ORDER BY t.Id;

PRINT '=== 3. Move-out and refund queue rows (who sees it) ===';
SELECT q.Id, rt.[Key] AS Responsibility, q.LeaseDetailsId, q.ClerkId AS ClerkCustomerId, su.UserName AS ClerkUser,
       qs.[Key] AS QueueStatus, q.CreatedDateTime,
       CASE WHEN q.StatusId = @Submitted THEN 'OPEN' ELSE 'closed' END AS OpenOrClosed
FROM dbo.RoundRobinQueues q
JOIN dbo.ResponsibilityTypes rt ON rt.Id = q.ResponsibilityTypeId
LEFT JOIN dbo.Status qs ON qs.Id = q.StatusId
LEFT JOIN dbo.Customers c ON c.Id = q.ClerkId
LEFT JOIN dbo.SystemUsers su ON su.Id = c.SystemUserId
WHERE q.PropertyLeaseApplicationId = @AppId AND q.ResponsibilityTypeId IN (@RtVacate, @RtRefund, @RtProc, @RtResponse, @RtAuth)
ORDER BY q.Id;

PRINT '=== 4. Refund users from AppSettings ===';
SELECT 'Client Services Supervisor (@SupUser)' AS [Role], @SupCust AS CustomerId, @SupUser AS UserName
UNION ALL SELECT 'Revenue Manager r_revenue_manager', @RmCust, (SELECT TOP 1 su.UserName FROM dbo.Customers c JOIN dbo.SystemUsers su ON su.Id = c.SystemUserId WHERE c.Id = @RmCust)
UNION ALL SELECT 'CEO s_ceo', @CeoCust, (SELECT TOP 1 su.UserName FROM dbo.Customers c JOIN dbo.SystemUsers su ON su.Id = c.SystemUserId WHERE c.Id = @CeoCust);

PRINT '=== 5. Diagnosis ===';
PRINT 'Lease ' + CAST(@LeaseId AS varchar) + ' is on ' + ISNULL(@LeaseStatus, '?');
IF @LeaseStatus IN (N's_awaiting_vacating_confirmation', N's_applicant_not_vacated_unit')
    PRINT 'Move-out is NOT confirmed as Vacated yet. CSO: Termination > Move-Out Confirmation > Vacated. Only then does the refund start.';
ELSE IF @LeaseStatus = N's_applicant_vacated_unit'
    BEGIN
        PRINT 'CAUSE: this lease was vacated on the older build, which waited for a CSO "Start Refund". UC031 sends it straight to the Client Services Supervisor. Run with @Fix = 1 to move it to Awaiting Deposit Refund Processing.';
        SET @MoveOld = 1; SET @NeedRt = @RtProc; SET @NeedClerk = @SupCust; SET @Step = 'Client Services Supervisor ' + @SupUser + ': Terminations > Terminated Leases';
    END
ELSE IF @LeaseStatus = N's_awaiting_deposit_refund_processing'
    BEGIN SET @NeedRt = @RtProc; SET @NeedClerk = @SupCust; SET @Step = 'Client Services Supervisor ' + @SupUser + ': Terminations > Terminated Leases'; END
ELSE IF @LeaseStatus = N's_awaiting_refund_response'
    BEGIN SET @NeedRt = @RtResponse; SET @NeedClerk = @RmCust; SET @Step = 'Revenue Manager: Terminations > Terminated Leases'; END
ELSE IF @LeaseStatus = N's_awaiting_refund_authorisation'
    BEGIN SET @NeedRt = @RtAuth; SET @NeedClerk = @CeoCust; SET @Step = 'CEO: Terminations > Terminated Leases'; END
ELSE IF @LeaseStatus = N'l_terminated_lease'
    PRINT 'Refund is finished: the lease is terminated.';
ELSE
    PRINT 'The lease is not at a move-out/refund step yet (see LeaseStatus). Finish the termination/eviction steps first.';

IF @NeedRt IS NOT NULL AND @MoveOld = 0
BEGIN
    IF EXISTS (SELECT 1 FROM dbo.RoundRobinQueues WHERE PropertyLeaseApplicationId = @AppId AND LeaseDetailsId = @LeaseId
                 AND ResponsibilityTypeId = @NeedRt AND StatusId = @Submitted AND ClerkId = @NeedClerk)
        PRINT 'OK: it should show for ' + @Step + '. Log in as that user.';
    ELSE IF EXISTS (SELECT 1 FROM dbo.RoundRobinQueues WHERE PropertyLeaseApplicationId = @AppId AND ResponsibilityTypeId = @NeedRt AND StatusId = @Submitted)
        PRINT 'CAUSE: the open task belongs to ANOTHER user or another lease row (see grid 3, OPEN row). Log in as that ClerkUser, or run with @Fix = 1 to give it to ' + @Step + '.';
    ELSE
        PRINT 'CAUSE: there is no open task for ' + @Step + ' (it was never created or was closed). Run with @Fix = 1 to create it.';
END

/* ---------------- Part 2: open the right task for the right user ---------------- */
IF @Fix = 1
BEGIN
    IF @NeedRt IS NULL BEGIN PRINT 'Nothing to fix at this status.'; RETURN; END
    IF @NeedClerk IS NULL BEGIN PRINT 'Cannot fix: user not found (check @SupUser / AppSettings above).'; RETURN; END
    IF @MoveOld = 1 AND (@Processing IS NULL OR @RtProc IS NULL)
    BEGIN PRINT 'Cannot fix: run PLM-Upgrade-ALL.sql first (new refund status / task type missing).'; RETURN; END
    IF @MoveOld = 0 AND EXISTS (SELECT 1 FROM dbo.RoundRobinQueues WHERE PropertyLeaseApplicationId = @AppId AND LeaseDetailsId = @LeaseId
                 AND ResponsibilityTypeId = @NeedRt AND StatusId = @Submitted AND ClerkId = @NeedClerk)
    BEGIN PRINT 'Already OK, nothing changed.'; RETURN; END

    BEGIN TRAN;
        IF @MoveOld = 1
        BEGIN
            -- old build: close the CSO Start Refund task and put the lease on Awaiting Deposit Refund Processing
            UPDATE dbo.RoundRobinQueues SET StatusId = @Archived, EndTaskDateTime = GETDATE()
             WHERE PropertyLeaseApplicationId = @AppId AND ResponsibilityTypeId = @RtRefund AND StatusId = @Submitted;
            UPDATE dbo.LeaseDetails SET StatusId = @Processing, ModifiedDateTime = GETDATE() WHERE Id = @LeaseId;
        END
        -- close other open rows of this step (wrong user / old lease row)
        UPDATE dbo.RoundRobinQueues SET StatusId = @Archived, EndTaskDateTime = GETDATE()
         WHERE PropertyLeaseApplicationId = @AppId AND ResponsibilityTypeId = @NeedRt AND StatusId = @Submitted;
        INSERT dbo.RoundRobinQueues (PropertyLeaseApplicationId, LeaseDetailsId, ResponsibilityTypeId, ClerkId, StatusId,
                                     CurrentTaskDateTime, CreatedDateTime, IsActive, IsDeleted)
        VALUES (@AppId, @LeaseId, @NeedRt, @NeedClerk, @Submitted, GETDATE(), GETDATE(), 1, 0);
        SELECT q.Id, q.LeaseDetailsId, q.ClerkId, s.[Key] AS QueueStatus FROM dbo.RoundRobinQueues q JOIN dbo.Status s ON s.Id = q.StatusId
         WHERE q.PropertyLeaseApplicationId = @AppId AND q.ResponsibilityTypeId = @NeedRt ORDER BY q.Id;
    IF @Commit = 1 BEGIN COMMIT; PRINT 'COMMITTED. It now shows for ' + @Step + '.'; END
    ELSE BEGIN ROLLBACK; PRINT 'DRY RUN rolled back. Set @Commit = 1 to apply.'; END
END
