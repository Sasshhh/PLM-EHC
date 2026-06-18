-- ==============================================================================
-- UNIFIED PLM SYSTEM MASTERDATA MIGRATION SCRIPT
-- ==============================================================================
-- Includes all necessary system metadata for:
-- 1. UC12-13: Unit Offer & Lease Signing Expirations (BR09 / BR19)
-- 2. UC18-22: Lease Renewal Workflow (Tenant, PM, RM, CEO Signatures)
-- 3. UC23: Manage Lease Termination (CSO Review, Appraisal, Legal Referral)
-- 4. UC24: Eviction capturing & CEO Authorization
-- 5. UC25: Serve Eviction Notice & Proof of Service
-- 6. UC26: Lease Disputes Workflow (Register, Review, Resolve, Close)
-- 7. UC27-30: Exit Inspection, Move-Out, and Deposit Refund Approval
-- ==============================================================================

PRINT '--- STARTING UNIFIED PLM MASTERDATA SETUP ---';

-- ==============================================================================
-- 1. STATUSES (Table: Status)
-- ==============================================================================
PRINT 'Setting up Statuses...';

-- UC12-13 Expirations
IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_lease_signing_expired') BEGIN
    INSERT INTO Status ([Key], [Name], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('s_lease_signing_expired', 'Lease Signing Expired (30-day)', 1, 1, 0, GETDATE());
    PRINT 'Inserted Status: s_lease_signing_expired';
END
ELSE BEGIN
    UPDATE Status SET [Name] = 'Lease Signing Expired (30-day)', [StatusTypeId] = 1, [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 's_lease_signing_expired';
    PRINT 'Updated Status: s_lease_signing_expired';
END

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_unit_offer_expired') BEGIN
    INSERT INTO Status ([Key], [Name], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('s_unit_offer_expired', 'Unit Offer Expired (30-day)', 1, 1, 0, GETDATE());
    PRINT 'Inserted Status: s_unit_offer_expired';
END
ELSE BEGIN
    UPDATE Status SET [Name] = 'Unit Offer Expired (30-day)', [StatusTypeId] = 1, [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 's_unit_offer_expired';
    PRINT 'Updated Status: s_unit_offer_expired';
END

-- UC18-22 Lease Renewals
IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_awaiting_renewal_docs') BEGIN
    INSERT INTO Status ([Key], [Name], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('s_awaiting_renewal_docs', 'Awaiting Renewal Document(s) Upload', 1, 1, 0, GETDATE());
    PRINT 'Inserted Status: s_awaiting_renewal_docs';
END
ELSE BEGIN
    UPDATE Status SET [Name] = 'Awaiting Renewal Document(s) Upload', [StatusTypeId] = 1, [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 's_awaiting_renewal_docs';
    PRINT 'Updated Status: s_awaiting_renewal_docs';
END

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_awaiting_renewal_lease_capture') BEGIN
    INSERT INTO Status ([Key], [Name], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('s_awaiting_renewal_lease_capture', 'Awaiting Renewal Lease Capture', 1, 1, 0, GETDATE());
    PRINT 'Inserted Status: s_awaiting_renewal_lease_capture';
END
ELSE BEGIN
    UPDATE Status SET [Name] = 'Awaiting Renewal Lease Capture', [StatusTypeId] = 1, [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 's_awaiting_renewal_lease_capture';
    PRINT 'Updated Status: s_awaiting_renewal_lease_capture';
END

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_awaiting_renewal_rm_signature') BEGIN
    INSERT INTO Status ([Key], [Name], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('s_awaiting_renewal_rm_signature', 'Awaiting Renewal RM Signature', 1, 1, 0, GETDATE());
    PRINT 'Inserted Status: s_awaiting_renewal_rm_signature';
END
ELSE BEGIN
    UPDATE Status SET [Name] = 'Awaiting Renewal RM Signature', [StatusTypeId] = 1, [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 's_awaiting_renewal_rm_signature';
    PRINT 'Updated Status: s_awaiting_renewal_rm_signature';
END

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_awaiting_renewal_ceo_signature') BEGIN
    INSERT INTO Status ([Key], [Name], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('s_awaiting_renewal_ceo_signature', 'Awaiting Renewal CEO Signature', 1, 1, 0, GETDATE());
    PRINT 'Inserted Status: s_awaiting_renewal_ceo_signature';
END
ELSE BEGIN
    UPDATE Status SET [Name] = 'Awaiting Renewal CEO Signature', [StatusTypeId] = 1, [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 's_awaiting_renewal_ceo_signature';
    PRINT 'Updated Status: s_awaiting_renewal_ceo_signature';
END

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_awaiting_renewal_tenant_signature') BEGIN
    INSERT INTO Status ([Key], [Name], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('s_awaiting_renewal_tenant_signature', 'Awaiting Renewal Tenant Signature', 1, 1, 0, GETDATE());
    PRINT 'Inserted Status: s_awaiting_renewal_tenant_signature';
END
ELSE BEGIN
    UPDATE Status SET [Name] = 'Awaiting Renewal Tenant Signature', [StatusTypeId] = 1, [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 's_awaiting_renewal_tenant_signature';
    PRINT 'Updated Status: s_awaiting_renewal_tenant_signature';
END

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_lease_renewal_agreement_concluded') BEGIN
    INSERT INTO Status ([Key], [Name], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('s_lease_renewal_agreement_concluded', 'Lease Renewal Agreement Concluded', 1, 1, 0, GETDATE());
    PRINT 'Inserted Status: s_lease_renewal_agreement_concluded';
END
ELSE BEGIN
    UPDATE Status SET [Name] = 'Lease Renewal Agreement Concluded', [StatusTypeId] = 1, [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 's_lease_renewal_agreement_concluded';
    PRINT 'Updated Status: s_lease_renewal_agreement_concluded';
END

-- UC23 Lease Terminations
IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_awaiting_cso_termination_review') BEGIN
    INSERT INTO Status ([Key], [Name], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('s_awaiting_cso_termination_review', 'Awaiting CSO Termination Review', 1, 1, 0, GETDATE());
    PRINT 'Inserted Status: s_awaiting_cso_termination_review';
END
ELSE BEGIN
    UPDATE Status SET [Name] = 'Awaiting CSO Termination Review', [StatusTypeId] = 1, [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 's_awaiting_cso_termination_review';
    PRINT 'Updated Status: s_awaiting_cso_termination_review';
END

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_awaiting_termination_appraisal') BEGIN
    INSERT INTO Status ([Key], [Name], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('s_awaiting_termination_appraisal', 'Awaiting Termination Appraisal', 1, 1, 0, GETDATE());
    PRINT 'Inserted Status: s_awaiting_termination_appraisal';
END
ELSE BEGIN
    UPDATE Status SET [Name] = 'Awaiting Termination Appraisal', [StatusTypeId] = 1, [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 's_awaiting_termination_appraisal';
    PRINT 'Updated Status: s_awaiting_termination_appraisal';
END

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_termination_not_supported') BEGIN
    INSERT INTO Status ([Key], [Name], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('s_termination_not_supported', 'Termination Not Supported', 1, 1, 0, GETDATE());
    PRINT 'Inserted Status: s_termination_not_supported';
END
ELSE BEGIN
    UPDATE Status SET [Name] = 'Termination Not Supported', [StatusTypeId] = 1, [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 's_termination_not_supported';
    PRINT 'Updated Status: s_termination_not_supported';
END

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_legal_referral_pending') BEGIN
    INSERT INTO Status ([Key], [Name], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('s_legal_referral_pending', 'Legal Referral Pending', 1, 1, 0, GETDATE());
    PRINT 'Inserted Status: s_legal_referral_pending';
END
ELSE BEGIN
    UPDATE Status SET [Name] = 'Legal Referral Pending', [StatusTypeId] = 1, [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 's_legal_referral_pending';
    PRINT 'Updated Status: s_legal_referral_pending';
END

-- UC24 Evictions CEO Approval
IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_awaiting_eviction_ceo_auth') BEGIN
    INSERT INTO Status ([Key], [Name], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('s_awaiting_eviction_ceo_auth', 'Awaiting Eviction CEO Authorization', 1, 1, 0, GETDATE());
    PRINT 'Inserted Status: s_awaiting_eviction_ceo_auth';
END
ELSE BEGIN
    UPDATE Status SET [Name] = 'Awaiting Eviction CEO Authorization', [StatusTypeId] = 1, [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 's_awaiting_eviction_ceo_auth';
    PRINT 'Updated Status: s_awaiting_eviction_ceo_auth';
END

-- UC25 Serve Eviction Notice
IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_awaiting_eviction_service') BEGIN
    INSERT INTO Status ([Key], [Name], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('s_awaiting_eviction_service', 'Awaiting Eviction Notice Service', 1, 1, 0, GETDATE());
    PRINT 'Inserted Status: s_awaiting_eviction_service';
END
ELSE BEGIN
    UPDATE Status SET [Name] = 'Awaiting Eviction Notice Service', [StatusTypeId] = 1, [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 's_awaiting_eviction_service';
    PRINT 'Updated Status: s_awaiting_eviction_service';
END

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_eviction_notice_served') BEGIN
    INSERT INTO Status ([Key], [Name], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('s_eviction_notice_served', 'Eviction Notice Served', 1, 1, 0, GETDATE());
    PRINT 'Inserted Status: s_eviction_notice_served';
END
ELSE BEGIN
    UPDATE Status SET [Name] = 'Eviction Notice Served', [StatusTypeId] = 1, [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 's_eviction_notice_served';
    PRINT 'Updated Status: s_eviction_notice_served';
END

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_awaiting_proof_of_service') BEGIN
    INSERT INTO Status ([Key], [Name], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('s_awaiting_proof_of_service', 'Awaiting Proof of Service Upload', 1, 1, 0, GETDATE());
    PRINT 'Inserted Status: s_awaiting_proof_of_service';
END
ELSE BEGIN
    UPDATE Status SET [Name] = 'Awaiting Proof of Service Upload', [StatusTypeId] = 1, [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 's_awaiting_proof_of_service';
    PRINT 'Updated Status: s_awaiting_proof_of_service';
END

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_proof_of_service_captured') BEGIN
    INSERT INTO Status ([Key], [Name], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('s_proof_of_service_captured', 'Proof of Service Captured', 1, 1, 0, GETDATE());
    PRINT 'Inserted Status: s_proof_of_service_captured';
END
ELSE BEGIN
    UPDATE Status SET [Name] = 'Proof of Service Captured', [StatusTypeId] = 1, [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 's_proof_of_service_captured';
    PRINT 'Updated Status: s_proof_of_service_captured';
END

-- UC26 Disputes
IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_dispute_open_awaiting_review') BEGIN
    INSERT INTO Status ([Key], [Name], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('s_dispute_open_awaiting_review', 'Dispute Open - Awaiting Review', 1, 1, 0, GETDATE());
    PRINT 'Inserted Status: s_dispute_open_awaiting_review';
END
ELSE BEGIN
    UPDATE Status SET [Name] = 'Dispute Open - Awaiting Review', [StatusTypeId] = 1, [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 's_dispute_open_awaiting_review';
    PRINT 'Updated Status: s_dispute_open_awaiting_review';
END

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_dispute_referred') BEGIN
    INSERT INTO Status ([Key], [Name], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('s_dispute_referred', 'Dispute Referred for Resolution', 1, 1, 0, GETDATE());
    PRINT 'Inserted Status: s_dispute_referred';
END
ELSE BEGIN
    UPDATE Status SET [Name] = 'Dispute Referred for Resolution', [StatusTypeId] = 1, [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 's_dispute_referred';
    PRINT 'Updated Status: s_dispute_referred';
END

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_dispute_resolved') BEGIN
    INSERT INTO Status ([Key], [Name], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('s_dispute_resolved', 'Dispute Resolved', 1, 1, 0, GETDATE());
    PRINT 'Inserted Status: s_dispute_resolved';
END
ELSE BEGIN
    UPDATE Status SET [Name] = 'Dispute Resolved', [StatusTypeId] = 1, [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 's_dispute_resolved';
    PRINT 'Updated Status: s_dispute_resolved';
END

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_dispute_closed') BEGIN
    INSERT INTO Status ([Key], [Name], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('s_dispute_closed', 'Dispute Closed', 1, 1, 0, GETDATE());
    PRINT 'Inserted Status: s_dispute_closed';
END
ELSE BEGIN
    UPDATE Status SET [Name] = 'Dispute Closed', [StatusTypeId] = 1, [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 's_dispute_closed';
    PRINT 'Updated Status: s_dispute_closed';
END

-- UC27-30 Exit, Vacating & Refunds
IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_awaiting_vacating_confirmation') BEGIN
    INSERT INTO Status ([Key], [Name], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('s_awaiting_vacating_confirmation', 'Awaiting CSO Vacating Confirmation', 1, 1, 0, GETDATE());
    PRINT 'Inserted Status: s_awaiting_vacating_confirmation';
END
ELSE BEGIN
    UPDATE Status SET [Name] = 'Awaiting CSO Vacating Confirmation', [StatusTypeId] = 1, [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 's_awaiting_vacating_confirmation';
    PRINT 'Updated Status: s_awaiting_vacating_confirmation';
END

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_awaiting_refund_authorisation') BEGIN
    INSERT INTO Status ([Key], [Name], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('s_awaiting_refund_authorisation', 'Awaiting Refund Authorisation', 1, 1, 0, GETDATE());
    PRINT 'Inserted Status: s_awaiting_refund_authorisation';
END
ELSE BEGIN
    UPDATE Status SET [Name] = 'Awaiting Refund Authorisation', [StatusTypeId] = 1, [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 's_awaiting_refund_authorisation';
    PRINT 'Updated Status: s_awaiting_refund_authorisation';
END

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_former_tenant') BEGIN
    INSERT INTO Status ([Key], [Name], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('s_former_tenant', 'Former Tenant', 1, 1, 0, GETDATE());
    PRINT 'Inserted Status: s_former_tenant';
END
ELSE BEGIN
    UPDATE Status SET [Name] = 'Former Tenant', [StatusTypeId] = 1, [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 's_former_tenant';
    PRINT 'Updated Status: s_former_tenant';
END


-- ==============================================================================
-- 2. RESPONSIBILITY TYPES (Table: ResponsibilityTypes)
-- ==============================================================================
PRINT 'Setting up Responsibility Types...';

-- UC18-22 Renewal signatures
IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key] = 'r_renewal_rm_sign') BEGIN
    INSERT INTO ResponsibilityTypes ([Key], [Name], [IsActive], [IsDeleted])
    VALUES ('r_renewal_rm_sign', 'Renewal RM Signature', 1, 0);
    PRINT 'Inserted ResponsibilityType: r_renewal_rm_sign';
END
ELSE BEGIN
    UPDATE ResponsibilityTypes SET [Name] = 'Renewal RM Signature', [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 'r_renewal_rm_sign';
    PRINT 'Updated ResponsibilityType: r_renewal_rm_sign';
END

IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key] = 'r_renewal_ceo_sign') BEGIN
    INSERT INTO ResponsibilityTypes ([Key], [Name], [IsActive], [IsDeleted])
    VALUES ('r_renewal_ceo_sign', 'Renewal CEO Signature', 1, 0);
    PRINT 'Inserted ResponsibilityType: r_renewal_ceo_sign';
END
ELSE BEGIN
    UPDATE ResponsibilityTypes SET [Name] = 'Renewal CEO Signature', [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 'r_renewal_ceo_sign';
    PRINT 'Updated ResponsibilityType: r_renewal_ceo_sign';
END

IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key] = 'r_renewal_tenant_sign') BEGIN
    INSERT INTO ResponsibilityTypes ([Key], [Name], [IsActive], [IsDeleted])
    VALUES ('r_renewal_tenant_sign', 'Renewal Tenant Signature', 1, 0);
    PRINT 'Inserted ResponsibilityType: r_renewal_tenant_sign';
END
ELSE BEGIN
    UPDATE ResponsibilityTypes SET [Name] = 'Renewal Tenant Signature', [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 'r_renewal_tenant_sign';
    PRINT 'Updated ResponsibilityType: r_renewal_tenant_sign';
END

-- UC23 Terminations
IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key] = 'r_termination_cso_review') BEGIN
    INSERT INTO ResponsibilityTypes ([Key], [Name], [IsActive], [IsDeleted])
    VALUES ('r_termination_cso_review', 'CSO Lease Termination Review', 1, 0);
    PRINT 'Inserted ResponsibilityType: r_termination_cso_review';
END
ELSE BEGIN
    UPDATE ResponsibilityTypes SET [Name] = 'CSO Lease Termination Review', [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 'r_termination_cso_review';
    PRINT 'Updated ResponsibilityType: r_termination_cso_review';
END

-- UC24 Evictions CEO
IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key] = 'r_eviction_ceo_auth') BEGIN
    INSERT INTO ResponsibilityTypes ([Key], [Name], [IsActive], [IsDeleted])
    VALUES ('r_eviction_ceo_auth', 'CEO Eviction Authorization', 1, 0);
    PRINT 'Inserted ResponsibilityType: r_eviction_ceo_auth';
END
ELSE BEGIN
    UPDATE ResponsibilityTypes SET [Name] = 'CEO Eviction Authorization', [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 'r_eviction_ceo_auth';
    PRINT 'Updated ResponsibilityType: r_eviction_ceo_auth';
END

-- UC25 Serve Eviction Notice
IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key] = 'r_eviction_cso_serve') BEGIN
    INSERT INTO ResponsibilityTypes ([Key], [Name], [IsActive], [IsDeleted])
    VALUES ('r_eviction_cso_serve', 'CSO Serve Eviction Notice', 1, 0);
    PRINT 'Inserted ResponsibilityType: r_eviction_cso_serve';
END
ELSE BEGIN
    UPDATE ResponsibilityTypes SET [Name] = 'CSO Serve Eviction Notice', [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 'r_eviction_cso_serve';
    PRINT 'Updated ResponsibilityType: r_eviction_cso_serve';
END

-- UC26 Disputes
IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key] = 'r_dispute_cso_review') BEGIN
    INSERT INTO ResponsibilityTypes ([Key], [Name], [IsActive], [IsDeleted])
    VALUES ('r_dispute_cso_review', 'CSO Dispute Review', 1, 0);
    PRINT 'Inserted ResponsibilityType: r_dispute_cso_review';
END
ELSE BEGIN
    UPDATE ResponsibilityTypes SET [Name] = 'CSO Dispute Review', [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 'r_dispute_cso_review';
    PRINT 'Updated ResponsibilityType: r_dispute_cso_review';
END

IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key] = 'r_dispute_cso_resolve') BEGIN
    INSERT INTO ResponsibilityTypes ([Key], [Name], [IsActive], [IsDeleted])
    VALUES ('r_dispute_cso_resolve', 'CSO Dispute Resolution', 1, 0);
    PRINT 'Inserted ResponsibilityType: r_dispute_cso_resolve';
END
ELSE BEGIN
    UPDATE ResponsibilityTypes SET [Name] = 'CSO Dispute Resolution', [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 'r_dispute_cso_resolve';
    PRINT 'Updated ResponsibilityType: r_dispute_cso_resolve';
END

-- UC27-30 Exit/Refunds
IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key] = 'r_refund_fo_recommendation') BEGIN
    INSERT INTO ResponsibilityTypes ([Key], [Name], [IsActive], [IsDeleted])
    VALUES ('r_refund_fo_recommendation', 'FO Deposit Refund Calculation', 1, 0);
    PRINT 'Inserted ResponsibilityType: r_refund_fo_recommendation';
END
ELSE BEGIN
    UPDATE ResponsibilityTypes SET [Name] = 'FO Deposit Refund Calculation', [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 'r_refund_fo_recommendation';
    PRINT 'Updated ResponsibilityType: r_refund_fo_recommendation';
END

IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key] = 'r_refund_rm_support') BEGIN
    INSERT INTO ResponsibilityTypes ([Key], [Name], [IsActive], [IsDeleted])
    VALUES ('r_refund_rm_support', 'RM Deposit Refund Review', 1, 0);
    PRINT 'Inserted ResponsibilityType: r_refund_rm_support';
END
ELSE BEGIN
    UPDATE ResponsibilityTypes SET [Name] = 'RM Deposit Refund Review', [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 'r_refund_rm_support';
    PRINT 'Updated ResponsibilityType: r_refund_rm_support';
END

IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key] = 'r_refund_ceo_authorization') BEGIN
    INSERT INTO ResponsibilityTypes ([Key], [Name], [IsActive], [IsDeleted])
    VALUES ('r_refund_ceo_authorization', 'CEO Deposit Refund Authorisation', 1, 0);
    PRINT 'Inserted ResponsibilityType: r_refund_ceo_authorization';
END
ELSE BEGIN
    UPDATE ResponsibilityTypes SET [Name] = 'CEO Deposit Refund Authorisation', [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 'r_refund_ceo_authorization';
    PRINT 'Updated ResponsibilityType: r_refund_ceo_authorization';
END


-- ==============================================================================
-- 3. ACTION TYPES (Table: RCSActionTypes)
-- ==============================================================================
PRINT 'Setting up RCS Action Types...';

-- UC22 Rejection flow
IF NOT EXISTS (SELECT 1 FROM RCSActionTypes WHERE [Key] = 'plm_not_supported_tenant') BEGIN
    INSERT INTO RCSActionTypes ([Key], [Name], [IsActive], [IsDeleted])
    VALUES ('plm_not_supported_tenant', 'Not Supported - Tenant', 1, 0);
    PRINT 'Inserted RCSActionType: plm_not_supported_tenant';
END
ELSE BEGIN
    UPDATE RCSActionTypes SET [Name] = 'Not Supported - Tenant', [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 'plm_not_supported_tenant';
    PRINT 'Updated RCSActionType: plm_not_supported_tenant';
END

IF NOT EXISTS (SELECT 1 FROM RCSActionTypes WHERE [Key] = 'plm_not_supported_cso') BEGIN
    INSERT INTO RCSActionTypes ([Key], [Name], [IsActive], [IsDeleted])
    VALUES ('plm_not_supported_cso', 'Not Supported - CSO', 1, 0);
    PRINT 'Inserted RCSActionType: plm_not_supported_cso';
END
ELSE BEGIN
    UPDATE RCSActionTypes SET [Name] = 'Not Supported - CSO', [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 'plm_not_supported_cso';
    PRINT 'Updated RCSActionType: plm_not_supported_cso';
END


-- ==============================================================================
-- 4. DOCUMENT TYPES (Table: DocumentTypes)
-- ==============================================================================
PRINT 'Setting up Document Types...';

-- UC25 Serve Eviction Notice
IF NOT EXISTS (SELECT 1 FROM DocumentTypes WHERE [Key] = 'dt_proof_of_service_eviction') BEGIN
    INSERT INTO DocumentTypes ([Key], [Name], [Description], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('dt_proof_of_service_eviction', 'Proof of Service (Eviction Notice)', 'Proof of service document (court/sheriff stamped notice or signed receipt)', 1, 0, GETDATE());
    PRINT 'Inserted DocumentType: dt_proof_of_service_eviction';
END
ELSE BEGIN
    UPDATE DocumentTypes SET [Name] = 'Proof of Service (Eviction Notice)', [Description] = 'Proof of service document (court/sheriff stamped notice or signed receipt)', [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 'dt_proof_of_service_eviction';
    PRINT 'Updated DocumentType: dt_proof_of_service_eviction';
END

-- UC26 Disputes
IF NOT EXISTS (SELECT 1 FROM DocumentTypes WHERE [Key] = 'dt_dispute_supporting_doc') BEGIN
    INSERT INTO DocumentTypes ([Key], [Name], [Description], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('dt_dispute_supporting_doc', 'Dispute Supporting Document', 'Any additional documents or photos submitted to support the tenant dispute', 1, 0, GETDATE());
    PRINT 'Inserted DocumentType: dt_dispute_supporting_doc';
END
ELSE BEGIN
    UPDATE DocumentTypes SET [Name] = 'Dispute Supporting Document', [Description] = 'Any additional documents or photos submitted to support the tenant dispute', [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 'dt_dispute_supporting_doc';
    PRINT 'Updated DocumentType: dt_dispute_supporting_doc';
END

-- UC27-30 Exit/Refunds
IF NOT EXISTS (SELECT 1 FROM DocumentTypes WHERE [Key] = 'dt_exit_interview_form') BEGIN
    INSERT INTO DocumentTypes ([Key], [Name], [Description], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('dt_exit_interview_form', 'Exit Interview Form', 'Completed Caretaker exit interview document', 1, 0, GETDATE());
    PRINT 'Inserted DocumentType: dt_exit_interview_form';
END
ELSE BEGIN
    UPDATE DocumentTypes SET [Name] = 'Exit Interview Form', [Description] = 'Completed Caretaker exit interview document', [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 'dt_exit_interview_form';
    PRINT 'Updated DocumentType: dt_exit_interview_form';
END

IF NOT EXISTS (SELECT 1 FROM DocumentTypes WHERE [Key] = 'dt_banking_details_proof') BEGIN
    INSERT INTO DocumentTypes ([Key], [Name], [Description], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('dt_banking_details_proof', 'Proof of Banking Details', 'Bank statement, cancelled cheque or bank confirmation letter', 1, 0, GETDATE());
    PRINT 'Inserted DocumentType: dt_banking_details_proof';
END
ELSE BEGIN
    UPDATE DocumentTypes SET [Name] = 'Proof of Banking Details', [Description] = 'Bank statement, cancelled cheque or bank confirmation letter', [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 'dt_banking_details_proof';
    PRINT 'Updated DocumentType: dt_banking_details_proof';
END


-- ==============================================================================
-- 5. EMAIL CONTENT TYPES (Table: EmailContentTypes)
-- ==============================================================================
PRINT 'Setting up Email Content Types...';

-- UC12-13 Timeout Expirations
IF NOT EXISTS (SELECT 1 FROM EmailContentTypes WHERE [Key] = 'plm_lease_signing_expired') BEGIN
    INSERT INTO EmailContentTypes ([Key], [Name], [Description], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('plm_lease_signing_expired', 'Lease Signing Expired Notification', 'Notification sent to the tenant when the 30-day lease signing deadline has expired.', 1, 0, GETDATE());
    PRINT 'Inserted EmailContentType: plm_lease_signing_expired';
END
ELSE BEGIN
    UPDATE EmailContentTypes SET [Name] = 'Lease Signing Expired Notification', [Description] = 'Notification sent to the tenant when the 30-day lease signing deadline has expired.', [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 'plm_lease_signing_expired';
    PRINT 'Updated EmailContentType: plm_lease_signing_expired';
END

IF NOT EXISTS (SELECT 1 FROM EmailContentTypes WHERE [Key] = 'plm_unit_offer_expired') BEGIN
    INSERT INTO EmailContentTypes ([Key], [Name], [Description], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('plm_unit_offer_expired', 'Unit Offer Expired Notification', 'Notification sent to the applicant when the 30-day unit acceptance deadline has expired.', 1, 0, GETDATE());
    PRINT 'Inserted EmailContentType: plm_unit_offer_expired';
END
ELSE BEGIN
    UPDATE EmailContentTypes SET [Name] = 'Unit Offer Expired Notification', [Description] = 'Notification sent to the applicant when the 30-day unit acceptance deadline has expired.', [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 'plm_unit_offer_expired';
    PRINT 'Updated EmailContentType: plm_unit_offer_expired';
END


-- ==============================================================================
-- 6. ACTIVITY TRACKER MESSAGES (Table: ActivityTrackerMessages)
-- ==============================================================================
PRINT 'Setting up Activity Tracker Messages...';

-- UC22 Rejection
IF NOT EXISTS (SELECT 1 FROM ActivityTrackerMessages WHERE [Key] = 'atm_lease_agreement_rejected_to_tenant') BEGIN
    INSERT INTO ActivityTrackerMessages ([Key], [Name], [Description], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('atm_lease_agreement_rejected_to_tenant', 'Lease Agreement Rejected (Tenant)', 'Lease agreement rejected and returned to Tenant for corrections', 1, 0, GETDATE());
    PRINT 'Inserted ActivityTrackerMessage: atm_lease_agreement_rejected_to_tenant';
END
ELSE BEGIN
    UPDATE ActivityTrackerMessages SET [Name] = 'Lease Agreement Rejected (Tenant)', [Description] = 'Lease agreement rejected and returned to Tenant for corrections', [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 'atm_lease_agreement_rejected_to_tenant';
    PRINT 'Updated ActivityTrackerMessage: atm_lease_agreement_rejected_to_tenant';
END

IF NOT EXISTS (SELECT 1 FROM ActivityTrackerMessages WHERE [Key] = 'atm_lease_agreement_rejected_to_cso') BEGIN
    INSERT INTO ActivityTrackerMessages ([Key], [Name], [Description], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('atm_lease_agreement_rejected_to_cso', 'Lease Agreement Rejected (CSO)', 'Lease agreement rejected and returned to CSO for lease detail corrections', 1, 0, GETDATE());
    PRINT 'Inserted ActivityTrackerMessage: atm_lease_agreement_rejected_to_cso';
END
ELSE BEGIN
    UPDATE ActivityTrackerMessages SET [Name] = 'Lease Agreement Rejected (CSO)', [Description] = 'Lease agreement rejected and returned to CSO for lease detail corrections', [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 'atm_lease_agreement_rejected_to_cso';
    PRINT 'Updated ActivityTrackerMessage: atm_lease_agreement_rejected_to_cso';
END

-- UC12-13 Expirations
IF NOT EXISTS (SELECT 1 FROM ActivityTrackerMessages WHERE [Key] = 'at_lease_signing_expired') BEGIN
    INSERT INTO ActivityTrackerMessages ([Key], [Name], [Description], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('at_lease_signing_expired', 'Lease Signing Expired', 'BR19: Application disregarded - lease agreement not signed within 30 days of being sent.', 1, 0, GETDATE());
    PRINT 'Inserted ActivityTrackerMessage: at_lease_signing_expired';
END
ELSE BEGIN
    UPDATE ActivityTrackerMessages SET [Name] = 'Lease Signing Expired', [Description] = 'BR19: Application disregarded - lease agreement not signed within 30 days of being sent.', [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 'at_lease_signing_expired';
    PRINT 'Updated ActivityTrackerMessage: at_lease_signing_expired';
END

IF NOT EXISTS (SELECT 1 FROM ActivityTrackerMessages WHERE [Key] = 'at_unit_offer_expired') BEGIN
    INSERT INTO ActivityTrackerMessages ([Key], [Name], [Description], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('at_unit_offer_expired', 'Unit Offer Expired', 'BR09: Unit offer withdrawn - applicant did not accept the matched unit within 30 days.', 1, 0, GETDATE());
    PRINT 'Inserted ActivityTrackerMessage: at_unit_offer_expired';
END
ELSE BEGIN
    UPDATE ActivityTrackerMessages SET [Name] = 'Unit Offer Expired', [Description] = 'BR09: Unit offer withdrawn - applicant did not accept the matched unit within 30 days.', [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 'at_unit_offer_expired';
    PRINT 'Updated ActivityTrackerMessage: at_unit_offer_expired';
END

PRINT '--- UNIFIED PLM MASTERDATA SETUP COMPLETE ---';
