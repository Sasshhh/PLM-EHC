/*
  47-EHC-Release-20261119.sql
  Database changes for the 19 Nov 2026 EHC release (exit inspection + deposit refund per UC031/UC032,
  old/new role names treated as one role).

  Safe to run more than once. Run it in SSMS against the EHC database AFTER deploying the site.
  (PLM-Upgrade-ALL.sql already contains these inserts; run this one if the database was upgraded earlier.)

  What it adds (only if missing):
    Status            s_awaiting_deposit_refund_processing   Awaiting Deposit Refund Processing
    Status            s_awaiting_exit_inspection_slot_selection   Awaiting Applicant to Select Exit Inspection Slots
    Status            s_exit_inspection_scheduled            Exit Inspection Scheduled
    ResponsibilityType r_deposit_refund_processing           Deposit Refund Processing (Client Services Supervisor)
    AppSetting        EDRMSACTIVE = off                      (EHC does not use EDRMS)

  No role changes are needed: CEO = Property Manager, Client Services Officer = Letting Officer and
  Client Services Supervisor = Housing Supervisor are handled in code (Helpers/RoleEquivalence.cs).
  Users must log out and in again after the deploy.

  The last section is a read-only check: every key the new flow needs, and who gets the refund tasks.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @rcs int = (SELECT TOP (1) Id FROM dbo.StatusTypes WHERE [Key] = N'st_rcs');
IF @rcs IS NULL THROW 50047, 'StatusType st_rcs is missing: run PLM-Upgrade-ALL.sql first.', 1;

BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM dbo.Status WHERE [Key] = N's_awaiting_deposit_refund_processing')
    INSERT dbo.Status ([Name], [Description], [Key], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES (N'Awaiting Deposit Refund Processing', N'Tenant moved out; Client Services Supervisor to process the deposit refund deductions',
            N's_awaiting_deposit_refund_processing',
            ISNULL((SELECT TOP (1) StatusTypeId FROM dbo.Status WHERE [Key] = N's_awaiting_refund_response'), @rcs), 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM dbo.ResponsibilityTypes WHERE [Key] = N'r_deposit_refund_processing')
    INSERT dbo.ResponsibilityTypes ([Name], [Description], [Key], IsActive, IsDeleted, CreatedDateTime)
    VALUES (N'Deposit Refund Processing', N'Client Services Supervisor processes the deposit refund / deductions (UC031)', N'r_deposit_refund_processing', 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM dbo.Status WHERE [Key] = N's_awaiting_exit_inspection_slot_selection')
    INSERT dbo.Status ([Name], [Description], [Key], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES (N'Awaiting Applicant to Select Exit Inspection Slots', N'Exit inspection slots published; waiting for the tenant to select one',
            N's_awaiting_exit_inspection_slot_selection',
            ISNULL((SELECT TOP (1) StatusTypeId FROM dbo.Status WHERE [Key] = N's_awaiting_exit_inspec'), @rcs), 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM dbo.Status WHERE [Key] = N's_exit_inspection_scheduled')
    INSERT dbo.Status ([Name], [Description], [Key], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES (N'Exit Inspection Scheduled', N'Tenant selected an exit inspection slot; supervisor to conduct the exit inspection',
            N's_exit_inspection_scheduled',
            ISNULL((SELECT TOP (1) StatusTypeId FROM dbo.Status WHERE [Key] = N's_awaiting_exit_inspec'), @rcs), 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM dbo.AppSettings WHERE [Key] = N'EDRMSACTIVE')
    INSERT dbo.AppSettings ([Name], [Description], [Key], [Value], IsActive, IsDeleted, CreatedDateTime)
    VALUES (N'EDRMSACTIVE', N'on = also push uploaded documents to EDRMS; off = PLM only (EHC default)', N'EDRMSACTIVE', N'off', 1, 0, GETDATE());
UPDATE dbo.AppSettings SET [Value] = N'off' WHERE [Key] = N'EDRMSACTIVE' AND ISNULL([Value], N'') <> N'off';

COMMIT TRANSACTION;
PRINT '47: release 2026-11-19 master data applied.';

/* ---------- Read-only check ---------- */
SELECT k.[Key], k.Kind, CASE WHEN x.Id IS NULL THEN 'MISSING' ELSE 'OK' END AS Result, x.[Name]
FROM (VALUES
    (N's_applicant_vacated_unit', 'Status'), (N's_awaiting_exit_inspec', 'Status'),
    (N's_awaiting_exit_inspection_slot_selection', 'Status'), (N's_exit_inspection_scheduled', 'Status'),
    (N's_awaiting_deposit_refund_processing', 'Status'), (N's_awaiting_refund_response', 'Status'),
    (N's_awaiting_refund_authorisation', 'Status'), (N'l_terminated_lease', 'Status'), (N's_former_tenant', 'Status')
) k([Key], Kind)
LEFT JOIN dbo.Status x ON x.[Key] = k.[Key]
UNION ALL
SELECT k.[Key], 'ResponsibilityType', CASE WHEN r.Id IS NULL THEN 'MISSING' ELSE 'OK' END, r.[Name]
FROM (VALUES (N'r_vacating_confirmation'), (N'r_inspections'), (N'r_shcedule_inspection_slots'),
             (N'r_deposit_refund_processing'), (N'r_awaiting_deposit_refund_response'), (N'r_refund_authorisation')) k([Key])
LEFT JOIN dbo.ResponsibilityTypes r ON r.[Key] = k.[Key]
UNION ALL
SELECT k.[Key], 'Role', CASE WHEN r.Id IS NULL THEN 'MISSING' ELSE 'OK' END, r.[Name]
FROM (VALUES (N'CEO'), (N'Property Manager'), (N'Client Services Officer'), (N'Housing Supervisor'), (N'Revenue Manager')) k([Key])
LEFT JOIN dbo.AspNetRoles r ON r.[Name] = k.[Key];

-- Who gets refund work (value = Customers.Id). The supervisor and CSO come from the tenant's complex first
-- (PreferredComplexAreas.HousingSuperId / LettingOfficerId); these are only the fallbacks.
SELECT s.[Key], s.[Value] AS CustomerId, u.UserName
FROM dbo.AppSettings s
LEFT JOIN dbo.Customers c ON CAST(c.Id AS nvarchar(20)) = s.[Value]
LEFT JOIN dbo.SystemUsers u ON u.Id = c.SystemUserId
WHERE s.[Key] IN (N'u_housing_super_visor', N'u_letting_officer', N'r_revenue_manager', N's_ceo', N'r_property_manager', N'EDRMSACTIVE');
