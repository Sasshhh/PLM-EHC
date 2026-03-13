SELECT m.Id, m.PropertyLeaseApplicationId, m.LeaseDetailsId, m.IsActive, m.IsDeleted,
       m.TenantSigned, m.TenantSignDate, LEFT(ISNULL(m.TenantSignature,'NULL'),40) AS SigPreview,
       l.IsNew, l.IsActive AS LeaseIsActive, l.IsDeleted AS LeaseIsDeleted
FROM dbo.PropertyLeaseAgreementMasters m
LEFT JOIN dbo.LeaseDetails l ON l.Id = m.LeaseDetailsId
WHERE m.PropertyLeaseApplicationId = 4204
ORDER BY m.Id DESC;
