IF NOT EXISTS (SELECT 1 FROM [Status] WHERE [Key] = 're_supported')
    INSERT INTO [Status] ([Name], [Key], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime]) VALUES ('Supported', 're_supported', 20, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM [Status] WHERE [Key] = 're_supported_conditions')
    INSERT INTO [Status] ([Name], [Key], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime]) VALUES ('Supported with Conditions', 're_supported_conditions', 20, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM [Status] WHERE [Key] = 're_not_supported')
    INSERT INTO [Status] ([Name], [Key], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime]) VALUES ('Not Supported', 're_not_supported', 20, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM [Status] WHERE [Key] = 're_additional_info_req')
    INSERT INTO [Status] ([Name], [Key], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime]) VALUES ('Request Additional Information', 're_additional_info_req', 20, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM [Status] WHERE [Key] = 're_pending_committee_outcome')
    INSERT INTO [Status] ([Name], [Key], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime]) VALUES ('Pending Evaluation Committee Outcome', 're_pending_committee_outcome', 20, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM [Status] WHERE [Key] = 're_recommended')
    INSERT INTO [Status] ([Name], [Key], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime]) VALUES ('Recommended', 're_recommended', 20, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM [Status] WHERE [Key] = 're_recommended_conditions')
    INSERT INTO [Status] ([Name], [Key], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime]) VALUES ('Recommended with Conditions', 're_recommended_conditions', 20, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM [Status] WHERE [Key] = 're_not_recommended')
    INSERT INTO [Status] ([Name], [Key], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime]) VALUES ('Not Recommended', 're_not_recommended', 20, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM [Status] WHERE [Key] = 're_deferred')
    INSERT INTO [Status] ([Name], [Key], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime]) VALUES ('Deferred', 're_deferred', 20, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM [Status] WHERE [Key] = 're_concluded_approved')
    INSERT INTO [Status] ([Name], [Key], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime]) VALUES ('Application Concluded & Approved', 're_concluded_approved', 20, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM [Status] WHERE [Key] = 're_concluded_rejected')
    INSERT INTO [Status] ([Name], [Key], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime]) VALUES ('Application Concluded & Rejected', 're_concluded_rejected', 20, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM [Status] WHERE [Key] = 're_awaiting_inspection')
    INSERT INTO [Status] ([Name], [Key], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime]) VALUES ('Awaiting Inspection', 're_awaiting_inspection', 20, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM [Status] WHERE [Key] = 're_awaiting_agreement_conclusion')
    INSERT INTO [Status] ([Name], [Key], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime]) VALUES ('Awaiting Lease/User Agreement Conclusion', 're_awaiting_agreement_conclusion', 20, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM [Status] WHERE [Key] = 're_awaiting_pto_review')
    INSERT INTO [Status] ([Name], [Key], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime]) VALUES ('Awaiting PTO Review', 're_awaiting_pto_review', 20, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM [Status] WHERE [Key] = 're_awaiting_pto_approval')
    INSERT INTO [Status] ([Name], [Key], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime]) VALUES ('Awaiting PTO Approval', 're_awaiting_pto_approval', 20, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM [Status] WHERE [Key] = 're_pto_approved')
    INSERT INTO [Status] ([Name], [Key], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime]) VALUES ('PTO Approved', 're_pto_approved', 20, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM [Status] WHERE [Key] = 're_pto_rejected')
    INSERT INTO [Status] ([Name], [Key], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime]) VALUES ('PTO Rejected', 're_pto_rejected', 20, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM [Status] WHERE [Key] = 're_pto_approved_conditions')
    INSERT INTO [Status] ([Name], [Key], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime]) VALUES ('PTO Approved with Conditions', 're_pto_approved_conditions', 20, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM [Status] WHERE [Key] = 're_pto_additional_info')
    INSERT INTO [Status] ([Name], [Key], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime]) VALUES ('Awaiting PTO Review, Additional Information Required', 're_pto_additional_info', 20, 1, 0, GETDATE());
