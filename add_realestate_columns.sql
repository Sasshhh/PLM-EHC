-- Committee Review
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'CommitteeDecision')
    ALTER TABLE RE_Applications ADD CommitteeDecision nvarchar(100) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'CommitteeComments')
    ALTER TABLE RE_Applications ADD CommitteeComments nvarchar(max) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'CommitteeResolutionFileId')
    ALTER TABLE RE_Applications ADD CommitteeResolutionFileId int NULL;

-- Final Authorization
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'FinalOutcome')
    ALTER TABLE RE_Applications ADD FinalOutcome nvarchar(100) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'FinalComments')
    ALTER TABLE RE_Applications ADD FinalComments nvarchar(max) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'FinalSignature')
    ALTER TABLE RE_Applications ADD FinalSignature nvarchar(max) NULL;

-- Inspections
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'InspectionType')
    ALTER TABLE RE_Applications ADD InspectionType nvarchar(100) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'InspectionDate')
    ALTER TABLE RE_Applications ADD InspectionDate datetime NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'InspectionTime')
    ALTER TABLE RE_Applications ADD InspectionTime nvarchar(100) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'InspectionStatus')
    ALTER TABLE RE_Applications ADD InspectionStatus nvarchar(100) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'InspectionComments')
    ALTER TABLE RE_Applications ADD InspectionComments nvarchar(max) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'InspectionPlumbing')
    ALTER TABLE RE_Applications ADD InspectionPlumbing nvarchar(100) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'InspectionElectrical')
    ALTER TABLE RE_Applications ADD InspectionElectrical nvarchar(100) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'InspectionFixtures')
    ALTER TABLE RE_Applications ADD InspectionFixtures nvarchar(100) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'InspectionSanitation')
    ALTER TABLE RE_Applications ADD InspectionSanitation nvarchar(100) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'InspectionHazards')
    ALTER TABLE RE_Applications ADD InspectionHazards nvarchar(100) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'InspectionWearTear')
    ALTER TABLE RE_Applications ADD InspectionWearTear nvarchar(100) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'InspectionFormFileId')
    ALTER TABLE RE_Applications ADD InspectionFormFileId int NULL;

-- Works Orders / Maintenance
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'WorkOrderNumber')
    ALTER TABLE RE_Applications ADD WorkOrderNumber nvarchar(100) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'WorkOrderStatus')
    ALTER TABLE RE_Applications ADD WorkOrderStatus nvarchar(100) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'WorkOrderTasks')
    ALTER TABLE RE_Applications ADD WorkOrderTasks nvarchar(max) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'WorkOrderIssueDescription')
    ALTER TABLE RE_Applications ADD WorkOrderIssueDescription nvarchar(max) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'WorkOrderPriority')
    ALTER TABLE RE_Applications ADD WorkOrderPriority nvarchar(100) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'WorkOrderDueDate')
    ALTER TABLE RE_Applications ADD WorkOrderDueDate datetime NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'WorkOrderMaterials')
    ALTER TABLE RE_Applications ADD WorkOrderMaterials nvarchar(max) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'WorkOrderSafetyInstructions')
    ALTER TABLE RE_Applications ADD WorkOrderSafetyInstructions nvarchar(max) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'WorkOrderAssignmentType')
    ALTER TABLE RE_Applications ADD WorkOrderAssignmentType nvarchar(100) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'WorkOrderTechnicianName')
    ALTER TABLE RE_Applications ADD WorkOrderTechnicianName nvarchar(250) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'WorkOrderRejectionReason')
    ALTER TABLE RE_Applications ADD WorkOrderRejectionReason nvarchar(max) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'WorkOrderJobSheetFileId')
    ALTER TABLE RE_Applications ADD WorkOrderJobSheetFileId int NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'WorkOrderManagerComments')
    ALTER TABLE RE_Applications ADD WorkOrderManagerComments nvarchar(max) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'WorkOrderManagerSignature')
    ALTER TABLE RE_Applications ADD WorkOrderManagerSignature nvarchar(max) NULL;

-- Permission to Occupy (PTO)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'PtoReferenceNumber')
    ALTER TABLE RE_Applications ADD PtoReferenceNumber nvarchar(100) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'PtoPurposeOfOccupation')
    ALTER TABLE RE_Applications ADD PtoPurposeOfOccupation nvarchar(max) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'PtoStartDate')
    ALTER TABLE RE_Applications ADD PtoStartDate datetime NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'PtoEndDate')
    ALTER TABLE RE_Applications ADD PtoEndDate datetime NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'PtoAcceptedIndemnity')
    ALTER TABLE RE_Applications ADD PtoAcceptedIndemnity bit NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'PtoStatus')
    ALTER TABLE RE_Applications ADD PtoStatus nvarchar(100) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'PtoReviewRecommendation')
    ALTER TABLE RE_Applications ADD PtoReviewRecommendation nvarchar(100) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'PtoReviewReason')
    ALTER TABLE RE_Applications ADD PtoReviewReason nvarchar(max) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'PtoDecision')
    ALTER TABLE RE_Applications ADD PtoDecision nvarchar(100) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'PtoDecisionReason')
    ALTER TABLE RE_Applications ADD PtoDecisionReason nvarchar(max) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RE_Applications') AND name = 'PtoSignature')
    ALTER TABLE RE_Applications ADD PtoSignature nvarchar(max) NULL;
