-- ============================================================================
-- MASTER PROD MIGRATION SCRIPT 
-- Target: CRMPLM_PROD (Production database)
-- Includes: UC022 Reject Flow, BR19/BR09 Expired Statuses & Tracker Messages
-- ============================================================================

BEGIN TRANSACTION;

-- ============================================================
-- 1. UC022 Reject Flow: Action Types
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM RCSActionTypes WHERE [Key] = 'plm_not_supported_tenant')
BEGIN
    INSERT INTO RCSActionTypes ([Key], [Name], [IsActive], [IsDeleted])
    VALUES ('plm_not_supported_tenant', 'Not Supported - Due to Tenant', 1, 0);
    PRINT 'Inserted RCSActionType: plm_not_supported_tenant';
END
ELSE PRINT 'RCSActionType plm_not_supported_tenant already exists';

IF NOT EXISTS (SELECT 1 FROM RCSActionTypes WHERE [Key] = 'plm_not_supported_cso')
BEGIN
    INSERT INTO RCSActionTypes ([Key], [Name], [IsActive], [IsDeleted])
    VALUES ('plm_not_supported_cso', 'Not Supported - Due to CSO', 1, 0);
    PRINT 'Inserted RCSActionType: plm_not_supported_cso';
END
ELSE PRINT 'RCSActionType plm_not_supported_cso already exists';


-- ============================================================
-- 2. UC022 Reject Flow: Activity Tracker Messages
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM ActivityTrackerMessages WHERE [Key] = 'at_lease_agreement_rejected_tenant')
BEGIN
    INSERT INTO ActivityTrackerMessages ([Key], [Name], [Description], [IsActive], [IsDeleted])
    VALUES ('at_lease_agreement_rejected_tenant',
            'Lease Agreement rejected - returned to Tenant for corrections',
            'Lease Agreement rejected - returned to Tenant for corrections',
            1, 0);
    PRINT 'Inserted ActivityTrackerMessage: at_lease_agreement_rejected_tenant';
END
ELSE PRINT 'ActivityTrackerMessage at_lease_agreement_rejected_tenant already exists';

IF NOT EXISTS (SELECT 1 FROM ActivityTrackerMessages WHERE [Key] = 'at_lease_agreement_rejected_cso')
BEGIN
    INSERT INTO ActivityTrackerMessages ([Key], [Name], [Description], [IsActive], [IsDeleted])
    VALUES ('at_lease_agreement_rejected_cso',
            'Lease Agreement rejected - returned to CSO for corrections',
            'Lease Agreement rejected - returned to CSO for corrections',
            1, 0);
    PRINT 'Inserted ActivityTrackerMessage: at_lease_agreement_rejected_cso';
END
ELSE PRINT 'ActivityTrackerMessage at_lease_agreement_rejected_cso already exists';


-- ============================================================
-- 3. BR19 & BR09: Status Records
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_lease_signing_expired')
BEGIN
    INSERT INTO Status ([Key], [Name], IsActive, IsDeleted, CreatedDateTime)
    VALUES ('s_lease_signing_expired', 'Lease Signing Expired (30-day)', 1, 0, GETDATE());
    PRINT 'Inserted Status: s_lease_signing_expired';
END
ELSE PRINT 'Status s_lease_signing_expired already exists';

IF NOT EXISTS (SELECT 1 FROM Status WHERE [Key] = 's_unit_offer_expired')
BEGIN
    INSERT INTO Status ([Key], [Name], IsActive, IsDeleted, CreatedDateTime)
    VALUES ('s_unit_offer_expired', 'Unit Offer Expired (30-day)', 1, 0, GETDATE());
    PRINT 'Inserted Status: s_unit_offer_expired';
END
ELSE PRINT 'Status s_unit_offer_expired already exists';


-- ============================================================
-- 4. BR19 & BR09: Email Content Types
-- ============================================================
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
ELSE PRINT 'EmailContentType plm_lease_signing_expired already exists';

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
ELSE PRINT 'EmailContentType plm_unit_offer_expired already exists';


-- ============================================================
-- 5. BR19 & BR09: Activity Tracker Messages
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM ActivityTrackerMessages WHERE [Key] = 'at_lease_signing_expired')
BEGIN
    INSERT INTO ActivityTrackerMessages ([Key], [Description], IsActive, IsDeleted, CreatedDateTime)
    VALUES (
        'at_lease_signing_expired',
        'BR19: Application disregarded — lease agreement not signed within 30 days of being sent.',
        1, 0, GETDATE()
    );
    PRINT 'Inserted ActivityTrackerMessage: at_lease_signing_expired';
END
ELSE PRINT 'ActivityTrackerMessage at_lease_signing_expired already exists';

IF NOT EXISTS (SELECT 1 FROM ActivityTrackerMessages WHERE [Key] = 'at_unit_offer_expired')
BEGIN
    INSERT INTO ActivityTrackerMessages ([Key], [Description], IsActive, IsDeleted, CreatedDateTime)
    VALUES (
        'at_unit_offer_expired',
        'BR09: Unit offer withdrawn — applicant did not accept the matched unit within 30 days. Application re-listed on waiting list.',
        1, 0, GETDATE()
    );
    PRINT 'Inserted ActivityTrackerMessage: at_unit_offer_expired';
END
ELSE PRINT 'ActivityTrackerMessage at_unit_offer_expired already exists';


COMMIT TRANSACTION;
PRINT '';
PRINT '--- Production Master Migration Data Seed Complete ---';
GO
