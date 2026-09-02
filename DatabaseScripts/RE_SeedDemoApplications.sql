USE [PropertyLeaseManagementRealEstate]
GO

-- ==========================================================================
-- RE_SeedDemoApplications.sql
-- Seeds 3 demo lease applications at various workflow stages so that
-- worklist screens (Risk Assessment, Departmental Reviews, etc.) show data.
-- Safe to re-run: uses NOT EXISTS guards.
-- ==========================================================================

PRINT '--- Seeding Demo RE Applications ---';

-- Look up the RealEstateCustomer user/customer IDs
DECLARE @SysUserId INT = (SELECT TOP 1 Id FROM dbo.SystemUsers WHERE UserName = 'RealEstateCustomer' ORDER BY Id DESC);
DECLARE @CustomerId INT = (SELECT TOP 1 Id FROM dbo.Customers WHERE SystemUserId = @SysUserId ORDER BY Id DESC);

-- Look up Status IDs by key
DECLARE @StatusAwaitingRisk INT     = (SELECT Id FROM dbo.Status WHERE [Key] = 's_awaiting_risk_assessment');
DECLARE @StatusAwaitingFee INT      = (SELECT Id FROM dbo.Status WHERE [Key] = 's_awaiting_application_fee_validation');
DECLARE @StatusVerified INT         = (SELECT Id FROM dbo.Status WHERE [Key] = 's_property_verified');
DECLARE @StatusCirculation INT      = (SELECT Id FROM dbo.Status WHERE [Key] = 're_in_circulation_for_evaluation');

-- Pick a CCC, Facility, and Unit
DECLARE @CCCId INT = 10;       -- Tokoza
DECLARE @FacilityId INT = 1;   -- Fannie Malape Co-operatives Industrial Hive Centre
DECLARE @UnitId INT = 1;       -- Indoor Unit (65m²)

-- Validate prerequisites
IF @SysUserId IS NULL OR @CustomerId IS NULL
BEGIN
    PRINT 'ERROR: RealEstateCustomer user not found. Run RE_Backfill_MasterData_And_Users.sql first.';
    RETURN;
END

IF @StatusAwaitingRisk IS NULL
BEGIN
    PRINT 'ERROR: Status s_awaiting_risk_assessment not found. Please check Status table.';
    RETURN;
END

-- ============================================================
-- Demo Application 1: Awaiting Risk Assessment
-- (This is what shows up on the UC 07 Risk Assessment screen)
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM dbo.RE_Applications WHERE ApplicationReferenceNumber = 'RE-DEMO-2026-001')
BEGIN
    INSERT INTO dbo.RE_Applications (
        ApplicationReferenceNumber, SystemUserId, CustomerId, ApplicantType,
        EntityName, CompanyRegistrationNumber, VatRegistrationNumber, TaxReferenceNumber,
        EntityRegisteredAddress, EntityRegisteredPostalCode,
        AuthorizedRepresentativeName, AuthorizedRepresentativeCapacity,
        EntityTelephone, EntityMobile, EntityFax, EntityEmail,
        BankName, BankAccountType, BankAccountName, BankAccountNumber, BankBranchCode,
        PurposeOfLease, CCCId, ErfFarmNumber, PropertyAddress, TownshipSuburbFarmName, PropertyPostalCode,
        FacilityOutdoorAdvertising, FacilityTelecommunications, FacilityInformalTrading, FacilityTaxiRankTrading,
        FacilityVocationalSkills, FacilityComputerTraining, FacilityIndustrialPark, FacilityBusinessHub,
        FacilityAutomotiveHub, FacilityAgriPark, FacilityIncubationFarm,
        StatusId, IsActive, IsDeleted, IsLocked,
        CreatedBySystemUserId, CreatedDateTime, ModifiedBySystemUserId, ModifiedDateTime,
        DepartmentId, SelectedFacilityId, SelectedFacilityUnitId, SelectedUnitCount, CalculatedMonthlyRental,
        PaymentValidationComment,
        SelectedUnitsJson
    ) VALUES (
        'RE-DEMO-2026-001', @SysUserId, @CustomerId, 'Company/Close Corporation',
        'Ekurhuleni Demo Trading (Pty) Ltd', '2026/123456/07', '4012345678', '1234567890',
        '12 Mandela Avenue, Tokoza', '1426',
        'John Mabaso', 'Managing Director',
        '0119991001', '0829991001', NULL, 'demo.trading@test.com',
        'FNB', 'Cheque', 'Ekurhuleni Demo Trading', '62012345678', '250655',
        'Commercial', @CCCId, 'ERF-4520', '14 Chris Hani Drive, Tokoza', 'Tokoza', '1426',
        0, 0, 0, 0,
        0, 0, 1, 0,
        0, 0, 0,
        @StatusAwaitingRisk, 1, 0, 0,
        @SysUserId, DATEADD(DAY, -5, GETDATE()), @SysUserId, DATEADD(DAY, -1, GETDATE()),
        3, @FacilityId, @UnitId, 2, 7410.00,
        'Payment verified - R500 application fee confirmed by Finance Officer.',
        '[{"facilityId":1,"facilityName":"Fannie Malape Co-operatives Industrial Hive Centre","unitId":1,"unitType":"Indoor Unit","unitSize":65.00,"tariffPerSqm":57.00,"qty":2,"monthlyRental":7410.00}]'
    );
    PRINT 'Inserted Demo App 1: RE-DEMO-2026-001 (Awaiting Risk Assessment)';
END
ELSE
BEGIN
    -- Update status to Awaiting Risk Assessment in case it was changed
    UPDATE dbo.RE_Applications SET StatusId = @StatusAwaitingRisk WHERE ApplicationReferenceNumber = 'RE-DEMO-2026-001';
    PRINT 'Updated Demo App 1: RE-DEMO-2026-001 status to Awaiting Risk Assessment';
END

-- ============================================================
-- Demo Application 2: Awaiting Application Fee Validation
-- (Shows up on UC 06 Payment Validation screen)
-- ============================================================
IF @StatusAwaitingFee IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.RE_Applications WHERE ApplicationReferenceNumber = 'RE-DEMO-2026-002')
BEGIN
    INSERT INTO dbo.RE_Applications (
        ApplicationReferenceNumber, SystemUserId, CustomerId, ApplicantType,
        EntityName, CompanyRegistrationNumber, VatRegistrationNumber, TaxReferenceNumber,
        EntityRegisteredAddress, EntityRegisteredPostalCode,
        AuthorizedRepresentativeName, AuthorizedRepresentativeCapacity,
        EntityTelephone, EntityMobile, EntityFax, EntityEmail,
        BankName, BankAccountType, BankAccountName, BankAccountNumber, BankBranchCode,
        PurposeOfLease, CCCId, ErfFarmNumber, PropertyAddress, TownshipSuburbFarmName, PropertyPostalCode,
        FacilityOutdoorAdvertising, FacilityTelecommunications, FacilityInformalTrading, FacilityTaxiRankTrading,
        FacilityVocationalSkills, FacilityComputerTraining, FacilityIndustrialPark, FacilityBusinessHub,
        FacilityAutomotiveHub, FacilityAgriPark, FacilityIncubationFarm,
        StatusId, IsActive, IsDeleted, IsLocked,
        CreatedBySystemUserId, CreatedDateTime, ModifiedBySystemUserId, ModifiedDateTime,
        DepartmentId, SelectedFacilityId, SelectedFacilityUnitId, SelectedUnitCount, CalculatedMonthlyRental,
        SelectedUnitsJson
    ) VALUES (
        'RE-DEMO-2026-002', @SysUserId, @CustomerId, 'Individual',
        'Sipho Ndlovu Trading', NULL, NULL, '9876543210',
        '45 Khumalo Street, Tsakane', '1550',
        'Sipho Ndlovu', 'Owner',
        '0119992002', '0829992002', NULL, 'sipho.ndlovu@test.com',
        'Standard Bank', 'Savings', 'Sipho Ndlovu', '200123456789', '051001',
        'Retail', 11, 'ERF-7810', '23 Sisulu Way, Tsakane', 'Tsakane', '1550',
        0, 0, 1, 0,
        0, 0, 0, 1,
        0, 0, 0,
        @StatusAwaitingFee, 1, 0, 0,
        @SysUserId, DATEADD(DAY, -3, GETDATE()), @SysUserId, DATEADD(DAY, -3, GETDATE()),
        3, 3, 3, 1, 570.00,
        '[{"facilityId":3,"facilityName":"Tsakane Business Park","unitId":3,"unitType":"Offices","unitSize":10.00,"tariffPerSqm":57.00,"qty":1,"monthlyRental":570.00}]'
    );
    PRINT 'Inserted Demo App 2: RE-DEMO-2026-002 (Awaiting Fee Validation)';
END

-- ============================================================
-- Demo Application 3: Property Verified (Departmental Reviews)
-- (Shows up on UC 08 Departmental Reviews screen)
-- ============================================================
IF @StatusVerified IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.RE_Applications WHERE ApplicationReferenceNumber = 'RE-DEMO-2026-003')
BEGIN
    INSERT INTO dbo.RE_Applications (
        ApplicationReferenceNumber, SystemUserId, CustomerId, ApplicantType,
        EntityName, CompanyRegistrationNumber, VatRegistrationNumber, TaxReferenceNumber,
        EntityRegisteredAddress, EntityRegisteredPostalCode,
        AuthorizedRepresentativeName, AuthorizedRepresentativeCapacity,
        EntityTelephone, EntityMobile, EntityFax, EntityEmail,
        BankName, BankAccountType, BankAccountName, BankAccountNumber, BankBranchCode,
        PurposeOfLease, CCCId, ErfFarmNumber, PropertyAddress, TownshipSuburbFarmName, PropertyPostalCode,
        FacilityOutdoorAdvertising, FacilityTelecommunications, FacilityInformalTrading, FacilityTaxiRankTrading,
        FacilityVocationalSkills, FacilityComputerTraining, FacilityIndustrialPark, FacilityBusinessHub,
        FacilityAutomotiveHub, FacilityAgriPark, FacilityIncubationFarm,
        StatusId, IsActive, IsDeleted, IsLocked,
        CreatedBySystemUserId, CreatedDateTime, ModifiedBySystemUserId, ModifiedDateTime,
        DepartmentId, SelectedFacilityId, SelectedFacilityUnitId, SelectedUnitCount, CalculatedMonthlyRental,
        PaymentValidationComment, RiskAssessmentRecommendation, RiskAssessmentReason,
        CreditBureauResult, HomeAffairsResult, DeedsResult, SassaResult, CipcResult,
        SelectedUnitsJson
    ) VALUES (
        'RE-DEMO-2026-003', @SysUserId, @CustomerId, 'Company/Close Corporation',
        'Gauteng Motor Works CC', '2025/654321/23', '4098765432', '0987654321',
        '88 Automotive Blvd, Katlehong', '1431',
        'Thabo Molefe', 'Director',
        '0119993003', '0829993003', NULL, 'thabo.motorworks@test.com',
        'Absa', 'Cheque', 'Gauteng Motor Works CC', '405123456789', '632005',
        'Manufacturing', @CCCId, 'ERF-9250', '90 Workshop Lane, Katlehong', 'Katlehong', '1431',
        0, 0, 0, 0,
        0, 0, 0, 0,
        1, 0, 0,
        @StatusVerified, 1, 0, 0,
        @SysUserId, DATEADD(DAY, -10, GETDATE()), @SysUserId, DATEADD(DAY, -2, GETDATE()),
        3, 17, 37, 1, 8576.00,
        'Payment validated on 2026-07-03.',
        'Recommended', 'All checks clear. No adverse findings.',
        'Clear', 'Verified', 'No encumbrances', 'No grants', 'Active and Compliant',
        '[{"facilityId":17,"facilityName":"Katlehong Automotive manufacturing hub","unitId":37,"unitType":"Unit A01 Workshop","unitSize":128.00,"tariffPerSqm":67.00,"qty":1,"monthlyRental":8576.00}]'
    );
    PRINT 'Inserted Demo App 3: RE-DEMO-2026-003 (Property Verified / Ready for Departmental Review)';
END

-- ============================================================
-- Create RoundRobinQueue entries so the property officer can
-- see the apps in their worklist (they are filtered by ClerkId)
-- ============================================================
PRINT 'Setting up RoundRobinQueue work items...';

DECLARE @PropOfficerSysId INT = (SELECT TOP 1 Id FROM dbo.SystemUsers WHERE UserName = 're_property_officer' ORDER BY Id DESC);
DECLARE @PropOfficerCustId INT = (SELECT TOP 1 Id FROM dbo.Customers WHERE SystemUserId = @PropOfficerSysId ORDER BY Id DESC);

DECLARE @FinOfficerSysId INT = (SELECT TOP 1 Id FROM dbo.SystemUsers WHERE UserName = 're_finance_officer' ORDER BY Id DESC);
DECLARE @FinOfficerCustId INT = (SELECT TOP 1 Id FROM dbo.Customers WHERE SystemUserId = @FinOfficerSysId ORDER BY Id DESC);

DECLARE @RespTypeRisk INT = (SELECT Id FROM dbo.ResponsibilityTypes WHERE [Key] = 're_risk_assessment');
DECLARE @RespTypeFee INT  = (SELECT Id FROM dbo.ResponsibilityTypes WHERE [Key] = 're_verify_payment');
DECLARE @StatusSubmitted INT = (SELECT Id FROM dbo.Status WHERE [Key] = 's_rcs_Submitted');

DECLARE @AppId1 INT = (SELECT Id FROM dbo.RE_Applications WHERE ApplicationReferenceNumber = 'RE-DEMO-2026-001');
DECLARE @AppId2 INT = (SELECT Id FROM dbo.RE_Applications WHERE ApplicationReferenceNumber = 'RE-DEMO-2026-002');

-- Queue entry for Risk Assessment (App 1 -> Property Officer)
IF @AppId1 IS NOT NULL AND @PropOfficerCustId IS NOT NULL AND @RespTypeRisk IS NOT NULL AND @StatusSubmitted IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.RoundRobinQueues WHERE RealEstateApplicationId = @AppId1 AND ResponsibilityTypeId = @RespTypeRisk AND EndTaskDateTime IS NULL)
    BEGIN
        INSERT INTO dbo.RoundRobinQueues (ClerkId, ResponsibilityTypeId, StatusId, RealEstateApplicationId, CurrentTaskDateTime, IsActive, IsDeleted, CreatedBySystemUserId, CreatedDateTime)
        VALUES (@PropOfficerCustId, @RespTypeRisk, @StatusSubmitted, @AppId1, DATEADD(DAY, -1, GETDATE()), 1, 0, @PropOfficerSysId, DATEADD(DAY, -1, GETDATE()));
        PRINT 'Inserted RoundRobinQueue: App1 -> Property Officer (Risk Assessment)';
    END
END

-- Queue entry for Fee Validation (App 2 -> Finance Officer)
IF @AppId2 IS NOT NULL AND @FinOfficerCustId IS NOT NULL AND @RespTypeFee IS NOT NULL AND @StatusSubmitted IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.RoundRobinQueues WHERE RealEstateApplicationId = @AppId2 AND ResponsibilityTypeId = @RespTypeFee AND EndTaskDateTime IS NULL)
    BEGIN
        INSERT INTO dbo.RoundRobinQueues (ClerkId, ResponsibilityTypeId, StatusId, RealEstateApplicationId, CurrentTaskDateTime, IsActive, IsDeleted, CreatedBySystemUserId, CreatedDateTime)
        VALUES (@FinOfficerCustId, @RespTypeFee, @StatusSubmitted, @AppId2, DATEADD(DAY, -3, GETDATE()), 1, 0, @FinOfficerSysId, DATEADD(DAY, -3, GETDATE()));
        PRINT 'Inserted RoundRobinQueue: App2 -> Finance Officer (Fee Validation)';
    END
END

PRINT '--- Demo Application Seeding Complete ---';
GO
