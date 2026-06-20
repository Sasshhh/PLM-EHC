-- ============================================================
-- PLM UC025 & UC026 MIGRATION SCRIPT
-- Serve Eviction Notice / Proof of Service + Manage Disputes
-- ============================================================
-- Run against: eServicesDbContext database
-- Date: 2026-05-21
-- ============================================================

-- ────────────────────────────────────────────────────
-- 1. UC025 STATUS ENTRIES (4 statuses)
-- ────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [dbo].[Status] WHERE [Key] = 's_awaiting_eviction_service')
INSERT INTO [dbo].[Status] ([Name],[Description],[Key],[StatusTypeId],[IsActive],[IsDeleted],[IsLocked],[CreatedDateTime])
VALUES ('Awaiting Eviction Service','Eviction notice approved — awaiting physical service','s_awaiting_eviction_service',
(SELECT TOP 1 Id FROM StatusTypes WHERE [Key]='st_lease_application'),1,0,0,GETDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Status] WHERE [Key] = 's_eviction_notice_served')
INSERT INTO [dbo].[Status] ([Name],[Description],[Key],[StatusTypeId],[IsActive],[IsDeleted],[IsLocked],[CreatedDateTime])
VALUES ('Eviction Notice Served','Notice served — awaiting proof of service','s_eviction_notice_served',
(SELECT TOP 1 Id FROM StatusTypes WHERE [Key]='st_lease_application'),1,0,0,GETDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Status] WHERE [Key] = 's_awaiting_proof_of_service')
INSERT INTO [dbo].[Status] ([Name],[Description],[Key],[StatusTypeId],[IsActive],[IsDeleted],[IsLocked],[CreatedDateTime])
VALUES ('Awaiting Proof of Service','Notice served — proof of service required','s_awaiting_proof_of_service',
(SELECT TOP 1 Id FROM StatusTypes WHERE [Key]='st_lease_application'),1,0,0,GETDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Status] WHERE [Key] = 's_proof_of_service_captured')
INSERT INTO [dbo].[Status] ([Name],[Description],[Key],[StatusTypeId],[IsActive],[IsDeleted],[IsLocked],[CreatedDateTime])
VALUES ('Proof of Service Captured','Proof captured — advancing to exit inspection','s_proof_of_service_captured',
(SELECT TOP 1 Id FROM StatusTypes WHERE [Key]='st_lease_application'),1,0,0,GETDATE());

-- ────────────────────────────────────────────────────
-- 2. UC026 STATUS ENTRIES (4 statuses)
-- ────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [dbo].[Status] WHERE [Key] = 's_dispute_open_awaiting_review')
INSERT INTO [dbo].[Status] ([Name],[Description],[Key],[StatusTypeId],[IsActive],[IsDeleted],[IsLocked],[CreatedDateTime])
VALUES ('Dispute Open — Awaiting Review','New dispute registered, pending CSO review','s_dispute_open_awaiting_review',
(SELECT TOP 1 Id FROM StatusTypes WHERE [Key]='st_lease_application'),1,0,0,GETDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Status] WHERE [Key] = 's_dispute_referred')
INSERT INTO [dbo].[Status] ([Name],[Description],[Key],[StatusTypeId],[IsActive],[IsDeleted],[IsLocked],[CreatedDateTime])
VALUES ('Dispute Referred','Dispute reviewed and referred for resolution','s_dispute_referred',
(SELECT TOP 1 Id FROM StatusTypes WHERE [Key]='st_lease_application'),1,0,0,GETDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Status] WHERE [Key] = 's_dispute_resolved')
INSERT INTO [dbo].[Status] ([Name],[Description],[Key],[StatusTypeId],[IsActive],[IsDeleted],[IsLocked],[CreatedDateTime])
VALUES ('Dispute Resolved','Revenue Manager resolved — awaiting CEO closure','s_dispute_resolved',
(SELECT TOP 1 Id FROM StatusTypes WHERE [Key]='st_lease_application'),1,0,0,GETDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Status] WHERE [Key] = 's_dispute_closed')
INSERT INTO [dbo].[Status] ([Name],[Description],[Key],[StatusTypeId],[IsActive],[IsDeleted],[IsLocked],[CreatedDateTime])
VALUES ('Dispute Closed','Dispute closed by CEO','s_dispute_closed',
(SELECT TOP 1 Id FROM StatusTypes WHERE [Key]='st_lease_application'),1,0,0,GETDATE());

-- ────────────────────────────────────────────────────
-- 3. RESPONSIBILITY TYPE ENTRIES (5 types)
-- ────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [dbo].[ResponsibilityTypes] WHERE [Key] = 'r_eviction_service')
INSERT INTO [dbo].[ResponsibilityTypes] ([Name],[Description],[Key],[IsActive],[IsDeleted],[IsLocked],[CreatedDateTime])
VALUES ('Eviction Service','UC025 — Serve eviction notice','r_eviction_service',1,0,0,GETDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[ResponsibilityTypes] WHERE [Key] = 'r_proof_of_service')
INSERT INTO [dbo].[ResponsibilityTypes] ([Name],[Description],[Key],[IsActive],[IsDeleted],[IsLocked],[CreatedDateTime])
VALUES ('Proof of Service','UC025 — Capture proof of service','r_proof_of_service',1,0,0,GETDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[ResponsibilityTypes] WHERE [Key] = 'r_dispute_review')
INSERT INTO [dbo].[ResponsibilityTypes] ([Name],[Description],[Key],[IsActive],[IsDeleted],[IsLocked],[CreatedDateTime])
VALUES ('Dispute Review','UC026 — CSO dispute review','r_dispute_review',1,0,0,GETDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[ResponsibilityTypes] WHERE [Key] = 'r_dispute_resolution')
INSERT INTO [dbo].[ResponsibilityTypes] ([Name],[Description],[Key],[IsActive],[IsDeleted],[IsLocked],[CreatedDateTime])
VALUES ('Dispute Resolution','UC026 — RM dispute resolution','r_dispute_resolution',1,0,0,GETDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[ResponsibilityTypes] WHERE [Key] = 'r_dispute_closure')
INSERT INTO [dbo].[ResponsibilityTypes] ([Name],[Description],[Key],[IsActive],[IsDeleted],[IsLocked],[CreatedDateTime])
VALUES ('Dispute Closure','UC026 — CEO dispute closure','r_dispute_closure',1,0,0,GETDATE());

-- ────────────────────────────────────────────────────
-- 4. ACTIVITY TRACKER MESSAGE ENTRIES (9 messages)
-- ────────────────────────────────────────────────────
-- UC025
IF NOT EXISTS (SELECT 1 FROM [dbo].[ActivityTrackerMessages] WHERE [Key] = 'at_eviction_notice_served')
INSERT INTO [dbo].[ActivityTrackerMessages] ([Name],[Description],[Key],[IsActive],[IsDeleted],[IsLocked],[CreatedDateTime])
VALUES ('Eviction Notice Served','The eviction notice has been served on the tenant','at_eviction_notice_served',1,0,0,GETDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[ActivityTrackerMessages] WHERE [Key] = 'at_proof_of_service_captured')
INSERT INTO [dbo].[ActivityTrackerMessages] ([Name],[Description],[Key],[IsActive],[IsDeleted],[IsLocked],[CreatedDateTime])
VALUES ('Proof of Service Captured','Proof of eviction notice service has been captured','at_proof_of_service_captured',1,0,0,GETDATE());

-- UC026
IF NOT EXISTS (SELECT 1 FROM [dbo].[ActivityTrackerMessages] WHERE [Key] = 'at_dispute_registered')
INSERT INTO [dbo].[ActivityTrackerMessages] ([Name],[Description],[Key],[IsActive],[IsDeleted],[IsLocked],[CreatedDateTime])
VALUES ('Dispute Registered','A lease dispute has been registered','at_dispute_registered',1,0,0,GETDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[ActivityTrackerMessages] WHERE [Key] = 'at_dispute_reviewed')
INSERT INTO [dbo].[ActivityTrackerMessages] ([Name],[Description],[Key],[IsActive],[IsDeleted],[IsLocked],[CreatedDateTime])
VALUES ('Dispute Reviewed','The dispute has been reviewed by CSO','at_dispute_reviewed',1,0,0,GETDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[ActivityTrackerMessages] WHERE [Key] = 'at_dispute_referred_legal')
INSERT INTO [dbo].[ActivityTrackerMessages] ([Name],[Description],[Key],[IsActive],[IsDeleted],[IsLocked],[CreatedDateTime])
VALUES ('Dispute Referred to Legal','The dispute has been referred to legal services','at_dispute_referred_legal',1,0,0,GETDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[ActivityTrackerMessages] WHERE [Key] = 'at_dispute_resolved')
INSERT INTO [dbo].[ActivityTrackerMessages] ([Name],[Description],[Key],[IsActive],[IsDeleted],[IsLocked],[CreatedDateTime])
VALUES ('Dispute Resolved','The dispute has been resolved by Revenue Manager','at_dispute_resolved',1,0,0,GETDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[ActivityTrackerMessages] WHERE [Key] = 'at_dispute_not_resolved')
INSERT INTO [dbo].[ActivityTrackerMessages] ([Name],[Description],[Key],[IsActive],[IsDeleted],[IsLocked],[CreatedDateTime])
VALUES ('Dispute Not Resolved','Revenue Manager could not resolve the dispute','at_dispute_not_resolved',1,0,0,GETDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[ActivityTrackerMessages] WHERE [Key] = 'at_dispute_closed_ceo')
INSERT INTO [dbo].[ActivityTrackerMessages] ([Name],[Description],[Key],[IsActive],[IsDeleted],[IsLocked],[CreatedDateTime])
VALUES ('Dispute Closed by CEO','The dispute has been officially closed by the CEO','at_dispute_closed_ceo',1,0,0,GETDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[ActivityTrackerMessages] WHERE [Key] = 'at_dispute_rejected_ceo')
INSERT INTO [dbo].[ActivityTrackerMessages] ([Name],[Description],[Key],[IsActive],[IsDeleted],[IsLocked],[CreatedDateTime])
VALUES ('Dispute Rejected by CEO','The CEO has rejected the dispute closure','at_dispute_rejected_ceo',1,0,0,GETDATE());

PRINT '── UC025/UC026 Migration Complete ──';
GO
