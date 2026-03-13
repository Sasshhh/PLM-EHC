-- ============================================================
-- TC-02 — Tenant Training: Schedule Training Slot
-- ============================================================
-- Purpose : Insert a PropertyLeaseApplication at status
--           "Payment Validated and Approved" (Id 122) so the
--           customer can see it on MyScheduledTraining and
--           click "Schedule Training".
--
-- Login as : Sashen Moodley  (Username: Sash38, Customer Id 45)
-- Start at : http://localhost:3450/TenantTraining/MyScheduledTraining
--
-- What it inserts:
--   1× PropertyLeaseApplications  — status 122 (AssessmentFeePaymentApproved)
--   1× MonthlyIncomes             — R18,000 gross, R13,500 net
--   1× MonthlyExpenses            — zeroed
--   5× PLMApplicationHistortyLogs — history up to fee approval
--
-- After scheduling via the UI the system will:
--   • Insert a TenantTraining record
--   • Update status to 3323 (Awaiting Online Training)
--   • Send email/SMS (mocked in dev)
-- ============================================================

SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRANSACTION

    -- --------------------------------------------------------
    -- LOOKUPS  (do not hard-code — resolve at runtime)
    -- --------------------------------------------------------
    DECLARE @StatusAssessmentApproved  INT = 122   -- s_rcs_assessment_payment_approved
    DECLARE @StatusDocPending          INT = 46    -- s_document_pending
    DECLARE @StatusSubmitted           INT = 99    -- s_rcs_Submitted (RRQ status)
    DECLARE @PurchaserTypeId           INT = 1     -- Individual Application
    DECLARE @PreferredComplexAreaId    INT = 24    -- Airport Park
    DECLARE @CustomerId                INT = 45    -- Sashen Moodley  (login: Sash38)
    DECLARE @CreatedBySystemUserId     INT = 45    -- same user owns the record
    DECLARE @CSOCustomerId             INT = 190   -- AshKay (Client Services Officer)

    -- Resolve LocationType / ReferenceType for document shell rows
    DECLARE @LocationTypeId  INT
    DECLARE @ReferenceTypeId INT
    SELECT TOP 1 @LocationTypeId  = Id FROM LocationTypes  WHERE IsDeleted = 0 AND IsActive = 1 ORDER BY Id
    SELECT TOP 1 @ReferenceTypeId = Id FROM ReferenceTypes WHERE IsDeleted = 0 AND IsActive = 1 ORDER BY Id

    -- --------------------------------------------------------
    -- STEP 1: Build a unique reference number
    -- --------------------------------------------------------
    DECLARE @RefNumber NVARCHAR(50)
    SET @RefNumber = 'EHC-TC02-' + CONVERT(VARCHAR, GETDATE(), 112) + RIGHT('000' + CAST(DATEPART(ms, GETDATE()) AS VARCHAR), 3)

    -- --------------------------------------------------------
    -- STEP 2: Insert the PropertyLeaseApplication
    -- --------------------------------------------------------
    INSERT INTO PropertyLeaseApplications (
        ApplicationReferenceNumber,
        CustomerId,
        SystemUserId,
        StatusId,
        PurchaserTypeId,
        PreferredComplexAreaId,
        FirstName, LastName,
        PurEmail,
        DOB,
        IDNo,
        CellNo,
        HumanEHCOptionsId,
        IncomeSourceId,
        SecAppIncomeSourceId,
        IdentificationTypeId,
        TitleTypeId,
        GrossIncome,
        NetIncome,
        TotalCombinedIncome,
        SecondApplicant,
        IsMigrated,
        IsFullyMigrated,
        PreUnitInspectionCompleted,
        PreUnitMaintenanceRequired,
        ExitInspectionCompleted,
        ExitUnitMaintenanceRequired,
        IsActive, IsDeleted, IsLocked,
        CreatedBySystemUserId, CreatedDateTime,
        ModifiedBySystemUserId, ModifiedDateTime
    )
    VALUES (
        @RefNumber,
        @CustomerId,
        45,     -- SystemUserId (matches Customer 45 / Sash38)
        @StatusAssessmentApproved,
        @PurchaserTypeId,
        @PreferredComplexAreaId,
        'Sashen', 'Moodley',
        'sashen.moodley@test.com',
        '1990-05-15',
        '9005155012087',
        '0821234567',
        NULL,   -- HumanEHCOptionsId (nullable FK)
        1,      -- IncomeSourceId (Salary/Wages)
        NULL,   -- SecAppIncomeSourceId (no second applicant)
        1,      -- IdentificationTypeId (RSA ID)
        1,      -- TitleTypeId (Mr)
        18000.00, -- GrossIncome (from MonthlyIncomes — kept in sync)
        13500.00, -- NetIncome
        18000.00, -- TotalCombinedIncome
        0,      -- SecondApplicant
        0,      -- IsMigrated
        0,      -- IsFullyMigrated
        0,      -- PreUnitInspectionCompleted
        0,      -- PreUnitMaintenanceRequired
        0,      -- ExitInspectionCompleted
        0,      -- ExitUnitMaintenanceRequired
        1, 0, 0,
        @CreatedBySystemUserId, GETDATE(),
        @CreatedBySystemUserId, GETDATE()
    )

    DECLARE @AppId INT = SCOPE_IDENTITY()
    PRINT 'PropertyLeaseApplication inserted — Id: ' + CAST(@AppId AS VARCHAR)

    -- --------------------------------------------------------
    -- STEP 3: Monthly Income
    -- --------------------------------------------------------
    INSERT INTO MonthlyIncomes (
        PropertyLeaseApplicationId,
        ApplicantTypeId,
        GrossIncome, NetIncome,
        IsActive, IsDeleted, IsLocked,
        CreatedBySystemUserId, CreatedDateTime,
        ModifiedBySystemUserId, ModifiedDateTime
    )
    VALUES (
        @AppId,
        1,          -- ApplicantTypeId (Primary applicant)
        18000.00, 13500.00,
        1, 0, 0,
        @CreatedBySystemUserId, GETDATE(),
        @CreatedBySystemUserId, GETDATE()
    )

    PRINT 'MonthlyIncome inserted'

    -- --------------------------------------------------------
    -- STEP 4: Monthly Expenses (zeroed — applicant qualifies)
    -- --------------------------------------------------------
    INSERT INTO MonthlyExpenses (
        PropertyLeaseApplicationId,
        ApplicantTypeId,
        IsActive, IsDeleted, IsLocked,
        CreatedBySystemUserId, CreatedDateTime,
        ModifiedBySystemUserId, ModifiedDateTime
    )
    VALUES (
        @AppId,
        1,      -- ApplicantTypeId (Primary applicant)
        1, 0, 0,
        @CreatedBySystemUserId, GETDATE(),
        @CreatedBySystemUserId, GETDATE()
    )

    PRINT 'MonthlyExpenses inserted'

    -- --------------------------------------------------------
    -- STEP 5: PLM History — simulate path to fee approval
    -- --------------------------------------------------------
    INSERT INTO PLMApplicationHistortyLogs (
        UserId, PropertyLeaseApplicationId, AuditAction,
        IsActive, IsDeleted, IsLocked,
        CreatedBySystemUserId, CreatedDateTime,
        ModifiedBySystemUserId, ModifiedDateTime
    )
    VALUES
    (@CustomerId, @AppId, 'Application captured by the applicant (Sashen Moodley) for Ekurhuleni Housing Company.', 1, 0, 0, @CreatedBySystemUserId, GETDATE(), @CreatedBySystemUserId, GETDATE()),
    (@CustomerId, @AppId, 'Application status changed to "Awaiting Application Fee Upload".', 1, 0, 0, @CreatedBySystemUserId, GETDATE(), @CreatedBySystemUserId, GETDATE()),
    (@CustomerId, @AppId, 'Applicant uploaded proof of application fee payment.', 1, 0, 0, @CreatedBySystemUserId, GETDATE(), @CreatedBySystemUserId, GETDATE()),
    (@CSOCustomerId, @AppId, 'Application fee payment validated. Status changed to "Awaiting Assessment Fee Payment".', 1, 0, 0, @CSOCustomerId, GETDATE(), @CSOCustomerId, GETDATE()),
    (@CSOCustomerId, @AppId, 'Assessment fee payment validated and approved. Status changed to "Payment Validated and Approved".', 1, 0, 0, @CSOCustomerId, GETDATE(), @CSOCustomerId, GETDATE())

    PRINT 'PLMApplicationHistortyLogs inserted (5 records)'

    -- --------------------------------------------------------
    -- SUMMARY
    -- --------------------------------------------------------
    PRINT '============================================'
    PRINT 'TC-02 SEED COMPLETE'
    PRINT 'Application Id  : ' + CAST(@AppId AS VARCHAR)
    PRINT 'Reference Number: ' + @RefNumber
    PRINT 'Status          : Payment Validated and Approved (Id 122)'
    PRINT 'Customer        : Sashen Moodley (Id 45, login: Sash38)'
    PRINT 'Area            : Airport Park (Id 24)'
    PRINT '============================================'
    PRINT 'NEXT STEP: Log in as Sash38 and go to:'
    PRINT 'http://localhost:3450/TenantTraining/MyScheduledTraining'
    PRINT 'Click "Schedule Training" on this application.'

    SELECT
        p.Id                          AS ApplicationId,
        p.ApplicationReferenceNumber,
        s.Name                        AS Status,
        p.FirstName + ' ' + p.LastName AS Applicant,
        p.PreferredComplexAreaId,
        p.PurchaserTypeId,
        mi.GrossIncome,
        mi.NetIncome
    FROM PropertyLeaseApplications p
    JOIN Status s ON s.Id = p.StatusId
    LEFT JOIN MonthlyIncomes mi ON mi.PropertyLeaseApplicationId = p.Id
    WHERE p.Id = @AppId

    COMMIT TRANSACTION
    PRINT 'Transaction committed.'

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION
    PRINT 'ERROR  : ' + ERROR_MESSAGE()
    PRINT 'Line   : ' + CAST(ERROR_LINE() AS VARCHAR)
END CATCH
