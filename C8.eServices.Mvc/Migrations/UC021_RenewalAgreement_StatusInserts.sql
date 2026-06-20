-- UC021: Generate Lease Renewal Agreement — Status & Responsibility Type Inserts
-- Run this script against the PLM database before deploying the code changes.

-- ============================================================
-- 1. Insert new Status records for UC021 workflow
-- ============================================================
DECLARE @StatusTypeId INT = (SELECT TOP 1 StatusTypeId FROM Status WHERE [Key] = 's_awaiting_renewal_agreement_conclusion');
DECLARE @Now DATETIME = GETDATE();

-- Only insert if not already present
IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_awaiting_renewal_lease_capture')
INSERT INTO Status ([Key], Name, StatusTypeId, IsActive, IsDeleted, IsLocked, IsNew, CreatedDateTime, ModifiedDateTime)
VALUES ('s_awaiting_renewal_lease_capture', 'Awaiting Renewal Lease Details Capture', @StatusTypeId, 1, 0, 0, 0, @Now, @Now);

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_awaiting_renewal_agreement_generation')
INSERT INTO Status ([Key], Name, StatusTypeId, IsActive, IsDeleted, IsLocked, IsNew, CreatedDateTime, ModifiedDateTime)
VALUES ('s_awaiting_renewal_agreement_generation', 'Awaiting Renewal Agreement Generation', @StatusTypeId, 1, 0, 0, 0, @Now, @Now);

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_awaiting_renewal_tenant_signature')
INSERT INTO Status ([Key], Name, StatusTypeId, IsActive, IsDeleted, IsLocked, IsNew, CreatedDateTime, ModifiedDateTime)
VALUES ('s_awaiting_renewal_tenant_signature', 'Awaiting Renewal Tenant Signature', @StatusTypeId, 1, 0, 0, 0, @Now, @Now);

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_awaiting_renewal_rm_signature')
INSERT INTO Status ([Key], Name, StatusTypeId, IsActive, IsDeleted, IsLocked, IsNew, CreatedDateTime, ModifiedDateTime)
VALUES ('s_awaiting_renewal_rm_signature', 'Awaiting Renewal RM Signature', @StatusTypeId, 1, 0, 0, 0, @Now, @Now);

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_awaiting_renewal_ceo_signature')
INSERT INTO Status ([Key], Name, StatusTypeId, IsActive, IsDeleted, IsLocked, IsNew, CreatedDateTime, ModifiedDateTime)
VALUES ('s_awaiting_renewal_ceo_signature', 'Awaiting Renewal CEO Signature', @StatusTypeId, 1, 0, 0, 0, @Now, @Now);

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_renewal_agreement_all_signed')
INSERT INTO Status ([Key], Name, StatusTypeId, IsActive, IsDeleted, IsLocked, IsNew, CreatedDateTime, ModifiedDateTime)
VALUES ('s_renewal_agreement_all_signed', 'Renewal Agreement Fully Signed', @StatusTypeId, 1, 0, 0, 0, @Now, @Now);

-- ============================================================
-- 2. Insert new ResponsibilityType records for UC021 signing
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key] = 'r_renewal_tenant_sign')
INSERT INTO ResponsibilityTypes ([Key], Name, IsActive, IsDeleted, IsLocked, IsNew, CreatedDateTime, ModifiedDateTime)
VALUES ('r_renewal_tenant_sign', 'Renewal Tenant Signature', 1, 0, 0, 0, @Now, @Now);

IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key] = 'r_renewal_rm_sign')
INSERT INTO ResponsibilityTypes ([Key], Name, IsActive, IsDeleted, IsLocked, IsNew, CreatedDateTime, ModifiedDateTime)
VALUES ('r_renewal_rm_sign', 'Renewal Revenue Manager Signature', 1, 0, 0, 0, @Now, @Now);

IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key] = 'r_renewal_ceo_sign')
INSERT INTO ResponsibilityTypes ([Key], Name, IsActive, IsDeleted, IsLocked, IsNew, CreatedDateTime, ModifiedDateTime)
VALUES ('r_renewal_ceo_sign', 'Renewal CEO Signature', 1, 0, 0, 0, @Now, @Now);

-- ============================================================
-- Verify
-- ============================================================
SELECT 'Statuses' AS [Type], [Key], Name FROM Status WHERE [Key] LIKE 's_%renewal%' ORDER BY Id;
SELECT 'ResponsibilityTypes' AS [Type], [Key], Name FROM ResponsibilityTypes WHERE [Key] LIKE 'r_renewal%' ORDER BY Id;
