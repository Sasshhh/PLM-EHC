-- UC021 Lease Renewal Master Data
-- Run this on PRE_LIVE/LIVE to ensure the renewal queues are visible to the respective roles

INSERT INTO ResponsibilityTypes ([Key], [Name], [IsActive], [IsDeleted])
VALUES
  ('r_renewal_rm_sign',     'Renewal RM Signature',     1, 0),
  ('r_renewal_ceo_sign',    'Renewal CEO Signature',    1, 0),
  ('r_renewal_tenant_sign', 'Renewal Tenant Signature', 1, 0)
