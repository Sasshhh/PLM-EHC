-- ==============================================================================
-- PLM FULL PRODUCTION DEPLOYMENT — COMPLETE METADATA SEED
-- ==============================================================================
-- Source: Verified against CRMPLMDEV_2025 local database 2026-05-18
-- All statements idempotent (safe to re-run). Inserts ALL fields.
-- Change the USE statement below to your production database name.
-- ==============================================================================
USE [PropertyLeaseManagement]   -- ← UPDATE TO YOUR PROD DB NAME
GO
SET NOCOUNT ON
GO

PRINT '==================================================================='
PRINT 'PLM FULL PRODUCTION DEPLOYMENT — START'
PRINT 'Execution Time: ' + CONVERT(VARCHAR, GETDATE(), 120)
PRINT '==================================================================='

-- ==============================================================
-- SECTION 1: STATUSES (21 rows)
-- Covers: all renewal workflow, signing, timeouts, termination
-- ==============================================================
PRINT ''
PRINT '--- Section 1: Statuses ---'

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_awaiting_agreement_renewal__')
    INSERT INTO Status ([Key],[Name],[Description],[StatusTypeId],IsActive,IsDeleted,CreatedDateTime)
    VALUES ('s_awaiting_agreement_renewal__','In Awaiting Agreement of Lease Renewal','In Awaiting Agreement of Lease Renewal',1,1,0,GETDATE())
ELSE UPDATE Status SET [Name]='In Awaiting Agreement of Lease Renewal',[Description]='In Awaiting Agreement of Lease Renewal',[StatusTypeId]=1,IsActive=1,IsDeleted=0 WHERE [Key]='s_awaiting_agreement_renewal__'
PRINT '  s_awaiting_agreement_renewal__'

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_awaiting_application_lease_renrewal')
    INSERT INTO Status ([Key],[Name],[Description],[StatusTypeId],IsActive,IsDeleted,CreatedDateTime)
    VALUES ('s_awaiting_application_lease_renrewal','In Awaiting Application Lease Renewal Review','In Awaiting Application Lease Renewal Review',1,1,0,GETDATE())
ELSE UPDATE Status SET [Name]='In Awaiting Application Lease Renewal Review',[Description]='In Awaiting Application Lease Renewal Review',[StatusTypeId]=1,IsActive=1,IsDeleted=0 WHERE [Key]='s_awaiting_application_lease_renrewal'
PRINT '  s_awaiting_application_lease_renrewal'

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_awaiting_renewal_agreement_conclusion')
    INSERT INTO Status ([Key],[Name],[Description],[StatusTypeId],IsActive,IsDeleted,CreatedDateTime)
    VALUES ('s_awaiting_renewal_agreement_conclusion','Awaiting Lease Renewal Agreement Conclusion','Awaiting Lease Renewal Agreement Conclusion',1,1,0,GETDATE())
ELSE UPDATE Status SET [Name]='Awaiting Lease Renewal Agreement Conclusion',[Description]='Awaiting Lease Renewal Agreement Conclusion',[StatusTypeId]=1,IsActive=1,IsDeleted=0 WHERE [Key]='s_awaiting_renewal_agreement_conclusion'
PRINT '  s_awaiting_renewal_agreement_conclusion'

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_awaiting_renewal_agreement_generation')
    INSERT INTO Status ([Key],[Name],[Description],[StatusTypeId],IsActive,IsDeleted,CreatedDateTime)
    VALUES ('s_awaiting_renewal_agreement_generation','Awaiting Renewal Agreement Generation','Awaiting Renewal Agreement Generation',20,1,0,GETDATE())
ELSE UPDATE Status SET [Name]='Awaiting Renewal Agreement Generation',[Description]='Awaiting Renewal Agreement Generation',[StatusTypeId]=20,IsActive=1,IsDeleted=0 WHERE [Key]='s_awaiting_renewal_agreement_generation'
PRINT '  s_awaiting_renewal_agreement_generation'

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_awaiting_renewal_ceo_approval')
    INSERT INTO Status ([Key],[Name],[Description],[StatusTypeId],IsActive,IsDeleted,CreatedDateTime)
    VALUES ('s_awaiting_renewal_ceo_approval','Awaiting Renewal CEO Approval','Awaiting Renewal CEO Approval',20,1,0,GETDATE())
ELSE UPDATE Status SET [Name]='Awaiting Renewal CEO Approval',[Description]='Awaiting Renewal CEO Approval',[StatusTypeId]=20,IsActive=1,IsDeleted=0 WHERE [Key]='s_awaiting_renewal_ceo_approval'
PRINT '  s_awaiting_renewal_ceo_approval'

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_awaiting_renewal_ceo_signature')
    INSERT INTO Status ([Key],[Name],[Description],[StatusTypeId],IsActive,IsDeleted,CreatedDateTime)
    VALUES ('s_awaiting_renewal_ceo_signature','Awaiting Renewal CEO Signature','Lease Renewal is awaiting signature by the Property Manager (CEO)',20,1,0,GETDATE())
ELSE UPDATE Status SET [Name]='Awaiting Renewal CEO Signature',[Description]='Lease Renewal is awaiting signature by the Property Manager (CEO)',[StatusTypeId]=20,IsActive=1,IsDeleted=0 WHERE [Key]='s_awaiting_renewal_ceo_signature'
PRINT '  s_awaiting_renewal_ceo_signature'

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_awaiting_renewal_docs')
    INSERT INTO Status ([Key],[Name],[Description],[StatusTypeId],IsActive,IsDeleted,CreatedDateTime)
    VALUES ('s_awaiting_renewal_docs','In Awaiting Renewal Document(s) Upload','Awaiting Renewal Document(s) Upload',1,1,0,GETDATE())
ELSE UPDATE Status SET [Name]='In Awaiting Renewal Document(s) Upload',[Description]='Awaiting Renewal Document(s) Upload',[StatusTypeId]=1,IsActive=1,IsDeleted=0 WHERE [Key]='s_awaiting_renewal_docs'
PRINT '  s_awaiting_renewal_docs'

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_awaiting_renewal_lease_capture')
    INSERT INTO Status ([Key],[Name],[Description],[StatusTypeId],IsActive,IsDeleted,CreatedDateTime)
    VALUES ('s_awaiting_renewal_lease_capture','Awaiting Renewal Lease Capture','Awaiting Renewal Lease Capture',20,1,0,GETDATE())
ELSE UPDATE Status SET [Name]='Awaiting Renewal Lease Capture',[Description]='Awaiting Renewal Lease Capture',[StatusTypeId]=20,IsActive=1,IsDeleted=0 WHERE [Key]='s_awaiting_renewal_lease_capture'
PRINT '  s_awaiting_renewal_lease_capture'

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_awaiting_renewal_outcome')
    INSERT INTO Status ([Key],[Name],[Description],[StatusTypeId],IsActive,IsDeleted,CreatedDateTime)
    VALUES ('s_awaiting_renewal_outcome','Awaiting Renewal Outcome','Awaiting Renewal Outcome',1,1,0,GETDATE())
ELSE UPDATE Status SET [Name]='Awaiting Renewal Outcome',[Description]='Awaiting Renewal Outcome',[StatusTypeId]=1,IsActive=1,IsDeleted=0 WHERE [Key]='s_awaiting_renewal_outcome'
PRINT '  s_awaiting_renewal_outcome'

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_awaiting_renewal_review_outcome')
    INSERT INTO Status ([Key],[Name],[Description],[StatusTypeId],IsActive,IsDeleted,CreatedDateTime)
    VALUES ('s_awaiting_renewal_review_outcome','Awaiting Renewal Review Outcome','Awaiting Renewal Review Outcome',1,1,0,GETDATE())
ELSE UPDATE Status SET [Name]='Awaiting Renewal Review Outcome',[Description]='Awaiting Renewal Review Outcome',[StatusTypeId]=1,IsActive=1,IsDeleted=0 WHERE [Key]='s_awaiting_renewal_review_outcome'
PRINT '  s_awaiting_renewal_review_outcome'

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_awaiting_renewal_rm_signature')
    INSERT INTO Status ([Key],[Name],[Description],[StatusTypeId],IsActive,IsDeleted,CreatedDateTime)
    VALUES ('s_awaiting_renewal_rm_signature','Awaiting Renewal RM Signature','Awaiting Renewal RM Signature',20,1,0,GETDATE())
ELSE UPDATE Status SET [Name]='Awaiting Renewal RM Signature',[Description]='Awaiting Renewal RM Signature',[StatusTypeId]=20,IsActive=1,IsDeleted=0 WHERE [Key]='s_awaiting_renewal_rm_signature'
PRINT '  s_awaiting_renewal_rm_signature'

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_awaiting_renewal_tenant_lease')
    INSERT INTO Status ([Key],[Name],[Description],[StatusTypeId],IsActive,IsDeleted,CreatedDateTime)
    VALUES ('s_awaiting_renewal_tenant_lease','In Awaiting Lease Renewal','Awaiting Lease Renewal',1,1,0,GETDATE())
ELSE UPDATE Status SET [Name]='In Awaiting Lease Renewal',[Description]='Awaiting Lease Renewal',[StatusTypeId]=1,IsActive=1,IsDeleted=0 WHERE [Key]='s_awaiting_renewal_tenant_lease'
PRINT '  s_awaiting_renewal_tenant_lease'

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_awaiting_renewal_tenant_signature')
    INSERT INTO Status ([Key],[Name],[Description],[StatusTypeId],IsActive,IsDeleted,CreatedDateTime)
    VALUES ('s_awaiting_renewal_tenant_signature','Awaiting Renewal Tenant Signature','Awaiting Renewal Tenant Signature',20,1,0,GETDATE())
ELSE UPDATE Status SET [Name]='Awaiting Renewal Tenant Signature',[Description]='Awaiting Renewal Tenant Signature',[StatusTypeId]=20,IsActive=1,IsDeleted=0 WHERE [Key]='s_awaiting_renewal_tenant_signature'
PRINT '  s_awaiting_renewal_tenant_signature'

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_deactivated_lease')
    INSERT INTO Status ([Key],[Name],[Description],[StatusTypeId],IsActive,IsDeleted,CreatedDateTime)
    VALUES ('s_deactivated_lease','Deactivated New Lease Captured','Deactivated New Lease Captured',1,1,0,GETDATE())
ELSE UPDATE Status SET [Name]='Deactivated New Lease Captured',[Description]='Deactivated New Lease Captured',[StatusTypeId]=1,IsActive=1,IsDeleted=0 WHERE [Key]='s_deactivated_lease'
PRINT '  s_deactivated_lease'

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_lease_renewal_agreement_concluded')
    INSERT INTO Status ([Key],[Name],[Description],[StatusTypeId],IsActive,IsDeleted,CreatedDateTime)
    VALUES ('s_lease_renewal_agreement_concluded','Lease Renewal Agreement Concluded','Lease Renewal Agreement Concluded',20,1,0,GETDATE())
ELSE UPDATE Status SET [Name]='Lease Renewal Agreement Concluded',[Description]='Lease Renewal Agreement Concluded',[StatusTypeId]=20,IsActive=1,IsDeleted=0 WHERE [Key]='s_lease_renewal_agreement_concluded'
PRINT '  s_lease_renewal_agreement_concluded'

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_lease_signing_expired')
    INSERT INTO Status ([Key],[Name],[Description],[StatusTypeId],IsActive,IsDeleted,CreatedDateTime)
    VALUES ('s_lease_signing_expired','Lease Signing Expired (30-day)','BR19: Lease signing deadline expired - application disregarded after 30 days.',1,1,0,GETDATE())
ELSE UPDATE Status SET [Name]='Lease Signing Expired (30-day)',[Description]='BR19: Lease signing deadline expired - application disregarded after 30 days.',[StatusTypeId]=1,IsActive=1,IsDeleted=0 WHERE [Key]='s_lease_signing_expired'
PRINT '  s_lease_signing_expired'

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_lease_terminate_complaints')
    INSERT INTO Status ([Key],[Name],[Description],[StatusTypeId],IsActive,IsDeleted,CreatedDateTime)
    VALUES ('s_lease_terminate_complaints','Lease Terminated Due To Complaints','Lease Terminated Due To Complaints',1,1,0,GETDATE())
ELSE UPDATE Status SET [Name]='Lease Terminated Due To Complaints',[Description]='Lease Terminated Due To Complaints',[StatusTypeId]=1,IsActive=1,IsDeleted=0 WHERE [Key]='s_lease_terminate_complaints'
PRINT '  s_lease_terminate_complaints'

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_lease_terminated_due_to_complaints')
    INSERT INTO Status ([Key],[Name],[Description],[StatusTypeId],IsActive,IsDeleted,CreatedDateTime)
    VALUES ('s_lease_terminated_due_to_complaints','Lease Terminated Due To Complaints','Lease Terminated Due To Complaints',1,1,0,GETDATE())
ELSE UPDATE Status SET [Name]='Lease Terminated Due To Complaints',[Description]='Lease Terminated Due To Complaints',[StatusTypeId]=1,IsActive=1,IsDeleted=0 WHERE [Key]='s_lease_terminated_due_to_complaints'
PRINT '  s_lease_terminated_due_to_complaints'

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_renewal_agree_success')
    INSERT INTO Status ([Key],[Name],[Description],[StatusTypeId],IsActive,IsDeleted,CreatedDateTime)
    VALUES ('s_renewal_agree_success','Agreement Of Lease Renewed Successfully','Agreement Of Lease Renewed Successfully',1,1,0,GETDATE())
ELSE UPDATE Status SET [Name]='Agreement Of Lease Renewed Successfully',[Description]='Agreement Of Lease Renewed Successfully',[StatusTypeId]=1,IsActive=1,IsDeleted=0 WHERE [Key]='s_renewal_agree_success'
PRINT '  s_renewal_agree_success'

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_renewal_documents_uploaded')
    INSERT INTO Status ([Key],[Name],[Description],[StatusTypeId],IsActive,IsDeleted,CreatedDateTime)
    VALUES ('s_renewal_documents_uploaded','Agreement Renewal Documents Uploaded Successfully','Agreement Renewal Documents Uploaded Successfully',1,1,0,GETDATE())
ELSE UPDATE Status SET [Name]='Agreement Renewal Documents Uploaded Successfully',[Description]='Agreement Renewal Documents Uploaded Successfully',[StatusTypeId]=1,IsActive=1,IsDeleted=0 WHERE [Key]='s_renewal_documents_uploaded'
PRINT '  s_renewal_documents_uploaded'

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_unit_offer_expired')
    INSERT INTO Status ([Key],[Name],[Description],[StatusTypeId],IsActive,IsDeleted,CreatedDateTime)
    VALUES ('s_unit_offer_expired','Unit Offer Expired (30-day)','BR09: Unit offer expired - applicant did not accept within 30 days. Re-listed on waiting list.',1,1,0,GETDATE())
ELSE UPDATE Status SET [Name]='Unit Offer Expired (30-day)',[Description]='BR09: Unit offer expired - applicant did not accept within 30 days. Re-listed on waiting list.',[StatusTypeId]=1,IsActive=1,IsDeleted=0 WHERE [Key]='s_unit_offer_expired'
PRINT '  s_unit_offer_expired'

-- ==============================================================
-- SECTION 2: RESPONSIBILITY TYPES (14 rows)
-- ==============================================================
PRINT ''
PRINT '--- Section 2: Responsibility Types ---'

IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key]='r_agreement_renewal_2nd_recomm')
    INSERT INTO ResponsibilityTypes([Key],[Name],IsActive,IsDeleted) VALUES('r_agreement_renewal_2nd_recomm','Review Lease Renewal Recommendation',1,0)
ELSE UPDATE ResponsibilityTypes SET [Name]='Review Lease Renewal Recommendation',IsActive=1,IsDeleted=0 WHERE [Key]='r_agreement_renewal_2nd_recomm'
PRINT '  r_agreement_renewal_2nd_recomm'

IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key]='r_agreement_renewal_acceptance')
    INSERT INTO ResponsibilityTypes([Key],[Name],IsActive,IsDeleted) VALUES('r_agreement_renewal_acceptance','Accept Lease Renewal',1,0)
ELSE UPDATE ResponsibilityTypes SET [Name]='Accept Lease Renewal',IsActive=1,IsDeleted=0 WHERE [Key]='r_agreement_renewal_acceptance'
PRINT '  r_agreement_renewal_acceptance'

IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key]='r_agreement_renewal_review')
    INSERT INTO ResponsibilityTypes([Key],[Name],IsActive,IsDeleted) VALUES('r_agreement_renewal_review','Approve Lease Renewal',1,0)
ELSE UPDATE ResponsibilityTypes SET [Name]='Approve Lease Renewal',IsActive=1,IsDeleted=0 WHERE [Key]='r_agreement_renewal_review'
PRINT '  r_agreement_renewal_review'

IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key]='r_lease_renewal')
    INSERT INTO ResponsibilityTypes([Key],[Name],IsActive,IsDeleted) VALUES('r_lease_renewal','Lease Renewal',1,0)
ELSE UPDATE ResponsibilityTypes SET [Name]='Lease Renewal',IsActive=1,IsDeleted=0 WHERE [Key]='r_lease_renewal'
PRINT '  r_lease_renewal'

IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key]='r_lease_renewal_ceo_approval')
    INSERT INTO ResponsibilityTypes([Key],[Name],IsActive,IsDeleted) VALUES('r_lease_renewal_ceo_approval','Lease Renewal CEO Approval',1,0)
ELSE UPDATE ResponsibilityTypes SET [Name]='Lease Renewal CEO Approval',IsActive=1,IsDeleted=0 WHERE [Key]='r_lease_renewal_ceo_approval'
PRINT '  r_lease_renewal_ceo_approval'

IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key]='r_lease_renewal_recommendation')
    INSERT INTO ResponsibilityTypes([Key],[Name],IsActive,IsDeleted) VALUES('r_lease_renewal_recommendation','Lease Renewal Recommendation',1,0)
ELSE UPDATE ResponsibilityTypes SET [Name]='Lease Renewal Recommendation',IsActive=1,IsDeleted=0 WHERE [Key]='r_lease_renewal_recommendation'
PRINT '  r_lease_renewal_recommendation'

IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key]='r_lease_renewal_revenue')
    INSERT INTO ResponsibilityTypes([Key],[Name],IsActive,IsDeleted) VALUES('r_lease_renewal_revenue','Lease Renewal Revenue',1,0)
ELSE UPDATE ResponsibilityTypes SET [Name]='Lease Renewal Revenue',IsActive=1,IsDeleted=0 WHERE [Key]='r_lease_renewal_revenue'
PRINT '  r_lease_renewal_revenue'

IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key]='r_lease_renewals')
    INSERT INTO ResponsibilityTypes([Key],[Name],IsActive,IsDeleted) VALUES('r_lease_renewals','Property Lease Renewals',1,0)
ELSE UPDATE ResponsibilityTypes SET [Name]='Property Lease Renewals',IsActive=1,IsDeleted=0 WHERE [Key]='r_lease_renewals'
PRINT '  r_lease_renewals'

IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key]='r_renewal_ceo_sign')
    INSERT INTO ResponsibilityTypes([Key],[Name],IsActive,IsDeleted) VALUES('r_renewal_ceo_sign','Renewal CEO Signature',1,0)
ELSE UPDATE ResponsibilityTypes SET [Name]='Renewal CEO Signature',IsActive=1,IsDeleted=0 WHERE [Key]='r_renewal_ceo_sign'
PRINT '  r_renewal_ceo_sign'

IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key]='r_renewal_docs_upload')
    INSERT INTO ResponsibilityTypes([Key],[Name],IsActive,IsDeleted) VALUES('r_renewal_docs_upload','Renew Lease',1,0)
ELSE UPDATE ResponsibilityTypes SET [Name]='Renew Lease',IsActive=1,IsDeleted=0 WHERE [Key]='r_renewal_docs_upload'
PRINT '  r_renewal_docs_upload'

IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key]='r_renewal_first_recommendation')
    INSERT INTO ResponsibilityTypes([Key],[Name],IsActive,IsDeleted) VALUES('r_renewal_first_recommendation','Renewal Recommendation',1,0)
ELSE UPDATE ResponsibilityTypes SET [Name]='Renewal Recommendation',IsActive=1,IsDeleted=0 WHERE [Key]='r_renewal_first_recommendation'
PRINT '  r_renewal_first_recommendation'

IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key]='r_renewal_rm_sign')
    INSERT INTO ResponsibilityTypes([Key],[Name],IsActive,IsDeleted) VALUES('r_renewal_rm_sign','Renewal RM Signature',1,0)
ELSE UPDATE ResponsibilityTypes SET [Name]='Renewal RM Signature',IsActive=1,IsDeleted=0 WHERE [Key]='r_renewal_rm_sign'
PRINT '  r_renewal_rm_sign'

IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key]='r_renewal_tenant_sign')
    INSERT INTO ResponsibilityTypes([Key],[Name],IsActive,IsDeleted) VALUES('r_renewal_tenant_sign','Renewal Tenant Signature',1,0)
ELSE UPDATE ResponsibilityTypes SET [Name]='Renewal Tenant Signature',IsActive=1,IsDeleted=0 WHERE [Key]='r_renewal_tenant_sign'
PRINT '  r_renewal_tenant_sign'

IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key]='r_risk_assessment_renewal')
    INSERT INTO ResponsibilityTypes([Key],[Name],IsActive,IsDeleted) VALUES('r_risk_assessment_renewal','Renewal Risk Assessment',1,0)
ELSE UPDATE ResponsibilityTypes SET [Name]='Renewal Risk Assessment',IsActive=1,IsDeleted=0 WHERE [Key]='r_risk_assessment_renewal'
PRINT '  r_risk_assessment_renewal'

-- ==============================================================
-- SECTION 3: RCS ACTION TYPES (2 rows)
-- ==============================================================
PRINT ''
PRINT '--- Section 3: RCS Action Types ---'

IF NOT EXISTS (SELECT 1 FROM RCSActionTypes WHERE [Key]='plm_not_supported_tenant')
    INSERT INTO RCSActionTypes([Key],[Name],IsActive,IsDeleted) VALUES('plm_not_supported_tenant','Not Supported - Tenant',1,0)
ELSE UPDATE RCSActionTypes SET [Name]='Not Supported - Tenant',IsActive=1,IsDeleted=0 WHERE [Key]='plm_not_supported_tenant'
PRINT '  plm_not_supported_tenant'

IF NOT EXISTS (SELECT 1 FROM RCSActionTypes WHERE [Key]='plm_not_supported_cso')
    INSERT INTO RCSActionTypes([Key],[Name],IsActive,IsDeleted) VALUES('plm_not_supported_cso','Not Supported - CSO',1,0)
ELSE UPDATE RCSActionTypes SET [Name]='Not Supported - CSO',IsActive=1,IsDeleted=0 WHERE [Key]='plm_not_supported_cso'
PRINT '  plm_not_supported_cso'

-- SECTIONS 4-6 APPENDED BELOW

-- SECTION 4: EMAIL CONTENT TYPES (13 rows)
PRINT ''
PRINT '--- Section 4: Email Content Types ---'

MERGE EmailContentTypes AS T
USING (VALUES
  ('plm_agreement_renewal_accepted_by_leasee','Message Sent From System To Applicant','You have accepted agreement renewal for your agreement with reference number {0}. Please contact officials for more info.'),
  ('plm_agreement_renewal_approved','Message Sent From BO To Applicant','Agreement of Lease Renewal Has Been Approved for your application with the reference {0}. Your Renewal OTP is <b>{1}</b>, to be used for actioning the renewal. Please Login to action.'),
  ('plm_agreement_renewal_rejected','Message Sent From BO To Applicant','Agreement of Lease Renewal Has Been Rejected for your application with the reference {0}. Please contact your officials for more information.'),
  ('plm_agreement_renewal_rejected_by_leasee','Message Sent From System To Applicant','You have rejected agreement renewal for your agreement with reference number {0}. Please contact officials for more info.'),
  ('plm_agreement_renewal_upload_','Message Sent From BO To Applicant','Agreement Renewal Documents has been uploaded successfully for reference number {0}, your agreement period has been extented for {1} months due to end on {2}. Please contact officials for more info.'),
  ('plm_applicant_renewal','Message Sent From BO To Applicannt','Your application with Ref: {0}, is due for renewal in the next 3 months. Please login to action before the {date} or your application will proceed to the following step with your action as DID NOT RESPOND.'),
  ('plm_approve_renewal_recommendations','Message Sent From BO To Applicant','Your application with the reference number {0} has been approved for {1} months renewal period. Please login for more information.'),
  ('plm_awaiting_debit_order','Message Sent From BO To Applicant','Your application with Ref: {0} is awaiting Debit Order Authority. Please login to action.'),
  ('plm_backoffice_renewal_','Message Sent From System To BO','The application with Ref: {0}, assigned to you is due for renewal in the next 3 months. Please login to action.'),
  ('plm_lease_signing_expired','Lease Signing Expired Notification','BR19: Notification sent to the tenant when the 30-day lease signing deadline has expired and the application is disregarded.'),
  ('plm_rejected_renewal_recommendations','Message Sent From BO To Applicant','Your application with the reference number {0} has been rejected for renewal it will terminate. Please login for more information.'),
  ('plm_renewal_reject_by_customer','Message Sent From BO To Applicant','The renewal offer sent by Back Office to applicant has been rejected by applicant, application with the reference number {0}, Reason: '),
  ('plm_unit_offer_expired','Unit Offer Expired Notification','BR09: Notification sent to the applicant when the 30-day unit acceptance deadline has expired. The unit offer is withdrawn and the applicant re-listed on the waiting list.'),
  ('plm_up_for_renewal','Message Sent From System To Applicant','Your application with the reference number {0} is due for renewal, it currently under renewal review. Please login for more information.')
) AS S([Key],[Name],[Description])
ON T.[Key] = S.[Key]
WHEN MATCHED THEN UPDATE SET T.[Name]=S.[Name],T.[Description]=S.[Description],T.IsActive=1,T.IsDeleted=0
WHEN NOT MATCHED THEN INSERT([Key],[Name],[Description],IsActive,IsDeleted,CreatedDateTime) VALUES(S.[Key],S.[Name],S.[Description],1,0,GETDATE());
PRINT '  14 EmailContentTypes merged.'

-- SECTION 5: ACTIVITY TRACKER MESSAGES (17 rows)
PRINT ''
PRINT '--- Section 5: Activity Tracker Messages ---'

MERGE ActivityTrackerMessages AS T
USING (VALUES
  ('at_agreement_renewal_accepted_by_leasee','LEASEE ACCEPTS AGREEMENT RENEWAL','Agreement renewal has been accepted, the lease will continue and due to end at end of additional period.'),
  ('at_agreement_renewal_approved','Renewal Agreement Approved','Agreement of Lease Renewal Approved By {0}'),
  ('at_agreement_renewal_reject','Renewal Agreement Rejected','Agreement of Lease Renewal Rejected By {0}'),
  ('at_agreement_renewal_rejected_by_leasee','LEASEE REJECTS AGREEMENT RENEWAL','Agreement renewal has been rejected, the lease will terminate at end of initial leased period.'),
  ('at_lease_agreement_rejected_cso','Lease Agreement rejected - returned to CSO for corrections','Lease Agreement rejected - returned to CSO for corrections'),
  ('at_lease_agreement_rejected_tenant','Lease Agreement rejected - returned to Tenant for corrections','Lease Agreement rejected - returned to Tenant for corrections'),
  ('at_lease_signing_expired','Lease Signing Expired','BR19: Application disregarded - lease agreement not signed within 30 days of being sent.'),
  ('at_lease_validation_approve','Lease Application Passed Renewal Validation Against Tenant Complaints','Lease Passed Renewal Validation Against Tenant Complaints'),
  ('at_lease_validation_reject','Lease Application Faied Renewal Validation Against Tenant Complaints','Lease Application Faied Renewal Validation Against Tenant Complaints'),
  ('at_tenant_risk_approve','Risk Assessment Aproved, Lease Application Ready For Renewal','Risk Assessment Aproved, Lease Application Ready For Renewal'),
  ('at_tenant_risk_reject','Risk Assessment Rejected, Lease Application Not Ready For Renewal','Risk Assessment Rejected, Lease Application Not Ready For Renewal'),
  ('at_unit_offer_expired','Unit Offer Expired','BR09: Unit offer withdrawn - applicant did not accept the matched unit within 30 days. Application re-listed on waiting list.'),
  ('at_upload_renewal_documets','Upload Renewal Documents','Agreement renewal documents uploaded successfully, new agreement end date issued.'),
  ('atm_lease_agreement_rejected_to_cso','Lease Agreement Rejected (CSO)','Lease agreement rejected and returned to CSO for lease detail corrections'),
  ('atm_lease_agreement_rejected_to_tenant','Lease Agreement Rejected (Tenant)','Lease agreement rejected and returned to Tenant for corrections')
) AS S([Key],[Name],[Description])
ON T.[Key] = S.[Key]
WHEN MATCHED THEN UPDATE SET T.[Name]=S.[Name],T.[Description]=S.[Description],T.IsActive=1,T.IsDeleted=0
WHEN NOT MATCHED THEN INSERT([Key],[Name],[Description],IsActive,IsDeleted,CreatedDateTime) VALUES(S.[Key],S.[Name],S.[Description],1,0,GETDATE());
PRINT '  15 ActivityTrackerMessages merged.'

-- SECTION 6: DOCUMENT TYPES + CHECKLISTS (8 doc types)
PRINT ''
PRINT '--- Section 6: Document Types and Checklists ---'

DECLARE @AppId INT, @RefTypeId INT
SELECT @AppId=Id FROM Applications WHERE [Key]='a_rates_clearance_system'
SELECT @RefTypeId=Id FROM ReferenceTypes WHERE [Key]='rt_upload_rcs'

IF @AppId IS NULL OR @RefTypeId IS NULL
BEGIN PRINT '  ERROR: Missing Application or ReferenceType key. Skipping doc types.' GOTO Done END

DECLARE @dt INT

SET @dt = NULL
SELECT @dt=Id FROM DocumentTypes WHERE [Key]='dt_renewal_identity_document'
IF @dt IS NULL BEGIN INSERT INTO DocumentTypes([Key],[Name],[Description],IsActive,IsDeleted,CreatedDateTime,ModifiedDateTime,CreatedBySystemUserId) VALUES('dt_renewal_identity_document','ID Document (Renewal)','Copy of ID Document - submitted during lease renewal',1,0,GETDATE(),GETDATE(),1) SET @dt=SCOPE_IDENTITY() END ELSE UPDATE DocumentTypes SET [Name]='ID Document (Renewal)',[Description]='Copy of ID Document - submitted during lease renewal',IsActive=1,IsDeleted=0 WHERE [Key]='dt_renewal_identity_document'
IF NOT EXISTS(SELECT 1 FROM DocumentCheckLists WHERE DocumentTypeId=@dt AND ApplicationId=@AppId AND ReferenceTypeId=@RefTypeId) INSERT INTO DocumentCheckLists(DocumentTypeId,ApplicationId,ReferenceTypeId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime) VALUES(@dt,@AppId,@RefTypeId,1,0,0,1,GETDATE(),1,GETDATE())
PRINT '  dt_renewal_identity_document'

SET @dt = NULL
SELECT @dt=Id FROM DocumentTypes WHERE [Key]='dt_renewal_proof_of_income'
IF @dt IS NULL BEGIN INSERT INTO DocumentTypes([Key],[Name],[Description],IsActive,IsDeleted,CreatedDateTime,ModifiedDateTime,CreatedBySystemUserId) VALUES('dt_renewal_proof_of_income','Proof of Income (Renewal)','Payslip / Pension / Grant - submitted during lease renewal',1,0,GETDATE(),GETDATE(),1) SET @dt=SCOPE_IDENTITY() END ELSE UPDATE DocumentTypes SET [Name]='Proof of Income (Renewal)',[Description]='Payslip / Pension / Grant - submitted during lease renewal',IsActive=1,IsDeleted=0 WHERE [Key]='dt_renewal_proof_of_income'
IF NOT EXISTS(SELECT 1 FROM DocumentCheckLists WHERE DocumentTypeId=@dt AND ApplicationId=@AppId AND ReferenceTypeId=@RefTypeId) INSERT INTO DocumentCheckLists(DocumentTypeId,ApplicationId,ReferenceTypeId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime) VALUES(@dt,@AppId,@RefTypeId,1,0,0,1,GETDATE(),1,GETDATE())
PRINT '  dt_renewal_proof_of_income'

SET @dt = NULL
SELECT @dt=Id FROM DocumentTypes WHERE [Key]='dt_renewal_bank_statement'
IF @dt IS NULL BEGIN INSERT INTO DocumentTypes([Key],[Name],[Description],IsActive,IsDeleted,CreatedDateTime,ModifiedDateTime,CreatedBySystemUserId) VALUES('dt_renewal_bank_statement','Bank Statement (Renewal)','Bank statement - submitted during lease renewal',1,0,GETDATE(),GETDATE(),1) SET @dt=SCOPE_IDENTITY() END ELSE UPDATE DocumentTypes SET [Name]='Bank Statement (Renewal)',[Description]='Bank statement - submitted during lease renewal',IsActive=1,IsDeleted=0 WHERE [Key]='dt_renewal_bank_statement'
IF NOT EXISTS(SELECT 1 FROM DocumentCheckLists WHERE DocumentTypeId=@dt AND ApplicationId=@AppId AND ReferenceTypeId=@RefTypeId) INSERT INTO DocumentCheckLists(DocumentTypeId,ApplicationId,ReferenceTypeId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime) VALUES(@dt,@AppId,@RefTypeId,1,0,0,1,GETDATE(),1,GETDATE())
PRINT '  dt_renewal_bank_statement'

SET @dt = NULL
SELECT @dt=Id FROM DocumentTypes WHERE [Key]='dt_renewal_proof_of_employment'
IF @dt IS NULL BEGIN INSERT INTO DocumentTypes([Key],[Name],[Description],IsActive,IsDeleted,CreatedDateTime,ModifiedDateTime,CreatedBySystemUserId) VALUES('dt_renewal_proof_of_employment','Proof of Employment (Renewal)','Proof of employment - submitted during lease renewal',1,0,GETDATE(),GETDATE(),1) SET @dt=SCOPE_IDENTITY() END ELSE UPDATE DocumentTypes SET [Name]='Proof of Employment (Renewal)',[Description]='Proof of employment - submitted during lease renewal',IsActive=1,IsDeleted=0 WHERE [Key]='dt_renewal_proof_of_employment'
IF NOT EXISTS(SELECT 1 FROM DocumentCheckLists WHERE DocumentTypeId=@dt AND ApplicationId=@AppId AND ReferenceTypeId=@RefTypeId) INSERT INTO DocumentCheckLists(DocumentTypeId,ApplicationId,ReferenceTypeId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime) VALUES(@dt,@AppId,@RefTypeId,1,0,0,1,GETDATE(),1,GETDATE())
PRINT '  dt_renewal_proof_of_employment'

SET @dt = NULL
SELECT @dt=Id FROM DocumentTypes WHERE [Key]='dt_renewal_affidavit'
IF @dt IS NULL BEGIN INSERT INTO DocumentTypes([Key],[Name],[Description],IsActive,IsDeleted,CreatedDateTime,ModifiedDateTime,CreatedBySystemUserId) VALUES('dt_renewal_affidavit','Affidavit (Renewal)','Affidavit document - submitted during lease renewal',1,0,GETDATE(),GETDATE(),1) SET @dt=SCOPE_IDENTITY() END ELSE UPDATE DocumentTypes SET [Name]='Affidavit (Renewal)',[Description]='Affidavit document - submitted during lease renewal',IsActive=1,IsDeleted=0 WHERE [Key]='dt_renewal_affidavit'
IF NOT EXISTS(SELECT 1 FROM DocumentCheckLists WHERE DocumentTypeId=@dt AND ApplicationId=@AppId AND ReferenceTypeId=@RefTypeId) INSERT INTO DocumentCheckLists(DocumentTypeId,ApplicationId,ReferenceTypeId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime) VALUES(@dt,@AppId,@RefTypeId,1,0,0,1,GETDATE(),1,GETDATE())
PRINT '  dt_renewal_affidavit'

SET @dt = NULL
SELECT @dt=Id FROM DocumentTypes WHERE [Key]='dt_renewal_proof_of_address'
IF @dt IS NULL BEGIN INSERT INTO DocumentTypes([Key],[Name],[Description],IsActive,IsDeleted,CreatedDateTime,ModifiedDateTime,CreatedBySystemUserId) VALUES('dt_renewal_proof_of_address','Proof of Address (Renewal)','Proof of address - submitted during lease renewal',1,0,GETDATE(),GETDATE(),1) SET @dt=SCOPE_IDENTITY() END ELSE UPDATE DocumentTypes SET [Name]='Proof of Address (Renewal)',[Description]='Proof of address - submitted during lease renewal',IsActive=1,IsDeleted=0 WHERE [Key]='dt_renewal_proof_of_address'
IF NOT EXISTS(SELECT 1 FROM DocumentCheckLists WHERE DocumentTypeId=@dt AND ApplicationId=@AppId AND ReferenceTypeId=@RefTypeId) INSERT INTO DocumentCheckLists(DocumentTypeId,ApplicationId,ReferenceTypeId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime) VALUES(@dt,@AppId,@RefTypeId,1,0,0,1,GETDATE(),1,GETDATE())
PRINT '  dt_renewal_proof_of_address'

SET @dt = NULL
SELECT @dt=Id FROM DocumentTypes WHERE [Key]='dt_renewal_letter'
IF @dt IS NULL BEGIN INSERT INTO DocumentTypes([Key],[Name],[Description],IsActive,IsDeleted,CreatedDateTime,ModifiedDateTime,CreatedBySystemUserId) VALUES('dt_renewal_letter','Renewal Letter','Renewal Letter',1,0,GETDATE(),GETDATE(),1) SET @dt=SCOPE_IDENTITY() END ELSE UPDATE DocumentTypes SET [Name]='Renewal Letter',[Description]='Renewal Letter',IsActive=1,IsDeleted=0 WHERE [Key]='dt_renewal_letter'
IF NOT EXISTS(SELECT 1 FROM DocumentCheckLists WHERE DocumentTypeId=@dt AND ApplicationId=@AppId AND ReferenceTypeId=@RefTypeId) INSERT INTO DocumentCheckLists(DocumentTypeId,ApplicationId,ReferenceTypeId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime) VALUES(@dt,@AppId,@RefTypeId,1,0,0,1,GETDATE(),1,GETDATE())
PRINT '  dt_renewal_letter'

SET @dt = NULL
SELECT @dt=Id FROM DocumentTypes WHERE [Key]='dt_risk_assessment_renewal'
IF @dt IS NULL BEGIN INSERT INTO DocumentTypes([Key],[Name],[Description],IsActive,IsDeleted,CreatedDateTime,ModifiedDateTime,CreatedBySystemUserId) VALUES('dt_risk_assessment_renewal','Risk Assessment Supporting Document','Supporting Document',1,0,GETDATE(),GETDATE(),1) SET @dt=SCOPE_IDENTITY() END ELSE UPDATE DocumentTypes SET [Name]='Risk Assessment Supporting Document',[Description]='Supporting Document',IsActive=1,IsDeleted=0 WHERE [Key]='dt_risk_assessment_renewal'
IF NOT EXISTS(SELECT 1 FROM DocumentCheckLists WHERE DocumentTypeId=@dt AND ApplicationId=@AppId AND ReferenceTypeId=@RefTypeId) INSERT INTO DocumentCheckLists(DocumentTypeId,ApplicationId,ReferenceTypeId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime) VALUES(@dt,@AppId,@RefTypeId,1,0,0,1,GETDATE(),1,GETDATE())
PRINT '  dt_risk_assessment_renewal'

Done:
PRINT ''
PRINT '==================================================================='
PRINT 'PLM FULL PRODUCTION DEPLOYMENT � COMPLETE'
PRINT 'End Time: ' + CONVERT(VARCHAR, GETDATE(), 120)
PRINT '==================================================================='
PRINT 'TOTALS: 21 Statuses | 14 ResponsibilityTypes | 2 RCSActionTypes'
PRINT '        14 EmailContentTypes | 15 ActivityTrackerMessages'
PRINT '        8 DocumentTypes | 8 DocumentCheckLists'
SET NOCOUNT OFF
GO

