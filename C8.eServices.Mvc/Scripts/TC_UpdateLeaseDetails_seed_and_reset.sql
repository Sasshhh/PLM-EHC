-- ============================================================
-- TC: Update Lease Details - Seed & Repeatable Reset
-- ============================================================
-- Purpose  : Creates a PropertyLeaseApplication at status
--            s_awaiting_inspection_schedule_slots so it appears
--            on the UpdateLeaseDetails dashboard, with all
--            supporting records the form controller needs.
--
-- Idempotent: Run as many times as you like.
--             Each run either RE-USES the existing test app
--             (by ref-number prefix) or inserts a fresh one,
--             then resets every related table so the form
--             starts clean.
--
-- Login as : AshKay  (CSO)
-- URL      : http://localhost:3450/PropertyLeaseApplication/UpdateLeaseDetails
-- ============================================================

SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    -- --------------------------------------------------------
    -- CONSTANTS  (adjust only if your DB differs)
    -- --------------------------------------------------------

    -- Status IDs resolved by Key to avoid hard-coding
    -- s_awaiting_tenant_update_record = the status the UpdateLeaseDetails
    -- dashboard filters on (StatusKeys.AwaitingTenantUpdateDetails)
    DECLARE @StatusUpdateLeaseDetails INT = (
        SELECT Id FROM Status
        WHERE [Key] = 's_awaiting_tenant_update_record'
    )

    DECLARE @PurchaserTypeId        INT = 1     -- Individual Application
    DECLARE @PreferredComplexAreaId INT = 24    -- Airport Park
    DECLARE @CustomerId             INT = 45    -- Sashen Moodley  (Sash38)
    DECLARE @CreatedBySystemUserId  INT = 45
    DECLARE @CSOCustomerId          INT = 190   -- AshKay (Client Services Officer)
    DECLARE @IncomeSourceId         INT = 1     -- Salary/Wages
    DECLARE @IdentificationTypeId   INT = 1     -- RSA ID
    DECLARE @TitleTypeId            INT = 1     -- Mr

    PRINT 'StatusId for UpdateLeaseDetails: ' + ISNULL(CAST(@StatusUpdateLeaseDetails AS VARCHAR), 'NULL - CHECK STATUS KEY!')

    IF @StatusUpdateLeaseDetails IS NULL
    BEGIN
        RAISERROR('Status key s_awaiting_tenant_update_record not found. Check StatusKeys table.', 16, 1)
    END

    -- --------------------------------------------------------
    -- STEP 1: Find or create the test application
    -- --------------------------------------------------------
    DECLARE @AppId  INT
    DECLARE @RefNum NVARCHAR(50)

    -- Reuse existing test record if one already exists
    SELECT TOP 1 @AppId = Id, @RefNum = ApplicationReferenceNumber
    FROM PropertyLeaseApplications
    WHERE ApplicationReferenceNumber LIKE 'TC-ULD-%'
      AND IsDeleted = 0
    ORDER BY Id DESC

    IF @AppId IS NULL
    BEGIN
        -- First run: insert a brand-new application
        SET @RefNum = 'TC-ULD-' + CONVERT(VARCHAR(8), GETDATE(), 112)

        INSERT INTO PropertyLeaseApplications (
            ApplicationReferenceNumber,
            CustomerId, SystemUserId,
            StatusId, PurchaserTypeId,
            PreferredComplexAreaId,
            FirstName, LastName,
            PurEmail, DOB, IDNo, CellNo,
            HumanEHCOptionsId,
            IncomeSourceId, SecAppIncomeSourceId,
            IdentificationTypeId, TitleTypeId,
            GrossIncome, NetIncome, TotalCombinedIncome,
            SecondApplicant,
            IsMigrated, IsFullyMigrated,
            PreUnitInspectionCompleted, PreUnitMaintenanceRequired,
            ExitInspectionCompleted, ExitUnitMaintenanceRequired,
            IsActive, IsDeleted, IsLocked,
            CreatedBySystemUserId, CreatedDateTime,
            ModifiedBySystemUserId, ModifiedDateTime
        )
        VALUES (
            @RefNum,
            @CustomerId, 45,
            @StatusUpdateLeaseDetails, @PurchaserTypeId,
            @PreferredComplexAreaId,
            'Sashen', 'Moodley',
            'sashen.moodley@test.com', '1990-05-15', '9005155012087', '0821234567',
            NULL,
            @IncomeSourceId, NULL,
            @IdentificationTypeId, @TitleTypeId,
            18000.00, 13500.00, 18000.00,
            0, 0, 0, 0, 0, 0, 0,
            1, 0, 0,
            @CreatedBySystemUserId, GETDATE(),
            @CreatedBySystemUserId, GETDATE()
        )

        SET @AppId = SCOPE_IDENTITY()
        PRINT 'New application inserted   Id: ' + CAST(@AppId AS VARCHAR) + '  Ref: ' + @RefNum
    END
    ELSE
    BEGIN
        PRINT 'Reusing existing application Id: ' + CAST(@AppId AS VARCHAR) + '  Ref: ' + @RefNum

        -- Reset status back to UpdateLeaseDetails stage
        UPDATE PropertyLeaseApplications
        SET StatusId               = @StatusUpdateLeaseDetails,
            ModifiedDateTime       = GETDATE(),
            ModifiedBySystemUserId = @CSOCustomerId
        WHERE Id = @AppId

        PRINT 'Status reset to s_awaiting_tenant_update_record'
    END

    -- --------------------------------------------------------
    -- STEP 2: Remove any stale lease agreement master
    --         so the form starts with no pre-existing data
    -- --------------------------------------------------------
    DELETE FROM PropertyLeaseAgreementMasters
    WHERE PropertyLeaseApplicationId = @AppId

    DELETE FROM PropertyLeaseAgreementMasterAudits
    WHERE PropertyLeaseApplicationId = @AppId

    PRINT 'Cleared PropertyLeaseAgreementMaster (if any)'

    -- --------------------------------------------------------
    -- STEP 3: Soft-delete any stale LeaseDetails rows so STEP 3b
    --         inserts a clean one. Rows with IsDeleted=1 are ignored
    --         by the controller query so FK constraints are preserved.
    -- --------------------------------------------------------
    UPDATE LeaseDetails
    SET IsDeleted = 1, IsActive = 0, ModifiedDateTime = GETDATE()
    WHERE PropertyLeaseApplicationId = @AppId
      AND IsDeleted = 0

    IF @@ROWCOUNT > 0
        PRINT 'Soft-deleted existing LeaseDetails row(s)'

    -- --------------------------------------------------------
    -- STEP 3b: Ensure a LeaseDetails row exists with IsNew=1.
    --
    --          The controller path (e.g. GenerateLeaseAgreement) queries:
    --            db.LeaseDetails.FirstOrDefault(x =>
    --              x.PropertyLeaseApplicationId == application.Id
    --              && x.IsNew && x.IsActive && !x.IsDeleted)
    --          and throws "Invalid Property Lease." if null.
    --
    --          Fields are copied from the Airport Park AAP unit (Id=1)
    --          and the application itself. StatusId 139 =
    --          a_awaiting_lease_agreement (matches the LeaseDetails
    --          status used in completed real records at this stage).
    -- --------------------------------------------------------
    DECLARE @LeaseStatusId  INT = 139  -- a_awaiting_lease_agreement
    DECLARE @LeaseRefNo     NVARCHAR(50) = 'TC-ULD-LD-' + CONVERT(VARCHAR(8), GETDATE(), 112)

    IF NOT EXISTS (
        SELECT 1 FROM LeaseDetails
        WHERE PropertyLeaseApplicationId = @AppId
          AND IsNew     = 1
          AND IsActive  = 1
          AND IsDeleted = 0
    )
    BEGIN
        INSERT INTO LeaseDetails (
            PropertyLeaseApplicationId,
            LeaseReferenceNo,
            PurchaserTypeId,
            StartDate, EndDate, PeriodInMonths,
            RenewalNotice, TerminationNotice,
            DepositeAmount, RentalAmount, VATAmount, TotalIncludingVAT,
            StatementDate, EscalationDate,
            Email, SMS, Postal,
            LastName, FirstNames, IDNo,
            LeaAddress, LeaPostal, LeaSuburb,
            buildingName, PropertyId, SpaceUnitNo,
            StatusId, SystemUserId,
            IsNew, IsRenewed,
            DetailsUpdated,
            PreparationFee, CreditCheckFee,
            LeaseAdministrationFee,
            Electricity, ELEC,
            Water,       WTR,
            Refuse,
            Sewerage,
            SecurityFee, SEC,
            ShadePortParking, SPP,
            OpenParking,      OPP,
            StoreRooms,       STR,
            FloorNumber,
            TotalMonthlyCharges,
            MonthsOffered, Completed,
            IsActive, IsDeleted, IsLocked,
            CreatedBySystemUserId, CreatedDateTime,
            ModifiedBySystemUserId, ModifiedDateTime
        )
        SELECT
            @AppId,
            @LeaseRefNo,
            1,                              -- PurchaserTypeId: Individual
            GETDATE(),                      -- StartDate (placeholder; CSO fills on form)
            DATEADD(MONTH, 24, GETDATE()),  -- EndDate
            24,                             -- PeriodInMonths
            DATEADD(MONTH, 21, GETDATE()),  -- RenewalNotice
            DATEADD(MONTH, 23, GETDATE()),  -- TerminationNotice
            aap.RequiedDepositAmount,       -- DepositeAmount
            aap.MonthlyRentalAmount,        -- RentalAmount
            aap.MonthlyRentalAmount * 0.15, -- VATAmount
            aap.MonthlyRentalAmount * 1.15, -- TotalIncludingVAT
            NULL, NULL,                     -- StatementDate, EscalationDate
            0, 0, 0,                        -- Email, SMS, Postal
            p.LastName, p.FirstName, p.IDNo,
            aap.StreetName, aap.Postal, aap.Township,
            aap.BuildingName,               -- buildingName
            CAST(aap.Id AS NVARCHAR(50)),   -- PropertyId
            aap.SpaceUnitNumber,            -- SpaceUnitNo
            @LeaseStatusId,                 -- StatusId
            @CSOCustomerId,                 -- SystemUserId
            1,                              -- IsNew = 1  (required by the controller query)
            0,                              -- IsRenewed
            0,                              -- DetailsUpdated = 0 (not yet updated by CSO)
            0, 0,                           -- PreparationFee, CreditCheckFee
            0,                              -- LeaseAdministrationFee
            0, 0,                           -- Electricity, ELEC
            0, 0,                           -- Water, WTR
            0,                              -- Refuse
            0,                              -- Sewerage
            0, 0,                           -- SecurityFee, SEC
            0, 0,                           -- ShadePortParking, SPP
            0, 0,                           -- OpenParking, OPP
            0, 0,                           -- StoreRooms, STR
            NULL,                           -- FloorNumber
            0,                              -- TotalMonthlyCharges
            24, 0,                          -- MonthsOffered, Completed
            1, 0, 0,
            @CreatedBySystemUserId, GETDATE(),
            @CreatedBySystemUserId, GETDATE()
        FROM PropertyLeaseApplications p
        INNER JOIN MatchedUnits mu  ON mu.PropertyLeaseApplicationId = p.Id AND mu.IsDeleted = 0
        INNER JOIN ApplicationAllocatedProperties aap ON aap.Id = mu.ApplicationAllocatedPropertyId
        WHERE p.Id = @AppId

        PRINT 'LeaseDetails inserted (IsNew=1, DetailsUpdated=0)'
    END
    ELSE
        PRINT 'LeaseDetails already exists (IsNew=1, active) - skipped'

    -- --------------------------------------------------------
    -- STEP 4: Ensure MonthlyIncome exists (form reads it)
    -- --------------------------------------------------------
    IF NOT EXISTS (
        SELECT 1 FROM MonthlyIncomes
        WHERE PropertyLeaseApplicationId = @AppId AND IsDeleted = 0
    )
    BEGIN
        INSERT INTO MonthlyIncomes (
            PropertyLeaseApplicationId, ApplicantTypeId,
            GrossIncome, NetIncome,
            IsActive, IsDeleted, IsLocked,
            CreatedBySystemUserId, CreatedDateTime,
            ModifiedBySystemUserId, ModifiedDateTime
        )
        VALUES (
            @AppId, 1,
            18000.00, 13500.00,
            1, 0, 0,
            @CreatedBySystemUserId, GETDATE(),
            @CreatedBySystemUserId, GETDATE()
        )
        PRINT 'MonthlyIncome inserted'
    END
    ELSE
    BEGIN
        PRINT 'MonthlyIncome already exists - skipped'
    END

    -- --------------------------------------------------------
    -- STEP 5: Ensure MonthlyExpenses exists
    -- --------------------------------------------------------
    IF NOT EXISTS (
        SELECT 1 FROM MonthlyExpenses
        WHERE PropertyLeaseApplicationId = @AppId AND IsDeleted = 0
    )
    BEGIN
        INSERT INTO MonthlyExpenses (
            PropertyLeaseApplicationId, ApplicantTypeId,
            IsActive, IsDeleted, IsLocked,
            CreatedBySystemUserId, CreatedDateTime,
            ModifiedBySystemUserId, ModifiedDateTime
        )
        VALUES (
            @AppId, 1,
            1, 0, 0,
            @CreatedBySystemUserId, GETDATE(),
            @CreatedBySystemUserId, GETDATE()
        )
        PRINT 'MonthlyExpenses inserted'
    END
    ELSE
    BEGIN
        PRINT 'MonthlyExpenses already exists - skipped'
    END

    -- --------------------------------------------------------
    -- STEP 6: Ensure a MatchedUnits row exists pointing to a
    --         real Airport Park unit (OfferedComplexId = 24).
    --
    --         The controller (ApplicationUpdateTenantLeaseDetails) needs:
    --           matchedUnit.ApplicationAllocatedPropertyId -> applicationAllocatedProperty
    --         This is used for rental amount, deposit, ViewBag.UnitId etc.
    --         It MUST belong to Airport Park or the area routing breaks.
    --
    --         matchedUnit.UnitsId is loaded inside a try/catch in the
    --         controller; all real Airport Park MatchedUnits have UnitsId=NULL
    --         and the form works fine without it (it falls back to new Units()).
    --
    --         IsTaken is set to 1 while the test app holds the unit,
    --         and restored to 0 on each reset so the unit stays available.
    -- --------------------------------------------------------

    -- Best Airport Park unit: Id=1 (unit 54, Queens Court, R5000/month,
    -- IsTaken=0, BuildingName set, real rental data)
    DECLARE @UnitId INT = 1  -- AAP Id=1, OfferedComplexId=24 (Airport Park)

    -- Restore the unit to available on every reset
    -- (in case a prior test run left it marked taken)
    UPDATE ApplicationAllocatedProperties
    SET IsTaken = 0, ModifiedDateTime = GETDATE()
    WHERE Id = @UnitId AND IsTaken = 1

    -- Ensure BuildingName is populated (required by controller for lease.buildingName)
    UPDATE ApplicationAllocatedProperties
    SET BuildingName = ISNULL(BuildingName, 'Queens Court')
    WHERE Id = @UnitId AND BuildingName IS NULL

    IF NOT EXISTS (
        SELECT 1 FROM MatchedUnits
        WHERE PropertyLeaseApplicationId = @AppId AND IsDeleted = 0
    )
    BEGIN
        INSERT INTO MatchedUnits (
            PropertyLeaseApplicationId,
            ApplicationAllocatedPropertyId,
            UnitsId,        -- NULL is correct for Airport Park (matches real data)
            CustomerId,
            IsAccepted,
            IsActive, IsDeleted, IsLocked,
            CreatedBySystemUserId, CreatedDateTime,
            ModifiedBySystemUserId, ModifiedDateTime
        )
        VALUES (
            @AppId,
            @UnitId,
            NULL,           -- Airport Park units do not use Units table
            @CustomerId,
            1,
            1, 0, 0,
            @CreatedBySystemUserId, GETDATE(),
            @CreatedBySystemUserId, GETDATE()
        )
        PRINT 'MatchedUnits inserted (Airport Park AAP Id ' + CAST(@UnitId AS VARCHAR) + ', UnitsId NULL)'
    END
    ELSE
    BEGIN
        -- On re-run: fix up any wrong ApplicationAllocatedPropertyId or stale UnitsId
        UPDATE MatchedUnits
        SET ApplicationAllocatedPropertyId = @UnitId,
            UnitsId                        = NULL,
            ModifiedDateTime               = GETDATE()
        WHERE PropertyLeaseApplicationId = @AppId
          AND IsDeleted = 0

        PRINT 'MatchedUnits updated (AAP reset to Airport Park Id ' + CAST(@UnitId AS VARCHAR) + ')'
    END

    -- Mark the unit as taken now that the test app holds it
    UPDATE ApplicationAllocatedProperties
    SET IsTaken = 1, ModifiedDateTime = GETDATE()
    WHERE Id = @UnitId

    PRINT 'Unit ' + CAST(@UnitId AS VARCHAR) + ' (Queens Court, unit 54) marked IsTaken=1'

    -- --------------------------------------------------------
    -- STEP 6b: Ensure ApplicantUnits row exists.
    --
    --          The controller queries:
    --            ApplicantUnit AppUnit = core.ApplicantUnits
    --              .FirstOrDefault(x => x.PropertyLeaseApplicationId == Id)
    --          If NULL the RR routing block is skipped and the form
    --          crashes further on where AppUnit is assumed non-null.
    --
    --          MatchedID must point to the MatchedUnits.Id just upserted.
    --          LeaseID is non-nullable int; 0 = no active lease yet.
    -- --------------------------------------------------------
    DECLARE @MatchedUnitId INT = (
        SELECT TOP 1 Id FROM MatchedUnits
        WHERE PropertyLeaseApplicationId = @AppId AND IsDeleted = 0
        ORDER BY Id DESC
    )

    IF NOT EXISTS (
        SELECT 1 FROM ApplicantUnits
        WHERE PropertyLeaseApplicationId = @AppId AND IsDeleted = 0
    )
    BEGIN
        INSERT INTO ApplicantUnits (
            PropertyLeaseApplicationId,
            MatchedID,
            LeaseID,
            DepositPaid,
            OutstandingDepopsitAmount,
            IsActive, IsDeleted, IsLocked,
            CreatedBySystemUserId, CreatedDateTime,
            ModifiedBySystemUserId, ModifiedDateTime
        )
        VALUES (
            @AppId,
            @MatchedUnitId,
            0,
            0, 0,
            1, 0, 0,
            @CreatedBySystemUserId, GETDATE(),
            @CreatedBySystemUserId, GETDATE()
        )
        PRINT 'ApplicantUnits inserted (MatchedID: ' + CAST(@MatchedUnitId AS VARCHAR) + ')'
    END
    ELSE
    BEGIN
        UPDATE ApplicantUnits
        SET MatchedID        = @MatchedUnitId,
            LeaseID          = 0,
            ModifiedDateTime = GETDATE()
        WHERE PropertyLeaseApplicationId = @AppId
          AND IsDeleted = 0

        PRINT 'ApplicantUnits updated (MatchedID: ' + CAST(@MatchedUnitId AS VARCHAR) + ')'
    END

    -- --------------------------------------------------------
    -- STEP 7: Reset Round Robin queue for CaptureLeaseDetails
    --         The UpdateLeaseDetails dashboard requires:
    --           RoundRobinQueues.ResponsibilityTypeId = 15 (r_capture_lease_details)
    --           RoundRobinQueues.StatusId             = 99 (Submitted / open)
    --           RoundRobinQueues.ClerkId              = logged-in user's Customer.Id
    --           RoundRobinQueues.EndTaskDateTime      = NULL
    -- --------------------------------------------------------
    DECLARE @ArchivedStatusId          INT = 95   -- s_rcs_Archived
    DECLARE @SubmittedStatusId         INT = 99   -- s_rcs_Submitted
    DECLARE @CaptureLeaseResponsTypeId INT = 15   -- r_capture_lease_details

    -- Archive any existing open RRQ rows for this app at this responsibility
    UPDATE RoundRobinQueues
    SET StatusId         = @ArchivedStatusId,
        EndTaskDateTime  = GETDATE(),
        ModifiedDateTime = GETDATE()
    WHERE PropertyLeaseApplicationId = @AppId
      AND ResponsibilityTypeId       = @CaptureLeaseResponsTypeId
      AND StatusId                   = @SubmittedStatusId

    -- Insert a fresh open RRQ row assigned to the CSO (AshKay, Customer Id 190)
    INSERT INTO RoundRobinQueues (
        ClerkId,
        ResponsibilityTypeId,
        StatusId,
        PropertyLeaseApplicationId,
        CurrentTaskDateTime,
        EndTaskDateTime,
        DepartmentId,
        IsActive, IsDeleted, IsLocked,
        CreatedBySystemUserId, CreatedDateTime,
        ModifiedBySystemUserId, ModifiedDateTime
    )
    VALUES (
        @CSOCustomerId,           -- ClerkId = AshKay Customer Id (190)
        @CaptureLeaseResponsTypeId,
        @SubmittedStatusId,       -- open / Submitted
        @AppId,
        GETDATE(),
        NULL,                     -- EndTaskDateTime must be NULL to show on dashboard
        1,                        -- DepartmentId (any valid)
        1, 0, 0,
        @CSOCustomerId, GETDATE(),
        @CSOCustomerId, GETDATE()
    )

    PRINT 'RoundRobinQueue row inserted for CaptureLeaseDetails (ClerkId: ' + CAST(@CSOCustomerId AS VARCHAR) + ')'

    -- --------------------------------------------------------
    -- STEP 8: PLM history entry for this reset
    -- --------------------------------------------------------
    INSERT INTO PLMApplicationHistortyLogs (
        UserId, PropertyLeaseApplicationId, AuditAction,
        IsActive, IsDeleted, IsLocked,
        CreatedBySystemUserId, CreatedDateTime,
        ModifiedBySystemUserId, ModifiedDateTime
    )
    VALUES (
        @CSOCustomerId,
        @AppId,
        'TEST RESET: Application status reset to Update Lease Details stage by test script.',
        1, 0, 0,
        @CSOCustomerId, GETDATE(),
        @CSOCustomerId, GETDATE()
    )

    PRINT 'PLM history entry added'

    -- --------------------------------------------------------
    -- SUMMARY
    -- --------------------------------------------------------
    PRINT ''
    PRINT '============================================'
    PRINT 'TC-ULD RESET COMPLETE'
    PRINT 'Application Id  : ' + CAST(@AppId AS VARCHAR)
    PRINT 'Reference Number: ' + @RefNum
    PRINT 'Status          : s_awaiting_tenant_update_record'
    PRINT 'Customer        : Sashen Moodley (Id 45, login: Sash38)'
    PRINT 'CSO             : AshKay (Id 190)'
    PRINT '============================================'
    PRINT 'NEXT STEP: Log in as AshKay (CSO) and go to:'
    PRINT 'http://localhost:3450/PropertyLeaseApplication/UpdateLeaseDetails'
    PRINT 'The application should appear in the dashboard.'
    PRINT ''
    PRINT 'To reset and re-test: run this script again.'
    PRINT '============================================'

    -- Quick state snapshot
    SELECT
        p.Id                                    AS ApplicationId,
        p.ApplicationReferenceNumber,
        s.Name                                  AS [Status],
        s.[Key]                                 AS StatusKey,
        p.FirstName + ' ' + p.LastName          AS Applicant,
        p.PreferredComplexAreaId,
        mi.GrossIncome,
        mi.NetIncome,
        aap.SpaceUnitNumber,
        ISNULL(CAST(lam.Id AS VARCHAR), 'none') AS LeaseAgreementMasterId
    FROM PropertyLeaseApplications p
    INNER JOIN Status s ON s.Id = p.StatusId
    LEFT  JOIN MonthlyIncomes mi ON mi.PropertyLeaseApplicationId = p.Id AND mi.IsDeleted = 0
    LEFT  JOIN MatchedUnits mu ON mu.PropertyLeaseApplicationId = p.Id AND mu.IsDeleted = 0
    LEFT  JOIN ApplicationAllocatedProperties aap ON aap.Id = mu.ApplicationAllocatedPropertyId
    LEFT  JOIN PropertyLeaseAgreementMasters lam ON lam.PropertyLeaseApplicationId = p.Id
    WHERE p.Id = @AppId

    COMMIT TRANSACTION;
    PRINT 'Transaction committed.'

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'ERROR  : ' + ERROR_MESSAGE()
    PRINT 'Line   : ' + CAST(ERROR_LINE() AS VARCHAR)
END CATCH
