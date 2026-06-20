IF NOT EXISTS (SELECT 1 FROM [Status] WHERE [Key] = 's_awaiting_renewal_lease_capture')
    INSERT INTO [Status] ([Name], [Key], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime]) VALUES ('Awaiting Renewal Lease Capture', 's_awaiting_renewal_lease_capture', 20, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM [Status] WHERE [Key] = 's_awaiting_renewal_agreement_generation')
    INSERT INTO [Status] ([Name], [Key], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime]) VALUES ('Awaiting Renewal Agreement Generation', 's_awaiting_renewal_agreement_generation', 20, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM [Status] WHERE [Key] = 's_awaiting_renewal_tenant_signature')
    INSERT INTO [Status] ([Name], [Key], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime]) VALUES ('Awaiting Renewal Tenant Signature', 's_awaiting_renewal_tenant_signature', 20, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM [Status] WHERE [Key] = 's_awaiting_renewal_rm_signature')
    INSERT INTO [Status] ([Name], [Key], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime]) VALUES ('Awaiting Renewal RM Signature', 's_awaiting_renewal_rm_signature', 20, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM [Status] WHERE [Key] = 's_awaiting_renewal_ceo_approval')
    INSERT INTO [Status] ([Name], [Key], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime]) VALUES ('Awaiting Renewal CEO Approval', 's_awaiting_renewal_ceo_approval', 20, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM [Status] WHERE [Key] = 's_lease_renewal_agreement_concluded')
    INSERT INTO [Status] ([Name], [Key], [StatusTypeId], [IsActive], [IsDeleted], [CreatedDateTime]) VALUES ('Lease Renewal Agreement Concluded', 's_lease_renewal_agreement_concluded', 20, 1, 0, GETDATE());
