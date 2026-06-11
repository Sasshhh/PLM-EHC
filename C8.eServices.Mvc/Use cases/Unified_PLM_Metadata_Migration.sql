-- ==============================================================================
-- UNIFIED PLM SYSTEM METADATA MIGRATION SCRIPT
-- ==============================================================================
-- Includes all necessary system metadata for:
-- 1. UC021: Lease Renewal Workflow (Three-Step Signature)
-- 2. UC022: Lease Agreement Reject Flows (Reject to Tenant, Reject to CSO)
-- 3. BR19: 30-Day Lease Signing Expiration
-- 4. BR09: 30-Day Unit Offer Expiration
-- ==============================================================================

PRINT '--- STARTING UNIFIED METADATA MIGRATION ---';

-- ============================================================
-- 1. UC021: LEASE RENEWAL RESPONSIBILITY TYPES
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key] = 'r_renewal_rm_sign')
BEGIN
    INSERT INTO ResponsibilityTypes ([Key], [Name], [IsActive], [IsDeleted])
    VALUES ('r_renewal_rm_sign', 'Renewal RM Signature', 1, 0);
    PRINT 'Inserted ResponsibilityType: r_renewal_rm_sign';
END
ELSE 
BEGIN
    UPDATE ResponsibilityTypes SET [Name] = 'Renewal RM Signature', [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 'r_renewal_rm_sign';
    PRINT 'Updated ResponsibilityType: r_renewal_rm_sign';
END

IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key] = 'r_renewal_ceo_sign')
BEGIN
    INSERT INTO ResponsibilityTypes ([Key], [Name], [IsActive], [IsDeleted])
    VALUES ('r_renewal_ceo_sign', 'Renewal CEO Signature', 1, 0);
    PRINT 'Inserted ResponsibilityType: r_renewal_ceo_sign';
END
ELSE 
BEGIN
    UPDATE ResponsibilityTypes SET [Name] = 'Renewal CEO Signature', [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 'r_renewal_ceo_sign';
    PRINT 'Updated ResponsibilityType: r_renewal_ceo_sign';
END

IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key] = 'r_renewal_tenant_sign')
BEGIN
    INSERT INTO ResponsibilityTypes ([Key], [Name], [IsActive], [IsDeleted])
    VALUES ('r_renewal_tenant_sign', 'Renewal Tenant Signature', 1, 0);
    PRINT 'Inserted ResponsibilityType: r_renewal_tenant_sign';
END
ELSE 
BEGIN
    UPDATE ResponsibilityTypes SET [Name] = 'Renewal Tenant Signature', [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 'r_renewal_tenant_sign';
    PRINT 'Updated ResponsibilityType: r_renewal_tenant_sign';
END

-- ============================================================
-- 2. UC022: LEASE AGREEMENT REJECTION FLOW (ACTION TYPES & ACTIVITY TRACKERS)
-- ============================================================
-- RCSActionTypes for Rejection Routing
IF NOT EXISTS (SELECT 1 FROM RCSActionTypes WHERE [Key] = 'plm_not_supported_tenant')
BEGIN
    INSERT INTO RCSActionTypes ([Key], [Name], [IsActive], [IsDeleted])
    VALUES ('plm_not_supported_tenant', 'Not Supported - Tenant', 1, 0);
    PRINT 'Inserted RCSActionType: plm_not_supported_tenant';
END
ELSE 
BEGIN
    UPDATE RCSActionTypes SET [Name] = 'Not Supported - Tenant', [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 'plm_not_supported_tenant';
    PRINT 'Updated RCSActionType: plm_not_supported_tenant';
END

IF NOT EXISTS (SELECT 1 FROM RCSActionTypes WHERE [Key] = 'plm_not_supported_cso')
BEGIN
    INSERT INTO RCSActionTypes ([Key], [Name], [IsActive], [IsDeleted])
    VALUES ('plm_not_supported_cso', 'Not Supported - CSO', 1, 0);
    PRINT 'Inserted RCSActionType: plm_not_supported_cso';
END
ELSE 
BEGIN
    UPDATE RCSActionTypes SET [Name] = 'Not Supported - CSO', [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 'plm_not_supported_cso';
    PRINT 'Updated RCSActionType: plm_not_supported_cso';
END

-- Activity Tracker Messages for Rejection
IF NOT EXISTS (SELECT 1 FROM ActivityTrackerMessages WHERE [Key] = 'atm_lease_agreement_rejected_to_tenant')
BEGIN
    INSERT INTO ActivityTrackerMessages ([Key], [Name], [Description], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('atm_lease_agreement_rejected_to_tenant', 'Lease Agreement Rejected (Tenant)', 'Lease agreement rejected and returned to Tenant for corrections', 1, 0, GETDATE());
    PRINT 'Inserted ActivityTrackerMessage: atm_lease_agreement_rejected_to_tenant';
END
ELSE 
BEGIN
    UPDATE ActivityTrackerMessages SET [Name] = 'Lease Agreement Rejected (Tenant)', [Description] = 'Lease agreement rejected and returned to Tenant for corrections', [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 'atm_lease_agreement_rejected_to_tenant';
    PRINT 'Updated ActivityTrackerMessage: atm_lease_agreement_rejected_to_tenant';
END

IF NOT EXISTS (SELECT 1 FROM ActivityTrackerMessages WHERE [Key] = 'atm_lease_agreement_rejected_to_cso')
BEGIN
    INSERT INTO ActivityTrackerMessages ([Key], [Name], [Description], [IsActive], [IsDeleted], [CreatedDateTime])
    VALUES ('atm_lease_agreement_rejected_to_cso', 'Lease Agreement Rejected (CSO)', 'Lease agreement rejected and returned to CSO for lease detail corrections', 1, 0, GETDATE());
    PRINT 'Inserted ActivityTrackerMessage: atm_lease_agreement_rejected_to_cso';
END
ELSE 
BEGIN
    UPDATE ActivityTrackerMessages SET [Name] = 'Lease Agreement Rejected (CSO)', [Description] = 'Lease agreement rejected and returned to CSO for lease detail corrections', [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 'atm_lease_agreement_rejected_to_cso';
    PRINT 'Updated ActivityTrackerMessage: atm_lease_agreement_rejected_to_cso';
END

-- ============================================================
-- 3. BR19 & BR09: TIMEOUTS (STATUS, EMAILS, ACTIVITY TRACKERS)
-- ============================================================
-- Status Records
IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_lease_signing_expired')
BEGIN
    INSERT INTO Status ([Key], [Name], [StatusTypeId], IsActive, IsDeleted, CreatedDateTime)
    VALUES ('s_lease_signing_expired', 'Lease Signing Expired (30-day)', 1, 1, 0, GETDATE());
    PRINT 'Inserted Status: s_lease_signing_expired';
END
ELSE 
BEGIN
    UPDATE Status SET [Name] = 'Lease Signing Expired (30-day)', [StatusTypeId] = 1, [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 's_lease_signing_expired';
    PRINT 'Updated Status: s_lease_signing_expired';
END

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_unit_offer_expired')
BEGIN
    INSERT INTO Status ([Key], [Name], [StatusTypeId], IsActive, IsDeleted, CreatedDateTime)
    VALUES ('s_unit_offer_expired', 'Unit Offer Expired (30-day)', 1, 1, 0, GETDATE());
    PRINT 'Inserted Status: s_unit_offer_expired';
END
ELSE 
BEGIN
    UPDATE Status SET [Name] = 'Unit Offer Expired (30-day)', [StatusTypeId] = 1, [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 's_unit_offer_expired';
    PRINT 'Updated Status: s_unit_offer_expired';
END

-- Email Content Types
IF NOT EXISTS (SELECT 1 FROM EmailContentTypes WHERE [Key] = 'plm_lease_signing_expired')
BEGIN
    INSERT INTO EmailContentTypes ([Key], [Name], [Description], IsActive, IsDeleted, CreatedDateTime)
    VALUES (
        'plm_lease_signing_expired',
        'Lease Signing Expired Notification',
        'BR19: Notification sent to the tenant when the 30-day lease signing deadline has expired and the application is disregarded.',
        1, 0, GETDATE()
    );
    PRINT 'Inserted EmailContentType: plm_lease_signing_expired';
END
ELSE 
BEGIN
    UPDATE EmailContentTypes SET [Name] = 'Lease Signing Expired Notification', [Description] = 'BR19: Notification sent to the tenant when the 30-day lease signing deadline has expired and the application is disregarded.', [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 'plm_lease_signing_expired';
    PRINT 'Updated EmailContentType: plm_lease_signing_expired';
END

IF NOT EXISTS (SELECT 1 FROM EmailContentTypes WHERE [Key] = 'plm_unit_offer_expired')
BEGIN
    INSERT INTO EmailContentTypes ([Key], [Name], [Description], IsActive, IsDeleted, CreatedDateTime)
    VALUES (
        'plm_unit_offer_expired',
        'Unit Offer Expired Notification',
        'BR09: Notification sent to the applicant when the 30-day unit acceptance deadline has expired. The unit offer is withdrawn and the applicant re-listed on the waiting list.',
        1, 0, GETDATE()
    );
    PRINT 'Inserted EmailContentType: plm_unit_offer_expired';
END
ELSE 
BEGIN
    UPDATE EmailContentTypes SET [Name] = 'Unit Offer Expired Notification', [Description] = 'BR09: Notification sent to the applicant when the 30-day unit acceptance deadline has expired. The unit offer is withdrawn and the applicant re-listed on the waiting list.', [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 'plm_unit_offer_expired';
    PRINT 'Updated EmailContentType: plm_unit_offer_expired';
END

-- Activity Tracker Messages for Expirations
IF NOT EXISTS (SELECT 1 FROM ActivityTrackerMessages WHERE [Key] = 'at_lease_signing_expired')
BEGIN
    INSERT INTO ActivityTrackerMessages ([Key], [Name], [Description], IsActive, IsDeleted, CreatedDateTime)
    VALUES (
        'at_lease_signing_expired',
        'Lease Signing Expired',
        'BR19: Application disregarded - lease agreement not signed within 30 days of being sent.',
        1, 0, GETDATE()
    );
    PRINT 'Inserted ActivityTrackerMessage: at_lease_signing_expired';
END
ELSE 
BEGIN
    UPDATE ActivityTrackerMessages SET [Name] = 'Lease Signing Expired', [Description] = 'BR19: Application disregarded - lease agreement not signed within 30 days of being sent.', [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 'at_lease_signing_expired';
    PRINT 'Updated ActivityTrackerMessage: at_lease_signing_expired';
END

IF NOT EXISTS (SELECT 1 FROM ActivityTrackerMessages WHERE [Key] = 'at_unit_offer_expired')
BEGIN
    INSERT INTO ActivityTrackerMessages ([Key], [Name], [Description], IsActive, IsDeleted, CreatedDateTime)
    VALUES (
        'at_unit_offer_expired',
        'Unit Offer Expired',
        'BR09: Unit offer withdrawn - applicant did not accept the matched unit within 30 days. Application re-listed on waiting list.',
        1, 0, GETDATE()
    );
    PRINT 'Inserted ActivityTrackerMessage: at_unit_offer_expired';
END
ELSE 
BEGIN
    UPDATE ActivityTrackerMessages SET [Name] = 'Unit Offer Expired', [Description] = 'BR09: Unit offer withdrawn - applicant did not accept the matched unit within 30 days. Application re-listed on waiting list.', [IsActive] = 1, [IsDeleted] = 0 WHERE [Key] = 'at_unit_offer_expired';
    PRINT 'Updated ActivityTrackerMessage: at_unit_offer_expired';
END

PRINT '--- UNIFIED METADATA MIGRATION COMPLETE ---';
