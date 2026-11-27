/* 52 - EHC application: "Spouse" becomes "Co-Tenant" (working / not working + co-tenant affidavit)
   ----------------------------------------------------------------------------------------------
   Agreed with the BA in the DPRE FRS clarity meeting of 26 Nov 2026. An updated EHC submission
   use case will follow; this script covers what was agreed in the meeting.

   What it does (safe to run more than once):
     1. Adds column SecAppIsWorking (bit NULL) to dbo.PropertyLeaseApplications and
        dbo.PropertyLeaseApplicationAudits.  1 = co-tenant working, 0 = not working, NULL = no co-tenant.
     2. Adds document type dt_co_tenant_affidavit ("Co-Tenant Affidavit / Supporting Document").
     3. Adds its document checklist row next to the existing applicant Affidavit row
        (same Application, Reference Type and Department), so it shows on the
        EHC application Document Upload step when the application has a co-tenant.

   Run on the EHC database BEFORE deploying the build that contains the co-tenant change
   (the new build reads PropertyLeaseApplications.SecAppIsWorking).
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRAN;

/* 1. Columns ------------------------------------------------------------------------------- */
IF COL_LENGTH(N'dbo.PropertyLeaseApplications', N'SecAppIsWorking') IS NULL
BEGIN
    ALTER TABLE dbo.PropertyLeaseApplications ADD SecAppIsWorking bit NULL;
    PRINT 'Added column PropertyLeaseApplications.SecAppIsWorking';
END
ELSE PRINT 'Exists column PropertyLeaseApplications.SecAppIsWorking';

IF OBJECT_ID(N'dbo.PropertyLeaseApplicationAudits', 'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.PropertyLeaseApplicationAudits', N'SecAppIsWorking') IS NULL
BEGIN
    ALTER TABLE dbo.PropertyLeaseApplicationAudits ADD SecAppIsWorking bit NULL;
    PRINT 'Added column PropertyLeaseApplicationAudits.SecAppIsWorking';
END
ELSE PRINT 'Exists column PropertyLeaseApplicationAudits.SecAppIsWorking (or no audit table)';

/* 2. Document type ------------------------------------------------------------------------- */
DECLARE @DocTypeId INT = (SELECT TOP 1 Id FROM dbo.DocumentTypes WHERE [Key] = N'dt_co_tenant_affidavit');
DECLARE @AffidavitTypeId INT = (SELECT TOP 1 Id FROM dbo.DocumentTypes WHERE [Key] = N'dt_affidavit_document');

IF @DocTypeId IS NULL
BEGIN
    INSERT INTO dbo.DocumentTypes ([Name], [Description], [Key], DepartmentId, IsActive, IsDeleted, IsLocked,
                                   CreatedBySystemUserId, CreatedDateTime, ModifiedBySystemUserId, ModifiedDateTime)
    SELECT N'Co-Tenant Affidavit / Supporting Document',
           N'Required when the co-tenant is not working: signed affidavit confirming the co-tenant is not working and how they are supported. If the co-tenant is working, upload the co-tenant''s payslip or proof of income here.',
           N'dt_co_tenant_affidavit',
           a.DepartmentId, 1, 0, 0, a.CreatedBySystemUserId, GETDATE(), a.CreatedBySystemUserId, GETDATE()
    FROM (SELECT 1 AS x) d
    LEFT JOIN dbo.DocumentTypes a ON a.Id = @AffidavitTypeId;
    SET @DocTypeId = SCOPE_IDENTITY();
    PRINT 'Added DocumentType dt_co_tenant_affidavit (Id ' + CAST(@DocTypeId AS varchar(10)) + ')';
END
ELSE
BEGIN
    UPDATE dbo.DocumentTypes SET IsActive = 1, IsDeleted = 0 WHERE Id = @DocTypeId;
    PRINT 'Exists DocumentType dt_co_tenant_affidavit (Id ' + CAST(@DocTypeId AS varchar(10)) + ')';
END

/* 3. Checklist row: copy Application / ReferenceType / Department from the applicant Affidavit row
      used by the EHC application upload (reference type rt_upload_rcs). */
DECLARE @RefTypeId INT = (SELECT TOP 1 Id FROM dbo.ReferenceTypes WHERE [Key] = N'rt_upload_rcs');
IF @RefTypeId IS NULL BEGIN RAISERROR('Reference type rt_upload_rcs not found.', 16, 1); ROLLBACK; RETURN; END

DECLARE @AppId INT, @DeptId INT, @UserId INT;
SELECT TOP 1 @AppId = c.ApplicationId, @DeptId = c.DepartmentId, @UserId = c.CreatedBySystemUserId
FROM dbo.DocumentCheckLists c
WHERE c.DocumentTypeId = @AffidavitTypeId AND c.ReferenceTypeId = @RefTypeId AND c.IsActive = 1 AND c.IsDeleted = 0
ORDER BY c.Id;

IF @AppId IS NULL
    SELECT @AppId = Id FROM dbo.Applications WHERE [Key] = N'a_rates_clearance_system';
IF @AppId IS NULL BEGIN RAISERROR('Could not resolve the Application for the checklist row.', 16, 1); ROLLBACK; RETURN; END

IF NOT EXISTS (SELECT 1 FROM dbo.DocumentCheckLists WHERE DocumentTypeId = @DocTypeId AND ReferenceTypeId = @RefTypeId)
BEGIN
    INSERT INTO dbo.DocumentCheckLists (DocumentTypeId, ApplicationId, ReferenceTypeId, DepartmentId, IsActive, IsDeleted, IsLocked,
                                        CreatedBySystemUserId, CreatedDateTime, ModifiedBySystemUserId, ModifiedDateTime)
    VALUES (@DocTypeId, @AppId, @RefTypeId, @DeptId, 1, 0, 0, @UserId, GETDATE(), @UserId, GETDATE());
    PRINT 'Added DocumentCheckList row for dt_co_tenant_affidavit';
END
ELSE
BEGIN
    UPDATE dbo.DocumentCheckLists SET IsActive = 1, IsDeleted = 0
    WHERE DocumentTypeId = @DocTypeId AND ReferenceTypeId = @RefTypeId;
    PRINT 'Exists DocumentCheckList row for dt_co_tenant_affidavit';
END

COMMIT;

/* Check */
SELECT t.Id AS DocumentTypeId, t.[Key], t.[Name], c.Id AS CheckListId, c.ApplicationId, c.ReferenceTypeId, c.DepartmentId
FROM dbo.DocumentTypes t
JOIN dbo.DocumentCheckLists c ON c.DocumentTypeId = t.Id
WHERE t.[Key] IN (N'dt_affidavit_document', N'dt_co_tenant_affidavit') AND c.ReferenceTypeId = @RefTypeId;

SELECT COL_LENGTH(N'dbo.PropertyLeaseApplications', N'SecAppIsWorking') AS PLA_SecAppIsWorking_Bytes,
       COL_LENGTH(N'dbo.PropertyLeaseApplicationAudits', N'SecAppIsWorking') AS Audit_SecAppIsWorking_Bytes;
