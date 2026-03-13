-- ============================================================
-- EHC TEST APPLICATION SEED SCRIPT
-- Mirrors EHC2026022700001 exactly at "Awaiting Risk Assessment"
-- Assigns to AshKay (Customer 190) for Airport Park (Area 24)
-- Run on: CRMPLMDEV_2025
-- ============================================================

BEGIN TRANSACTION

BEGIN TRY

    -- --------------------------------------------------------
    -- STEP 1: Reference data IDs (hardcoded from live DB)
    -- --------------------------------------------------------
    DECLARE @CustomerId         INT = 45        -- applicant customer
    DECLARE @SystemUserId       INT = 45        -- applicant system user
    DECLARE @CreatedByUserId    INT = 45
    DECLARE @BackOfficeUserId   INT = 2320      -- back office who processed
    DECLARE @AshKayCustomerId   INT = 190       -- AshKay (Client Services Officer)

    DECLARE @StatusCapture      INT = (SELECT Id FROM Status WHERE [Key] = 'h_andries_scribante_old_age_')     -- initial capture status used in original
    DECLARE @StatusRiskAssess   INT = (SELECT Id FROM Status WHERE [Key] = 's_awaiting_risk_assessment')       -- 137
    DECLARE @StatusSubmitted    INT = (SELECT Id FROM Status WHERE [Key] = 's_rcs_Submitted')                  -- 99
    DECLARE @StatusDocPending   INT = (SELECT Id FROM Status WHERE [Key] = 's_document_pending')               -- 46
    DECLARE @StatusVerified     INT = (SELECT Id FROM Status WHERE [Key] = 's_document_verified')              -- 21

    DECLARE @RespTypeRisk       INT = (SELECT Id FROM ResponsibilityTypes WHERE [Key] = 'r_risk_assessment')   -- 11

    DECLARE @PreferredAreaId    INT = 24        -- Airport Park
    DECLARE @HumanEHCOptionsId  INT = 7
    DECLARE @IncomeSourceId     INT = 1
    DECLARE @IdentTypeId        INT = 1
    DECLARE @TitleTypeId        INT = 1
    DECLARE @PurchaserTypeId    INT = 1
    DECLARE @ReferenceTypeId    INT = 12        -- PLM reference type
    DECLARE @LocationTypeId     INT = 3         -- eServicesDb

    -- Generate a unique reference number using today's date
    DECLARE @RefNumber VARCHAR(50) = 'EHC' + FORMAT(GETDATE(), 'yyyyMMdd') + RIGHT('0000' + CAST(ABS(CHECKSUM(NEWID())) % 9999 AS VARCHAR), 4)

    -- --------------------------------------------------------
    -- STEP 2: PropertyLeaseApplication (main record)
    -- --------------------------------------------------------
    INSERT INTO PropertyLeaseApplications (
        StatusId, PurchaserTypeId, Gender, MaritalStatus, Initial, FirstName, LastName,
        IDNo, DOB, CellNo, HomeNo, WorkNo, PurEmail,
        ResAddress, ResSuburb, ResPostal,
        GrossIncome, NetIncome, TotalCombinedIncome,
        HouseRequired, PrefArea, PreferredComplexAreaId, HumanEHCOptionsId,
        IncomeSourceId, IdentificationTypeId, TitleTypeId, HousingType,
        ApplicationReferenceNumber, CustomerId, SystemUserId,
        ApplicantFullName, SecondApplicant,
        SecAppIncomeSourceId,
        IsActive, IsDeleted, IsLocked,
        CreatedBySystemUserId, CreatedDateTime,
        ModifiedBySystemUserId, ModifiedDateTime
    )
    VALUES (
        @StatusRiskAssess, @PurchaserTypeId, 'Female', NULL, NULL, 'Test', 'Applicant',
        '9001015009087', '1990-01-01', '0831234567', '0831234567', '0831234567', 'test.applicant@test.com',
        '1 Test Street', 'Germiston', '1401',
        20000.00, 15000.00, 15000.00,
        '7', 'p_airportpark', @PreferredAreaId, @HumanEHCOptionsId,
        @IncomeSourceId, @IdentTypeId, @TitleTypeId, 'ho_ekurhuleni_hc',
        @RefNumber, @CustomerId, @SystemUserId,
        'Mr Test Applicant', 0,
        NULL,
        1, 0, 0,
        @CreatedByUserId, GETDATE(),
        @CreatedByUserId, GETDATE()
    )

    DECLARE @AppId INT = SCOPE_IDENTITY()

    PRINT 'PropertyLeaseApplication inserted: Id=' + CAST(@AppId AS VARCHAR) + ', Ref=' + @RefNumber

    -- --------------------------------------------------------
    -- STEP 3: MonthlyIncomes
    -- --------------------------------------------------------
    INSERT INTO MonthlyIncomes (
        GrossIncome, Allowances, FringeBenefits, OtherRegularIncome, TotalGrossIncome,
        PayeTaxLessDeductions, PensionProvidentLessDeductions, UIFLessDeductions,
        MedicalAidLessDeductions, OtherLessDeductions, TotalDeductions,
        NetIncome, OtherDividendsIncome, TotalNetIncome,
        PropertyLeaseApplicationId, ApplicantTypeId,
        IsActive, IsDeleted, IsLocked,
        CreatedBySystemUserId, CreatedDateTime,
        ModifiedBySystemUserId, ModifiedDateTime
    )
    VALUES (
        20000.00, 1000.00, 100.00, 100.00, 21200.00,
        1000.00, 100.00, 100.00,
        100.00, 100.00, 1400.00,
        19800.00, 1000.00, 20800.00,
        @AppId, 1,
        1, 0, 0,
        @CreatedByUserId, GETDATE(),
        @CreatedByUserId, GETDATE()
    )

    PRINT 'MonthlyIncome inserted'

    -- --------------------------------------------------------
    -- STEP 4: MonthlyExpenses
    -- --------------------------------------------------------
    INSERT INTO MonthlyExpenses (
        PropertyLeaseApplicationId, ApplicantTypeId,
        HEAccommodation, HEInsurances, HERatesTaxes, HESecurity, HEUpkeep,
        HEUtilitiesElectricity, HEUtilitiesWater, HEOthers,
        VEFuel, VEInsurance, VEMaintenance, VEVehicleFinance,
        ELifeAssurances, EShortTermInsurances, EOtherInsurancesFuneral,
        LESupportMaintenance, LEBankCharges, LECellularAirtimeData, LEClothing,
        LECreditCards, LEDomesticEmployees, LEDonations, LEEducationSchool,
        LEEntertainment, LEGroceries, LEInstalmentAccounts, LEMedicalAid,
        LEMemberships, LEPersonalLoans, LEPetCare, LERetailAccounts,
        LESecurity, LESubscriptions, LETelephones, LETransport,
        LETV, LEOtherExpenses, LETotalExpenses,
        IsActive, IsDeleted, IsLocked,
        CreatedBySystemUserId, CreatedDateTime,
        ModifiedBySystemUserId, ModifiedDateTime
    )
    VALUES (
        @AppId, 1,
        0,0,0,0,0, 0,0,0, 0,0,0,0, 0,0,0,
        0,0,0,0, 0,0,0,0, 0,0,0,0, 0,0,0,0, 0,0,0,0, 0,0,0,
        1, 0, 0,
        @CreatedByUserId, GETDATE(),
        @CreatedByUserId, GETDATE()
    )

    PRINT 'MonthlyExpenses inserted'

    -- --------------------------------------------------------
    -- STEP 5: Files + Documents (8 docs, same dummy PNG as original)
    -- --------------------------------------------------------
    DECLARE @DummyFile VARBINARY(MAX) = 0x89504E470D0A1A0A0000000D494844520000026E00000102080600000047765BAF000000017352474200AECE1CE90000000467414D410000B18F0BFC61050000000970485973000012740000127401DE661F780000054A49444154785EEDDBCB51EB50144541CECB3F449401603E1260C0E83150105A55DD13DF1056ED63CD
    DECLARE @DummyFileName VARCHAR(200) = 'test_document.png'
    DECLARE @DummyContentType VARCHAR(100) = 'image/png'
    DECLARE @DummyFileSize INT = 1461

    -- DocumentCheckList IDs used by original (100,98,99,102,103,104,101,118)
    DECLARE @CheckListIds TABLE (CheckListId INT, Seq INT)
    INSERT INTO @CheckListIds VALUES (100,1),(98,2),(99,3),(102,4),(103,5),(104,6),(101,7),(118,8)

    DECLARE @CheckListId INT, @Seq INT, @FileId INT, @DocStatusId INT

    DECLARE doc_cursor CURSOR FOR SELECT CheckListId, Seq FROM @CheckListIds ORDER BY Seq

    OPEN doc_cursor
    FETCH NEXT FROM doc_cursor INTO @CheckListId, @Seq

    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Insert File
        INSERT INTO Files (FileName, ContentType, Content, FileSize, IsActive, IsDeleted, IsLocked, CreatedBySystemUserId, CreatedDateTime, ModifiedBySystemUserId, ModifiedDateTime)
        VALUES (@DummyFileName, @DummyContentType, @DummyFile, @DummyFileSize, 1, 0, 0, @CreatedByUserId, GETDATE(), @CreatedByUserId, GETDATE())

        SET @FileId = SCOPE_IDENTITY()

        -- Last doc (118) uses StatusId 116 in original, rest use 46
        SET @DocStatusId = CASE WHEN @CheckListId = 118 THEN 116 ELSE @StatusDocPending END

        -- Insert Document
        INSERT INTO Documents (
            CustomerId, ReferenceTypeId, ReferenceId, LocationTypeId,
            DocumentLocation, DocumentName, StatusId,
            DocumentCheckListId, FileId,
            IsActive, IsDeleted, IsLocked,
            CreatedBySystemUserId, CreatedDateTime,
            ModifiedBySystemUserId, ModifiedDateTime,
            PropertyLeaseApplicationId
        )
        VALUES (
            @CustomerId, @ReferenceTypeId, @CustomerId, @LocationTypeId,
            'eServicesDb', @DummyFileName, @DocStatusId,
            @CheckListId, @FileId,
            1, 0, 0,
            @CreatedByUserId, GETDATE(),
            @CreatedByUserId, GETDATE(),
            @AppId
        )

        FETCH NEXT FROM doc_cursor INTO @CheckListId, @Seq
    END

    CLOSE doc_cursor
    DEALLOCATE doc_cursor

    PRINT 'Documents + Files inserted (8 records)'

    -- --------------------------------------------------------
    -- STEP 6: PLMApplicationHistoryLog entries
    -- --------------------------------------------------------
    INSERT INTO PLMApplicationHistortyLogs (UserId, PropertyLeaseApplicationId, AuditAction, IsActive, IsDeleted, IsLocked, CreatedBySystemUserId, CreatedDateTime, ModifiedBySystemUserId, ModifiedDateTime)
    VALUES
    (@CustomerId,      @AppId, 'Property Lease Management application captured by the applicant By Test Applicant for Ekurhuleni Housing', 1, 0, 0, @CreatedByUserId, GETDATE(), @CreatedByUserId, GETDATE()),
    (@CustomerId,      @AppId, 'Application Status Changed From "In Awaiting Re-Upload of Documents" To "In Awaiting Application Fee Upload".', 1, 0, 0, @CreatedByUserId, GETDATE(), @CreatedByUserId, GETDATE()),
    (@CustomerId,      @AppId, 'Applicant Uploaded Application Documents', 1, 0, 0, @CreatedByUserId, GETDATE(), @CreatedByUserId, GETDATE()),
    (@CustomerId,      @AppId, 'Application Status Changed From "In Awaiting Application Fee Upload" To "In Awaiting Application Fee Validation".', 1, 0, 0, @CreatedByUserId, GETDATE(), @CreatedByUserId, GETDATE()),
    (@AshKayCustomerId,@AppId, 'Application Fee Validated. Application Status Changed To "Awaiting Risk Assessment Outcome".', 1, 0, 0, @AshKayCustomerId, GETDATE(), @AshKayCustomerId, GETDATE())

    PRINT 'PLMApplicationHistoryLogs inserted'

    -- --------------------------------------------------------
    -- STEP 7: RoundRobinQueue (Risk Assessment ? AshKay)
    -- --------------------------------------------------------
    INSERT INTO RoundRobinQueues (
        ClerkId, ResponsibilityTypeId, StatusId,
        IsActive, IsDeleted, IsLocked,
        CreatedBySystemUserId, CreatedDateTime,
        ModifiedBySystemUserId, ModifiedDateTime,
        CurrentTaskDateTime, PropertyLeaseApplicationId
    )
    VALUES (
        @AshKayCustomerId, @RespTypeRisk, @StatusSubmitted,
        1, 0, 0,
        @BackOfficeUserId, GETDATE(),
        @BackOfficeUserId, GETDATE(),
        GETDATE(), @AppId
    )

    PRINT 'RoundRobinQueue inserted (Risk Assessment ? AshKay)'

    -- --------------------------------------------------------
    -- SUMMARY
    -- --------------------------------------------------------
    PRINT '============================================'
    PRINT 'SEED COMPLETE'
    PRINT 'Application ID  : ' + CAST(@AppId AS VARCHAR)
    PRINT 'Reference Number: ' + @RefNumber
    PRINT 'Status          : Awaiting Risk Assessment'
    PRINT 'Assigned To     : AshKay (Customer 190)'
    PRINT 'Area            : Airport Park (Id 24)'
    PRINT '============================================'

    -- Verify
    SELECT
        p.Id,
        p.ApplicationReferenceNumber,
        s.Name AS Status,
        p.FirstName + ' ' + p.LastName AS Applicant,
        p.PreferredComplexAreaId,
        rrq.ClerkId AS AssignedClerkId,
        rt.Name AS AssignedFor
    FROM PropertyLeaseApplications p
    JOIN Status s ON s.Id = p.StatusId
    LEFT JOIN RoundRobinQueues rrq ON rrq.PropertyLeaseApplicationId = p.Id
    LEFT JOIN ResponsibilityTypes rt ON rt.Id = rrq.ResponsibilityTypeId
    WHERE p.Id = @AppId

    COMMIT TRANSACTION
    PRINT 'Transaction committed.'

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION
    PRINT 'ERROR: ' + ERROR_MESSAGE()
    PRINT 'Line : ' + CAST(ERROR_LINE() AS VARCHAR)
END CATCH
