-- DPRE-Q21: documents attached to a lease record in the lease register (Lease Details > Lease Documents).
-- Adds the reference type used to link those documents to RE_Leases. Safe to run more than once.

IF NOT EXISTS (SELECT 1 FROM dbo.ReferenceTypes WHERE [Key] = 'rt_real_estate_lease')
    INSERT INTO dbo.ReferenceTypes ([Name], [Description], [Key], [IsActive], [IsDeleted], [IsLocked], [CreatedBySystemUserId], [CreatedDateTime])
    VALUES (N'Real Estate Lease', N'Real Estate lease register record (RE_Leases)', N'rt_real_estate_lease', 1, 0, 0, 1, GETDATE());
GO
