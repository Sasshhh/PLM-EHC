-- =============================================
-- ADD RENEWAL-SPECIFIC DOCUMENT TYPES & CHECKLISTS
-- Purpose : Prevent collision between original PLM application documents
--           and renewal-cycle documents uploaded by the tenant.
--           The original upload flow (DocumentCaptureApplication) and the
--           renewal upload flow (DocumentCaptureTenantLease) previously shared
--           the same DocumentCheckList IDs, causing original docs to appear in
--           the renewal upload page and be deletable by tenants during renewal.
-- Run on  : PropertyLeaseManagementPreGoLive (and any lower env)
-- Author  : Auto-generated 2026-05-17
-- =============================================

USE [PropertyLeaseManagementPreGoLive]
GO

SET NOCOUNT ON
GO

PRINT '=========================================='
PRINT 'Setting Up Renewal Document Types & Checklists'
PRINT '=========================================='
PRINT ''

-- ── Resolve parent IDs ──────────────────────────────────────────────────────
DECLARE @ApplicationId    INT
DECLARE @ReferenceTypeId  INT

SELECT @ApplicationId   = Id FROM Applications   WHERE [Key] = 'a_rates_clearance_system'
SELECT @ReferenceTypeId = Id FROM ReferenceTypes  WHERE [Key] = 'rt_upload_rcs'

PRINT 'Application ID     : ' + CAST(ISNULL(@ApplicationId,   0) AS VARCHAR(10))
PRINT 'Reference Type ID  : ' + CAST(ISNULL(@ReferenceTypeId, 0) AS VARCHAR(10))
PRINT ''

IF @ApplicationId IS NULL OR @ReferenceTypeId IS NULL
BEGIN
    RAISERROR('ERROR: Could not resolve Application or ReferenceType. Aborting.', 16, 1)
    RETURN
END

-- ── Helper: ensure DocumentType + DocumentCheckList ─────────────────────────
-- 1. ID Document (Renewal)
DECLARE @IdDocRenewalId INT
SELECT  @IdDocRenewalId = Id FROM DocumentTypes WHERE [Key] = 'dt_renewal_identity_document'
IF @IdDocRenewalId IS NULL
BEGIN
    INSERT INTO DocumentTypes ([Key],[Name],[Description],IsActive,IsDeleted,CreatedDateTime,ModifiedDateTime,CreatedBySystemUserId)
    VALUES ('dt_renewal_identity_document','ID Document (Renewal)','Copy of ID Document - submitted during lease renewal',1,0,GETDATE(),GETDATE(),1)
    SET @IdDocRenewalId = SCOPE_IDENTITY()
    PRINT 'Added DocumentType : dt_renewal_identity_document'
END ELSE PRINT 'Exists DocumentType: dt_renewal_identity_document (ID: ' + CAST(@IdDocRenewalId AS VARCHAR) + ')'

IF NOT EXISTS (SELECT 1 FROM DocumentCheckLists WHERE DocumentTypeId = @IdDocRenewalId AND ReferenceTypeId = @ReferenceTypeId AND ApplicationId = @ApplicationId)
BEGIN
    INSERT INTO DocumentCheckLists (DocumentTypeId,ApplicationId,ReferenceTypeId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime)
    VALUES (@IdDocRenewalId,@ApplicationId,@ReferenceTypeId,1,0,0,1,GETDATE(),1,GETDATE())
    PRINT 'Added CheckList    : dt_renewal_identity_document'
END ELSE PRINT 'Exists CheckList   : dt_renewal_identity_document'

-- 2. Proof of Income (Renewal)
DECLARE @IncomeRenewalId INT
SELECT  @IncomeRenewalId = Id FROM DocumentTypes WHERE [Key] = 'dt_renewal_proof_of_income'
IF @IncomeRenewalId IS NULL
BEGIN
    INSERT INTO DocumentTypes ([Key],[Name],[Description],IsActive,IsDeleted,CreatedDateTime,ModifiedDateTime,CreatedBySystemUserId)
    VALUES ('dt_renewal_proof_of_income','Proof of Income (Renewal)','Payslip / Pension / Grant - submitted during lease renewal',1,0,GETDATE(),GETDATE(),1)
    SET @IncomeRenewalId = SCOPE_IDENTITY()
    PRINT 'Added DocumentType : dt_renewal_proof_of_income'
END ELSE PRINT 'Exists DocumentType: dt_renewal_proof_of_income (ID: ' + CAST(@IncomeRenewalId AS VARCHAR) + ')'

IF NOT EXISTS (SELECT 1 FROM DocumentCheckLists WHERE DocumentTypeId = @IncomeRenewalId AND ReferenceTypeId = @ReferenceTypeId AND ApplicationId = @ApplicationId)
BEGIN
    INSERT INTO DocumentCheckLists (DocumentTypeId,ApplicationId,ReferenceTypeId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime)
    VALUES (@IncomeRenewalId,@ApplicationId,@ReferenceTypeId,1,0,0,1,GETDATE(),1,GETDATE())
    PRINT 'Added CheckList    : dt_renewal_proof_of_income'
END ELSE PRINT 'Exists CheckList   : dt_renewal_proof_of_income'

-- 3. Bank Statement (Renewal)
DECLARE @BankRenewalId INT
SELECT  @BankRenewalId = Id FROM DocumentTypes WHERE [Key] = 'dt_renewal_bank_statement'
IF @BankRenewalId IS NULL
BEGIN
    INSERT INTO DocumentTypes ([Key],[Name],[Description],IsActive,IsDeleted,CreatedDateTime,ModifiedDateTime,CreatedBySystemUserId)
    VALUES ('dt_renewal_bank_statement','Bank Statement (Renewal)','Bank statement - submitted during lease renewal',1,0,GETDATE(),GETDATE(),1)
    SET @BankRenewalId = SCOPE_IDENTITY()
    PRINT 'Added DocumentType : dt_renewal_bank_statement'
END ELSE PRINT 'Exists DocumentType: dt_renewal_bank_statement (ID: ' + CAST(@BankRenewalId AS VARCHAR) + ')'

IF NOT EXISTS (SELECT 1 FROM DocumentCheckLists WHERE DocumentTypeId = @BankRenewalId AND ReferenceTypeId = @ReferenceTypeId AND ApplicationId = @ApplicationId)
BEGIN
    INSERT INTO DocumentCheckLists (DocumentTypeId,ApplicationId,ReferenceTypeId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime)
    VALUES (@BankRenewalId,@ApplicationId,@ReferenceTypeId,1,0,0,1,GETDATE(),1,GETDATE())
    PRINT 'Added CheckList    : dt_renewal_bank_statement'
END ELSE PRINT 'Exists CheckList   : dt_renewal_bank_statement'

-- 4. Proof of Employment (Renewal)
DECLARE @EmploymentRenewalId INT
SELECT  @EmploymentRenewalId = Id FROM DocumentTypes WHERE [Key] = 'dt_renewal_proof_of_employment'
IF @EmploymentRenewalId IS NULL
BEGIN
    INSERT INTO DocumentTypes ([Key],[Name],[Description],IsActive,IsDeleted,CreatedDateTime,ModifiedDateTime,CreatedBySystemUserId)
    VALUES ('dt_renewal_proof_of_employment','Proof of Employment (Renewal)','Proof of employment - submitted during lease renewal',1,0,GETDATE(),GETDATE(),1)
    SET @EmploymentRenewalId = SCOPE_IDENTITY()
    PRINT 'Added DocumentType : dt_renewal_proof_of_employment'
END ELSE PRINT 'Exists DocumentType: dt_renewal_proof_of_employment (ID: ' + CAST(@EmploymentRenewalId AS VARCHAR) + ')'

IF NOT EXISTS (SELECT 1 FROM DocumentCheckLists WHERE DocumentTypeId = @EmploymentRenewalId AND ReferenceTypeId = @ReferenceTypeId AND ApplicationId = @ApplicationId)
BEGIN
    INSERT INTO DocumentCheckLists (DocumentTypeId,ApplicationId,ReferenceTypeId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime)
    VALUES (@EmploymentRenewalId,@ApplicationId,@ReferenceTypeId,1,0,0,1,GETDATE(),1,GETDATE())
    PRINT 'Added CheckList    : dt_renewal_proof_of_employment'
END ELSE PRINT 'Exists CheckList   : dt_renewal_proof_of_employment'

-- 5. Affidavit (Renewal)
DECLARE @AffidavitRenewalId INT
SELECT  @AffidavitRenewalId = Id FROM DocumentTypes WHERE [Key] = 'dt_renewal_affidavit'
IF @AffidavitRenewalId IS NULL
BEGIN
    INSERT INTO DocumentTypes ([Key],[Name],[Description],IsActive,IsDeleted,CreatedDateTime,ModifiedDateTime,CreatedBySystemUserId)
    VALUES ('dt_renewal_affidavit','Affidavit (Renewal)','Affidavit document - submitted during lease renewal',1,0,GETDATE(),GETDATE(),1)
    SET @AffidavitRenewalId = SCOPE_IDENTITY()
    PRINT 'Added DocumentType : dt_renewal_affidavit'
END ELSE PRINT 'Exists DocumentType: dt_renewal_affidavit (ID: ' + CAST(@AffidavitRenewalId AS VARCHAR) + ')'

IF NOT EXISTS (SELECT 1 FROM DocumentCheckLists WHERE DocumentTypeId = @AffidavitRenewalId AND ReferenceTypeId = @ReferenceTypeId AND ApplicationId = @ApplicationId)
BEGIN
    INSERT INTO DocumentCheckLists (DocumentTypeId,ApplicationId,ReferenceTypeId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime)
    VALUES (@AffidavitRenewalId,@ApplicationId,@ReferenceTypeId,1,0,0,1,GETDATE(),1,GETDATE())
    PRINT 'Added CheckList    : dt_renewal_affidavit'
END ELSE PRINT 'Exists CheckList   : dt_renewal_affidavit'

-- 6. Proof of Address (Renewal)
DECLARE @AddressRenewalId INT
SELECT  @AddressRenewalId = Id FROM DocumentTypes WHERE [Key] = 'dt_renewal_proof_of_address'
IF @AddressRenewalId IS NULL
BEGIN
    INSERT INTO DocumentTypes ([Key],[Name],[Description],IsActive,IsDeleted,CreatedDateTime,ModifiedDateTime,CreatedBySystemUserId)
    VALUES ('dt_renewal_proof_of_address','Proof of Address (Renewal)','Proof of address - submitted during lease renewal',1,0,GETDATE(),GETDATE(),1)
    SET @AddressRenewalId = SCOPE_IDENTITY()
    PRINT 'Added DocumentType : dt_renewal_proof_of_address'
END ELSE PRINT 'Exists DocumentType: dt_renewal_proof_of_address (ID: ' + CAST(@AddressRenewalId AS VARCHAR) + ')'

IF NOT EXISTS (SELECT 1 FROM DocumentCheckLists WHERE DocumentTypeId = @AddressRenewalId AND ReferenceTypeId = @ReferenceTypeId AND ApplicationId = @ApplicationId)
BEGIN
    INSERT INTO DocumentCheckLists (DocumentTypeId,ApplicationId,ReferenceTypeId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime)
    VALUES (@AddressRenewalId,@ApplicationId,@ReferenceTypeId,1,0,0,1,GETDATE(),1,GETDATE())
    PRINT 'Added CheckList    : dt_renewal_proof_of_address'
END ELSE PRINT 'Exists CheckList   : dt_renewal_proof_of_address'

-- ── Verification ─────────────────────────────────────────────────────────────
PRINT ''
PRINT '=========================================='
PRINT 'VERIFICATION'
PRINT '=========================================='

SELECT
    dt.Id           AS DocTypeId,
    dt.[Key]        AS DocTypeKey,
    dt.[Name]       AS DocTypeName,
    dcl.Id          AS CheckListId,
    a.[Key]         AS ApplicationKey,
    rt.[Key]        AS ReferenceTypeKey
FROM DocumentTypes dt
JOIN DocumentCheckLists dcl ON dcl.DocumentTypeId = dt.Id
JOIN Applications       a   ON a.Id = dcl.ApplicationId
JOIN ReferenceTypes     rt  ON rt.Id = dcl.ReferenceTypeId
WHERE dt.[Key] IN (
    'dt_renewal_identity_document',
    'dt_renewal_proof_of_income',
    'dt_renewal_bank_statement',
    'dt_renewal_proof_of_employment',
    'dt_renewal_affidavit',
    'dt_renewal_proof_of_address'
)

PRINT ''
PRINT 'DONE: Renewal document types and checklists configured.'

SET NOCOUNT OFF
GO
