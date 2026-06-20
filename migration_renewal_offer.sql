-- Migration: ExtendPropertyLeaseRenewalOffer_ApprovalTrail
-- Run this in SSMS against CRMPLMDEV_2025

ALTER TABLE PropertyLeaseRenewalOffers
    ADD ProposedEndDate           DATETIME2 NULL,
        ProposedRenewalNotice     DATETIME2 NULL,
        ProposedTerminationNotice DATETIME2 NULL,
        CSO_Outcome               NVARCHAR(200) NULL,
        CSO_Comment               NVARCHAR(MAX) NULL,
        CSO_Date                  DATETIME2 NULL,
        CSO_SystemUserId          INT NULL,
        RM_Outcome                NVARCHAR(200) NULL,
        RM_Comment                NVARCHAR(MAX) NULL,
        RM_Date                   DATETIME2 NULL,
        RM_SystemUserId           INT NULL,
        CEO_Outcome               NVARCHAR(200) NULL,
        CEO_Comment               NVARCHAR(MAX) NULL,
        CEO_Date                  DATETIME2 NULL,
        CEO_SystemUserId          INT NULL,
        CustomerDeclineReason     NVARCHAR(MAX) NULL,
        CustomerResponseDate      DATETIME2 NULL;
