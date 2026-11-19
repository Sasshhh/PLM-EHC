/* =====================================================================
   49-EHC-Reset-To-Refund-Start.sql
   Put a moved-out application back to the START of the refund:
   "CSO confirmed tenant Vacated", waiting for the Client Services
   Supervisor on Terminations > Terminated Leases (Select > Process Refund).

   What it does for the given application / lease reference:
     - latest lease row + application  -> s_awaiting_deposit_refund_processing
     - termination record: clears every refund field the supervisor, RM and
       CEO captured (maintenance, deductions, nett, recommendation, reasons,
       official numbers, RM support, CEO response/signature/date)
     - closes (archives) every open refund task: deposit refund processing,
       refund response (RM), refund authorisation (CEO), old CSO start refund
     - opens ONE new Deposit Refund Processing task for the supervisor:
       @SupUser if given, else the complex's Housing Supervisor
       (PreferredComplexAreas.HousingSuperId), else AppSettings
       u_housing_super_visor (same order as the site)

   Not changed: uploaded documents (proof of banking stays; a new upload is
   added next to it), and the tenant's customer status. If the CEO already
   approved, the tenant is "Former Tenant"; that does not block the refund
   screens. Set @CustomerStatusKey to change it back if you want.

   @Commit = 0 (default) previews and rolls back. @Commit = 1 applies.
   ===================================================================== */
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Ref               nvarchar(50)  = N'EHC2026111600001'; -- application OR lease reference
DECLARE @SupUser           nvarchar(100) = NULL;                -- e.g. N'tshepangm'; NULL = complex supervisor / AppSettings
DECLARE @CustomerStatusKey nvarchar(100) = NULL;                -- optional: Status.[Key] to put the tenant's customer status on
DECLARE @Commit            bit = 0;

DECLARE @AppId int, @LeaseId int, @CustomerId int, @Processing int, @Submitted int, @Archived int,
        @RtProc int, @RtResponse int, @RtAuth int, @RtOldStart int, @SupCust int, @SupSource nvarchar(60), @CustStatus int;

SELECT TOP 1 @AppId = a.Id, @CustomerId = a.CustomerId FROM dbo.PropertyLeaseApplications a
 WHERE a.ApplicationReferenceNumber = @Ref
    OR a.Id IN (SELECT PropertyLeaseApplicationId FROM dbo.LeaseDetails WHERE LeaseReferenceNo = @Ref)
 ORDER BY a.Id DESC;
IF @AppId IS NULL BEGIN PRINT 'No application/lease found for ' + @Ref; RETURN; END

SELECT TOP 1 @LeaseId = Id FROM dbo.LeaseDetails WHERE PropertyLeaseApplicationId = @AppId AND IsDeleted = 0 ORDER BY Id DESC;

SELECT TOP 1 @Processing = Id FROM dbo.Status WHERE [Key] = N's_awaiting_deposit_refund_processing' ORDER BY Id;
SELECT TOP 1 @Submitted  = Id FROM dbo.Status WHERE [Key] = N's_rcs_Submitted' ORDER BY Id;
SELECT TOP 1 @Archived   = Id FROM dbo.Status WHERE [Key] = N's_rcs_Archived'  ORDER BY Id;
SELECT TOP 1 @RtProc     = Id FROM dbo.ResponsibilityTypes WHERE [Key] = N'r_deposit_refund_processing' ORDER BY Id;
SELECT TOP 1 @RtResponse = Id FROM dbo.ResponsibilityTypes WHERE [Key] = N'r_awaiting_deposit_refund_response' ORDER BY Id;
SELECT TOP 1 @RtAuth     = Id FROM dbo.ResponsibilityTypes WHERE [Key] = N'r_refund_authorisation' ORDER BY Id;
SELECT TOP 1 @RtOldStart = Id FROM dbo.ResponsibilityTypes WHERE [Key] = N'r_deposit_refund' ORDER BY Id;

IF @Processing IS NULL OR @RtProc IS NULL
BEGIN PRINT 'Run 47-EHC-Release-20261119.sql first (refund processing status / task type missing).'; RETURN; END
IF @LeaseId IS NULL BEGIN PRINT 'The application has no lease row.'; RETURN; END

-- Supervisor: @SupUser > complex Housing Supervisor > AppSettings u_housing_super_visor
IF @SupUser IS NOT NULL
BEGIN
    SELECT TOP 1 @SupCust = c.Id FROM dbo.Customers c JOIN dbo.SystemUsers su ON su.Id = c.SystemUserId WHERE su.UserName = @SupUser ORDER BY c.Id;
    SET @SupSource = N'@SupUser';
END
IF @SupCust IS NULL
BEGIN
    SELECT TOP 1 @SupCust = pca.HousingSuperId
    FROM dbo.ApplicantUnits au
    JOIN dbo.MatchedUnits mu ON mu.Id = au.MatchedID
    LEFT JOIN dbo.ApplicationAllocatedProperties aap ON aap.Id = mu.ApplicationAllocatedPropertyId
    LEFT JOIN dbo.UnitsEkurhuleniHousingCompanies u ON u.Id = mu.UnitsEkurhuleniHousingCompanyId
    JOIN dbo.PreferredComplexAreas pca ON pca.Id = CASE WHEN mu.ApplicationAllocatedPropertyId IS NOT NULL THEN aap.OfferedComplexId ELSE u.PreferredComplexAreaId END
    WHERE au.PropertyLeaseApplicationId = @AppId AND pca.HousingSuperId IS NOT NULL
    ORDER BY au.Id;
    IF @SupCust IS NOT NULL SET @SupSource = N'complex Housing Supervisor';
END
IF @SupCust IS NULL
BEGIN
    SELECT TOP 1 @SupCust = TRY_CAST([Value] AS int) FROM dbo.AppSettings WHERE [Key] = N'u_housing_super_visor' ORDER BY Id;
    SET @SupSource = N'AppSettings u_housing_super_visor';
END
IF @SupCust IS NULL OR @SupCust = 0 BEGIN PRINT 'Cannot find the supervisor: set @SupUser.'; RETURN; END

IF @CustomerStatusKey IS NOT NULL
BEGIN
    SELECT TOP 1 @CustStatus = Id FROM dbo.Status WHERE [Key] = @CustomerStatusKey ORDER BY Id;
    IF @CustStatus IS NULL BEGIN PRINT 'Status key not found: ' + @CustomerStatusKey; RETURN; END
END

PRINT '=== BEFORE ===';
SELECT 'Application' AS Item, a.Id, a.ApplicationReferenceNumber AS Ref, s.[Key] AS StatusKey, s.Name AS StatusName
FROM dbo.PropertyLeaseApplications a LEFT JOIN dbo.Status s ON s.Id = a.StatusId WHERE a.Id = @AppId
UNION ALL
SELECT 'Lease (latest row)', l.Id, l.LeaseReferenceNo, s.[Key], s.Name
FROM dbo.LeaseDetails l LEFT JOIN dbo.Status s ON s.Id = l.StatusId WHERE l.Id = @LeaseId
UNION ALL
SELECT 'Tenant customer', c.Id, NULL, s.[Key], s.Name
FROM dbo.Customers c LEFT JOIN dbo.Status s ON s.Id = c.StatusId WHERE c.Id = @CustomerId;

SELECT t.Id AS TerminationId, t.MaintenanceCost, t.ApprovedDeductions, t.NettRefundAmount, t.DepositRefundRecommended,
       t.FinancialOfficerReason, t.RevenueManagerRefundSupport, t.RevenueManagerRefundReason, t.CEORefundResponse, t.CEORefundReason, t.CEORefundSignDate
FROM dbo.LeaseTerminations t WHERE t.PropertyLeaseApplicationId = @AppId AND t.IsDeleted = 0 ORDER BY t.Id;

BEGIN TRAN;
    UPDATE dbo.LeaseDetails SET StatusId = @Processing, ModifiedDateTime = GETDATE() WHERE Id = @LeaseId;
    UPDATE dbo.PropertyLeaseApplications SET StatusId = @Processing, ModifiedDateTime = GETDATE() WHERE Id = @AppId;

    UPDATE dbo.LeaseTerminations
       SET MaintenanceCost = NULL, ApprovedDeductions = NULL, NettRefundAmount = NULL, DepositRefundRecommended = NULL,
           FinancialOfficerOfficialNumber = NULL, FinancialOfficerReason = NULL,
           RevenueManagerRefundSupport = NULL, RevenueManagerRefundOfficialNumber = NULL, RevenueManagerRefundReason = NULL,
           CEORefundResponse = NULL, CEORefundOfficialNumber = NULL, CEORefundSignature = NULL, CEORefundSignDate = NULL, CEORefundReason = NULL,
           ModifiedDateTime = GETDATE()
     WHERE PropertyLeaseApplicationId = @AppId AND IsDeleted = 0;

    UPDATE dbo.RoundRobinQueues SET StatusId = @Archived, EndTaskDateTime = GETDATE()
     WHERE PropertyLeaseApplicationId = @AppId AND StatusId <> @Archived
       AND ResponsibilityTypeId IN (@RtProc, ISNULL(@RtResponse, -1), ISNULL(@RtAuth, -1), ISNULL(@RtOldStart, -1));

    INSERT dbo.RoundRobinQueues (PropertyLeaseApplicationId, LeaseDetailsId, ResponsibilityTypeId, ClerkId, StatusId,
                                 CurrentTaskDateTime, CreatedDateTime, IsActive, IsDeleted)
    VALUES (@AppId, @LeaseId, @RtProc, @SupCust, @Submitted, GETDATE(), GETDATE(), 1, 0);

    IF @CustStatus IS NOT NULL
        UPDATE dbo.Customers SET StatusId = @CustStatus, ModifiedDateTime = GETDATE() WHERE Id = @CustomerId;

    PRINT '=== AFTER ===';
    SELECT 'Application' AS Item, a.Id, s.[Key] AS StatusKey, s.Name AS StatusName
    FROM dbo.PropertyLeaseApplications a LEFT JOIN dbo.Status s ON s.Id = a.StatusId WHERE a.Id = @AppId
    UNION ALL
    SELECT 'Lease (latest row)', l.Id, s.[Key], s.Name
    FROM dbo.LeaseDetails l LEFT JOIN dbo.Status s ON s.Id = l.StatusId WHERE l.Id = @LeaseId
    UNION ALL
    SELECT 'Tenant customer', c.Id, s.[Key], s.Name
    FROM dbo.Customers c LEFT JOIN dbo.Status s ON s.Id = c.StatusId WHERE c.Id = @CustomerId;

    SELECT q.Id, rt.[Key] AS Responsibility, q.LeaseDetailsId, q.ClerkId, su.UserName AS ClerkUser, qs.[Key] AS QueueStatus, q.CreatedDateTime
    FROM dbo.RoundRobinQueues q
    JOIN dbo.ResponsibilityTypes rt ON rt.Id = q.ResponsibilityTypeId
    JOIN dbo.Status qs ON qs.Id = q.StatusId
    LEFT JOIN dbo.Customers c ON c.Id = q.ClerkId
    LEFT JOIN dbo.SystemUsers su ON su.Id = c.SystemUserId
    WHERE q.PropertyLeaseApplicationId = @AppId AND q.ResponsibilityTypeId IN (@RtProc, ISNULL(@RtResponse, -1), ISNULL(@RtAuth, -1), ISNULL(@RtOldStart, -1))
      AND (q.StatusId = @Submitted OR q.EndTaskDateTime >= DATEADD(MINUTE, -1, GETDATE()))   -- open now, or closed by this run
    ORDER BY q.Id;

    SELECT t.Id AS TerminationId, t.MaintenanceCost, t.ApprovedDeductions, t.NettRefundAmount, t.DepositRefundRecommended,
           t.FinancialOfficerReason, t.RevenueManagerRefundSupport, t.RevenueManagerRefundReason, t.CEORefundResponse, t.CEORefundReason, t.CEORefundSignDate
    FROM dbo.LeaseTerminations t WHERE t.PropertyLeaseApplicationId = @AppId AND t.IsDeleted = 0 ORDER BY t.Id;

    PRINT 'Supervisor task goes to customer ' + CAST(@SupCust AS varchar) + ' (' + @SupSource + ').';
IF @Commit = 1 BEGIN COMMIT; PRINT 'COMMITTED. The supervisor sees it under Terminations > Terminated Leases (log out/in if already signed in).'; END
ELSE BEGIN ROLLBACK; PRINT 'PREVIEW only, rolled back. Set @Commit = 1 to apply.'; END
