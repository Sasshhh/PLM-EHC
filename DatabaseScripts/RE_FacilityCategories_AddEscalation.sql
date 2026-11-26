-- DPRE-Q11 (BA meeting 26 Nov 2026): rental tariffs and escalation are set in the Administrator console.
-- Adds an annual escalation % per facility category (and its audit table). Safe to run more than once.

IF COL_LENGTH('dbo.RE_FacilityCategories', 'EscalationPercentage') IS NULL
    ALTER TABLE dbo.RE_FacilityCategories ADD EscalationPercentage DECIMAL(18, 2) NULL;
GO
IF OBJECT_ID('dbo.RE_FacilityCategoryAudits', 'U') IS NOT NULL AND COL_LENGTH('dbo.RE_FacilityCategoryAudits', 'EscalationPercentage') IS NULL
    ALTER TABLE dbo.RE_FacilityCategoryAudits ADD EscalationPercentage DECIMAL(18, 2) NULL;
GO
