/*
  25-EHC-Gap-MasterData.sql  (hand-written)

  Rows the EHC code looks up by key that NO seed script in the repo creates.
  Each one was traced to the code path that needs it (see comments). Insert-only:
  if your database already has the key, nothing changes. Run after 20-Upgrade-MasterData.sql.

    sqlcmd -S <server> -E -b -i 25-EHC-Gap-MasterData.sql -v DbName="PropertyLeaseManagementRealEstateFinal"

  Names and email wording below are new (not copied from dev); edit them in the
  admin screens if the business wants different text.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;
USE [$(DbName)];
GO
BEGIN TRANSACTION;

DECLARE @rcs int = (SELECT TOP (1) Id FROM dbo.StatusTypes WHERE [Key] = N'st_rcs');
IF @rcs IS NULL THROW 50020, 'StatusType st_rcs is missing: run 20-Upgrade-MasterData.sql first.', 1;

-- Lease renewal outcome: LeaseDetailsController (renewal approve/reject) -> StatusKeys.LeaseRenewalApproved / Rejected
IF NOT EXISTS (SELECT 1 FROM dbo.Status WHERE [Key] = N's_lease_renewal_approved')
    INSERT dbo.Status ([Name], [Description], [Key], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES (N'Lease Renewal Approved', N'Lease renewal approved', N's_lease_renewal_approved', @rcs, 1, 0, GETDATE());
IF NOT EXISTS (SELECT 1 FROM dbo.Status WHERE [Key] = N's_lease_renewal_rejected')
    INSERT dbo.Status ([Name], [Description], [Key], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES (N'Lease Renewal Rejected', N'Lease renewal rejected', N's_lease_renewal_rejected', @rcs, 1, 0, GETDATE());

-- CEO rejects a risk assessment: RiskAssessmentOutcomesController finishes the round-robin job with ResponsibilityTypes.Key = 'CEO'
IF NOT EXISTS (SELECT 1 FROM dbo.ResponsibilityTypes WHERE [Key] = N'CEO')
    INSERT dbo.ResponsibilityTypes ([Name], [Description], [Key], IsActive, IsDeleted, CreatedDateTime)
    VALUES (N'CEO', N'CEO risk assessment decision', N'CEO', 1, 0, GETDATE());

-- Deposit refund authorisation: PropertyLeaseApplicationController -> ResponsibilityTypeKeys.RefundAuthorisation
IF NOT EXISTS (SELECT 1 FROM dbo.ResponsibilityTypes WHERE [Key] = N'r_refund_authorisation')
    INSERT dbo.ResponsibilityTypes ([Name], [Description], [Key], IsActive, IsDeleted, CreatedDateTime)
    VALUES (N'Refund Authorisation', N'Deposit refund authorisation', N'r_refund_authorisation', 1, 0, GETDATE());

-- Tenant training invite: TenantTrainingController -> ActivityTrackerMessageKeys.TenantInvitedToTraining
IF NOT EXISTS (SELECT 1 FROM dbo.ActivityTrackerMessages WHERE [Key] = N'at_tenant_invited_to_training')
    INSERT dbo.ActivityTrackerMessages ([Name], [Description], [Key], IsActive, IsDeleted, CreatedDateTime)
    VALUES (N'Tenant Invited To Training', N'Tenant invited to complete the online pre-tenancy training', N'at_tenant_invited_to_training', 1, 0, GETDATE());

-- Risk assessment rejected on ITC: RiskAssessmentOutcomesController -> EmailContentKeys.RiskAssessmentRejectITC
IF NOT EXISTS (SELECT 1 FROM dbo.EmailContentTypes WHERE [Key] = N'plm_on_risk_reject_itc')
    INSERT dbo.EmailContentTypes ([Name], [Description], [Key], IsActive, IsDeleted, CreatedDateTime)
    VALUES (N'Message Sent From BO To Applicant',
            N'Your application with the reference number {0} has been rejected for risk assessment due to the credit (ITC) check. Reason: {1}',
            N'plm_on_risk_reject_itc', 1, 0, GETDATE());

-- Termination review "Legal Referral" option: PropertyLeaseApplicationController -> RCSActionTypeKeys.LegalReferral
IF NOT EXISTS (SELECT 1 FROM dbo.RCSActionTypes WHERE [Key] = N'plm_legal_referral')
    INSERT dbo.RCSActionTypes ([Name], [Description], [Key], IsActive, IsDeleted, CreatedDateTime)
    VALUES (N'Legal Referral', N'Refer to Legal', N'plm_legal_referral', 1, 0, GETDATE());

-- Water meter reading upload: CaptureController / DepartmentsApprovalsController -> DocumentTypeKeys.WaterMeterReading
IF NOT EXISTS (SELECT 1 FROM dbo.DocumentTypes WHERE [Key] = N'dt_water_meter_reading')
    INSERT dbo.DocumentTypes ([Name], [Description], [Key], IsActive, IsDeleted, CreatedDateTime)
    VALUES (N'Water Meter Reading', N'Water meter reading', N'dt_water_meter_reading', 1, 0, GETDATE());

COMMIT TRANSACTION;
PRINT 'EHC gap master data applied.';
