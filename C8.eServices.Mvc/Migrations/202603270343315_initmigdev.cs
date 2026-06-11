namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initmigdev : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MaintenanceJobCardSignatures",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AllocatedUnitMaintenanceEHCId = c.Int(nullable: false),
                        OfficialNumber = c.String(nullable: false, maxLength: 50),
                        ApprovalAction = c.String(nullable: false, maxLength: 20),
                        Reason = c.String(nullable: false, maxLength: 1000),
                        SignatureData = c.String(nullable: false),
                        ApprovalDate = c.DateTime(nullable: false),
                        SignedByCustomerId = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        CreatedBySystemUserId = c.Int(nullable: false),
                        CreatedDateTime = c.DateTime(nullable: false),
                        ModifiedBySystemUserId = c.Int(),
                        ModifiedDateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AllocatedUnitMaintenanceEHCs", t => t.AllocatedUnitMaintenanceEHCId)
                .ForeignKey("dbo.SystemUsers", t => t.CreatedBySystemUserId)
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .ForeignKey("dbo.Customers", t => t.SignedByCustomerId)
                .Index(t => t.AllocatedUnitMaintenanceEHCId)
                .Index(t => t.SignedByCustomerId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId);
            
            CreateTable(
                "dbo.MaintenanceJobCardTasks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AllocatedUnitMaintenanceEHCId = c.Int(nullable: false),
                        StartDate = c.DateTime(nullable: false, storeType: "date"),
                        EndDate = c.DateTime(nullable: false, storeType: "date"),
                        Activity = c.String(nullable: false, maxLength: 500),
                        MaterialUsed = c.String(maxLength: 200),
                        QuantityUsed = c.Decimal(precision: 18, scale: 2),
                        LabourUsed = c.String(maxLength: 200),
                        TotalCosts = c.Decimal(precision: 18, scale: 2),
                        TaskComments = c.String(maxLength: 1000),
                        SupportingDocumentId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        CreatedBySystemUserId = c.Int(nullable: false),
                        CreatedDateTime = c.DateTime(nullable: false),
                        ModifiedBySystemUserId = c.Int(),
                        ModifiedDateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AllocatedUnitMaintenanceEHCs", t => t.AllocatedUnitMaintenanceEHCId)
                .ForeignKey("dbo.SystemUsers", t => t.CreatedBySystemUserId)
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .ForeignKey("dbo.Documents", t => t.SupportingDocumentId)
                .Index(t => t.AllocatedUnitMaintenanceEHCId)
                .Index(t => t.SupportingDocumentId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId);
            
            CreateTable(
                "dbo.ComplaintCategories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Key = c.String(maxLength: 50),
                        Description = c.String(),
                        DisplayOrder = c.Int(),
                        DepartmentId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsLocked = c.Boolean(),
                        CreatedBySystemUserId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedBySystemUserId = c.Int(),
                        ModifiedDateTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SystemUsers", t => t.CreatedBySystemUserId)
                .ForeignKey("dbo.ApplicationEntities", t => t.DepartmentId)
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .Index(t => t.DepartmentId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId);
            
            CreateTable(
                "dbo.TenantComplaints",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CaseReferenceNumber = c.String(nullable: false, maxLength: 50),
                        OfficialNumber = c.String(),
                        ComplainantComplexId = c.Int(nullable: false),
                        ComplainantFirstName = c.String(nullable: false, maxLength: 100),
                        ComplainantSurname = c.String(nullable: false, maxLength: 100),
                        ComplainantEmail = c.String(maxLength: 100),
                        ComplainantCellphone = c.String(maxLength: 20),
                        ComplainantUnitNumber = c.String(maxLength: 20),
                        ComplainantBlockNumber = c.String(maxLength: 20),
                        RespondentComplexId = c.Int(nullable: false),
                        RespondentBlockNumber = c.String(nullable: false, maxLength: 20),
                        RespondentUnitNumber = c.String(nullable: false, maxLength: 20),
                        RespondentFirstName = c.String(maxLength: 100),
                        RespondentSurname = c.String(maxLength: 100),
                        ComplaintCategoryId = c.Int(nullable: false),
                        ComplaintTypeId = c.Int(),
                        DetailedDescription = c.String(nullable: false),
                        StatusId = c.Int(nullable: false),
                        SubmittedByCustomerId = c.Int(),
                        SubmittedByUserId = c.Int(),
                        DateSubmitted = c.DateTime(),
                        AssignedToId = c.Int(),
                        DateAssigned = c.DateTime(),
                        DepartmentId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsLocked = c.Boolean(),
                        CreatedBySystemUserId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedBySystemUserId = c.Int(),
                        ModifiedDateTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Customers", t => t.AssignedToId)
                .ForeignKey("dbo.PreferredComplexAreas", t => t.ComplainantComplexId)
                .ForeignKey("dbo.ComplaintCategories", t => t.ComplaintCategoryId)
                .ForeignKey("dbo.ComplaintTypes", t => t.ComplaintTypeId)
                .ForeignKey("dbo.SystemUsers", t => t.CreatedBySystemUserId)
                .ForeignKey("dbo.ApplicationEntities", t => t.DepartmentId)
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .ForeignKey("dbo.PreferredComplexAreas", t => t.RespondentComplexId)
                .ForeignKey("dbo.Status", t => t.StatusId)
                .ForeignKey("dbo.Customers", t => t.SubmittedByCustomerId)
                .Index(t => t.ComplainantComplexId)
                .Index(t => t.RespondentComplexId)
                .Index(t => t.ComplaintCategoryId)
                .Index(t => t.ComplaintTypeId)
                .Index(t => t.StatusId)
                .Index(t => t.SubmittedByCustomerId)
                .Index(t => t.AssignedToId)
                .Index(t => t.DepartmentId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId);
            
            CreateTable(
                "dbo.ComplaintTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ComplaintCategoryId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Key = c.String(maxLength: 50),
                        Description = c.String(),
                        DisplayOrder = c.Int(),
                        DepartmentId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsLocked = c.Boolean(),
                        CreatedBySystemUserId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedBySystemUserId = c.Int(),
                        ModifiedDateTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ComplaintCategories", t => t.ComplaintCategoryId)
                .ForeignKey("dbo.SystemUsers", t => t.CreatedBySystemUserId)
                .ForeignKey("dbo.ApplicationEntities", t => t.DepartmentId)
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .Index(t => t.ComplaintCategoryId)
                .Index(t => t.DepartmentId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId);
            
            CreateTable(
                "dbo.ComplaintEvidences",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TenantComplaintId = c.Int(nullable: false),
                        FileName = c.String(nullable: false, maxLength: 255),
                        FileType = c.String(nullable: false, maxLength: 100),
                        FilePath = c.String(),
                        FileSize = c.Long(),
                        UploadedById = c.Int(),
                        UploadDate = c.DateTime(),
                        Description = c.String(maxLength: 500),
                        DepartmentId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsLocked = c.Boolean(),
                        CreatedBySystemUserId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedBySystemUserId = c.Int(),
                        ModifiedDateTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SystemUsers", t => t.CreatedBySystemUserId)
                .ForeignKey("dbo.ApplicationEntities", t => t.DepartmentId)
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .ForeignKey("dbo.TenantComplaints", t => t.TenantComplaintId)
                .ForeignKey("dbo.Customers", t => t.UploadedById)
                .Index(t => t.TenantComplaintId)
                .Index(t => t.UploadedById)
                .Index(t => t.DepartmentId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId);
            
            CreateTable(
                "dbo.ComplaintInvestigations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TenantComplaintId = c.Int(nullable: false),
                        AppointmentDate = c.DateTime(),
                        AppointmentTime = c.Time(precision: 7),
                        ScheduledById = c.Int(),
                        DateScheduled = c.DateTime(),
                        RespondentConfirmed = c.Boolean(nullable: false),
                        DateConfirmed = c.DateTime(),
                        Outcome = c.String(maxLength: 50),
                        OutcomeDetails = c.String(),
                        OutcomeDate = c.DateTime(),
                        InvestigatedById = c.Int(),
                        ExternalReferralId = c.Int(),
                        DepartmentId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsLocked = c.Boolean(),
                        CreatedBySystemUserId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedBySystemUserId = c.Int(),
                        ModifiedDateTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SystemUsers", t => t.CreatedBySystemUserId)
                .ForeignKey("dbo.ApplicationEntities", t => t.DepartmentId)
                .ForeignKey("dbo.ComplaintExternalReferrals", t => t.ExternalReferralId)
                .ForeignKey("dbo.Customers", t => t.InvestigatedById)
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .ForeignKey("dbo.Customers", t => t.ScheduledById)
                .ForeignKey("dbo.TenantComplaints", t => t.TenantComplaintId)
                .Index(t => t.TenantComplaintId)
                .Index(t => t.ScheduledById)
                .Index(t => t.InvestigatedById)
                .Index(t => t.ExternalReferralId)
                .Index(t => t.DepartmentId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId);
            
            CreateTable(
                "dbo.ComplaintInvestigationDocuments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ComplaintInvestigationId = c.Int(nullable: false),
                        FileName = c.String(nullable: false, maxLength: 255),
                        FileType = c.String(nullable: false, maxLength: 100),
                        FilePath = c.String(),
                        FileSize = c.Long(),
                        UploadedById = c.Int(),
                        UploadDate = c.DateTime(),
                        Description = c.String(maxLength: 500),
                        DepartmentId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsLocked = c.Boolean(),
                        CreatedBySystemUserId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedBySystemUserId = c.Int(),
                        ModifiedDateTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ComplaintInvestigations", t => t.ComplaintInvestigationId)
                .ForeignKey("dbo.SystemUsers", t => t.CreatedBySystemUserId)
                .ForeignKey("dbo.ApplicationEntities", t => t.DepartmentId)
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .ForeignKey("dbo.Customers", t => t.UploadedById)
                .Index(t => t.ComplaintInvestigationId)
                .Index(t => t.UploadedById)
                .Index(t => t.DepartmentId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId);
            
            CreateTable(
                "dbo.ComplaintExternalReferrals",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AgencyName = c.String(nullable: false, maxLength: 200),
                        Address = c.String(),
                        ContactPerson = c.String(maxLength: 100),
                        ContactNumber = c.String(maxLength: 20),
                        ContactEmail = c.String(maxLength: 100),
                        BriefDescription = c.String(),
                        ReferredById = c.Int(),
                        DateReferred = c.DateTime(),
                        DepartmentId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsLocked = c.Boolean(),
                        CreatedBySystemUserId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedBySystemUserId = c.Int(),
                        ModifiedDateTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SystemUsers", t => t.CreatedBySystemUserId)
                .ForeignKey("dbo.ApplicationEntities", t => t.DepartmentId)
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .ForeignKey("dbo.Customers", t => t.ReferredById)
                .Index(t => t.ReferredById)
                .Index(t => t.DepartmentId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId);
            
            AddColumn("dbo.PreferredComplexAreas", "MaintenanceManagerId", c => c.Int());
            AddColumn("dbo.PropertyLeaseApplications", "HasDSTV", c => c.Boolean());
            AddColumn("dbo.PropertyLeaseApplications", "DSTVActivationFee", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseApplications", "DSTVMonthlyLevy", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseApplications", "AccessCardDeposit", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseApplications", "KeyDeposit", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.AllocatedUnitMaintenanceEHCs", "BeforeImageId", c => c.Int());
            AddColumn("dbo.AllocatedUnitMaintenanceEHCs", "AfterImageId", c => c.Int());
            AddColumn("dbo.AllocatedUnitMaintenanceEHCs", "JobCardSubmitted", c => c.Boolean());
            AddColumn("dbo.AllocatedUnitMaintenanceEHCs", "JobCardSubmittedDate", c => c.DateTime());
            AddColumn("dbo.AllocatedUnitMaintenanceEHCs", "Inspection", c => c.Boolean());
            AddColumn("dbo.PreferredComplexAreaAudits", "MaintenanceManagerId", c => c.Int());
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "PropertyManagerSignatureDate", c => c.DateTime());
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "RevenueManagerSignatureDate", c => c.DateTime());
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "Witness1Signature", c => c.String());
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "Witness1Name", c => c.String(maxLength: 200));
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "Witness1SignatureDate", c => c.DateTime());
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "AccessCardDeposit", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "KeyDeposit", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "DSTVActivationFee", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "DSTVMonthlyLevy", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "HasDSTV", c => c.Boolean());
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "CommencementDay", c => c.String(maxLength: 50));
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantBankName", c => c.String(maxLength: 100));
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantAccountNumber", c => c.String(maxLength: 20));
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantAccountHolderName", c => c.String(maxLength: 200));
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantAccountType", c => c.String(maxLength: 50));
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantBranchCode", c => c.String(maxLength: 10));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "PropertyManagerSignatureDate", c => c.DateTime());
            AddColumn("dbo.PropertyLeaseAgreementMasters", "RevenueManagerSignatureDate", c => c.DateTime());
            AddColumn("dbo.PropertyLeaseAgreementMasters", "Witness1Signature", c => c.String());
            AddColumn("dbo.PropertyLeaseAgreementMasters", "Witness1Name", c => c.String(maxLength: 200));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "Witness1SignatureDate", c => c.DateTime());
            AddColumn("dbo.PropertyLeaseAgreementMasters", "AccessCardDeposit", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "KeyDeposit", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "DSTVActivationFee", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "DSTVMonthlyLevy", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "HasDSTV", c => c.Boolean());
            AddColumn("dbo.PropertyLeaseAgreementMasters", "CommencementDay", c => c.String(maxLength: 2));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "TenantBankName", c => c.String(maxLength: 100));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "TenantAccountNumber", c => c.String(maxLength: 20));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "TenantAccountHolderName", c => c.String(maxLength: 200));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "TenantAccountType", c => c.String(maxLength: 50));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "TenantBranchCode", c => c.String(maxLength: 10));
            AddColumn("dbo.PropertyLeaseApplicationAudits", "HasDSTV", c => c.Boolean());
            AddColumn("dbo.PropertyLeaseApplicationAudits", "DSTVActivationFee", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseApplicationAudits", "DSTVMonthlyLevy", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseApplicationAudits", "AccessCardDeposit", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseApplicationAudits", "KeyDeposit", c => c.Decimal(precision: 18, scale: 2));
            CreateIndex("dbo.PreferredComplexAreas", "MaintenanceManagerId");
            CreateIndex("dbo.AllocatedUnitMaintenanceEHCs", "BeforeImageId");
            CreateIndex("dbo.AllocatedUnitMaintenanceEHCs", "AfterImageId");
            CreateIndex("dbo.PreferredComplexAreaAudits", "MaintenanceManagerId");
            AddForeignKey("dbo.PreferredComplexAreas", "MaintenanceManagerId", "dbo.Customers", "Id");
            AddForeignKey("dbo.AllocatedUnitMaintenanceEHCs", "AfterImageId", "dbo.Documents", "Id");
            AddForeignKey("dbo.AllocatedUnitMaintenanceEHCs", "BeforeImageId", "dbo.Documents", "Id");
            AddForeignKey("dbo.PreferredComplexAreaAudits", "MaintenanceManagerId", "dbo.Customers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PreferredComplexAreaAudits", "MaintenanceManagerId", "dbo.Customers");
            DropForeignKey("dbo.ComplaintCategories", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.ComplaintCategories", "DepartmentId", "dbo.ApplicationEntities");
            DropForeignKey("dbo.ComplaintCategories", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.TenantComplaints", "SubmittedByCustomerId", "dbo.Customers");
            DropForeignKey("dbo.TenantComplaints", "StatusId", "dbo.Status");
            DropForeignKey("dbo.TenantComplaints", "RespondentComplexId", "dbo.PreferredComplexAreas");
            DropForeignKey("dbo.TenantComplaints", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.ComplaintInvestigations", "TenantComplaintId", "dbo.TenantComplaints");
            DropForeignKey("dbo.ComplaintInvestigations", "ScheduledById", "dbo.Customers");
            DropForeignKey("dbo.ComplaintInvestigations", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.ComplaintInvestigations", "InvestigatedById", "dbo.Customers");
            DropForeignKey("dbo.ComplaintExternalReferrals", "ReferredById", "dbo.Customers");
            DropForeignKey("dbo.ComplaintExternalReferrals", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.ComplaintInvestigations", "ExternalReferralId", "dbo.ComplaintExternalReferrals");
            DropForeignKey("dbo.ComplaintExternalReferrals", "DepartmentId", "dbo.ApplicationEntities");
            DropForeignKey("dbo.ComplaintExternalReferrals", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.ComplaintInvestigationDocuments", "UploadedById", "dbo.Customers");
            DropForeignKey("dbo.ComplaintInvestigationDocuments", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.ComplaintInvestigationDocuments", "DepartmentId", "dbo.ApplicationEntities");
            DropForeignKey("dbo.ComplaintInvestigationDocuments", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.ComplaintInvestigationDocuments", "ComplaintInvestigationId", "dbo.ComplaintInvestigations");
            DropForeignKey("dbo.ComplaintInvestigations", "DepartmentId", "dbo.ApplicationEntities");
            DropForeignKey("dbo.ComplaintInvestigations", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.ComplaintEvidences", "UploadedById", "dbo.Customers");
            DropForeignKey("dbo.ComplaintEvidences", "TenantComplaintId", "dbo.TenantComplaints");
            DropForeignKey("dbo.ComplaintEvidences", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.ComplaintEvidences", "DepartmentId", "dbo.ApplicationEntities");
            DropForeignKey("dbo.ComplaintEvidences", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.TenantComplaints", "DepartmentId", "dbo.ApplicationEntities");
            DropForeignKey("dbo.TenantComplaints", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.ComplaintTypes", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.ComplaintTypes", "DepartmentId", "dbo.ApplicationEntities");
            DropForeignKey("dbo.ComplaintTypes", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.TenantComplaints", "ComplaintTypeId", "dbo.ComplaintTypes");
            DropForeignKey("dbo.ComplaintTypes", "ComplaintCategoryId", "dbo.ComplaintCategories");
            DropForeignKey("dbo.TenantComplaints", "ComplaintCategoryId", "dbo.ComplaintCategories");
            DropForeignKey("dbo.TenantComplaints", "ComplainantComplexId", "dbo.PreferredComplexAreas");
            DropForeignKey("dbo.TenantComplaints", "AssignedToId", "dbo.Customers");
            DropForeignKey("dbo.MaintenanceJobCardTasks", "SupportingDocumentId", "dbo.Documents");
            DropForeignKey("dbo.MaintenanceJobCardTasks", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.MaintenanceJobCardTasks", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.MaintenanceJobCardTasks", "AllocatedUnitMaintenanceEHCId", "dbo.AllocatedUnitMaintenanceEHCs");
            DropForeignKey("dbo.MaintenanceJobCardSignatures", "SignedByCustomerId", "dbo.Customers");
            DropForeignKey("dbo.MaintenanceJobCardSignatures", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.MaintenanceJobCardSignatures", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.MaintenanceJobCardSignatures", "AllocatedUnitMaintenanceEHCId", "dbo.AllocatedUnitMaintenanceEHCs");
            DropForeignKey("dbo.AllocatedUnitMaintenanceEHCs", "BeforeImageId", "dbo.Documents");
            DropForeignKey("dbo.AllocatedUnitMaintenanceEHCs", "AfterImageId", "dbo.Documents");
            DropForeignKey("dbo.PreferredComplexAreas", "MaintenanceManagerId", "dbo.Customers");
            DropIndex("dbo.PreferredComplexAreaAudits", new[] { "MaintenanceManagerId" });
            DropIndex("dbo.ComplaintExternalReferrals", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.ComplaintExternalReferrals", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.ComplaintExternalReferrals", new[] { "DepartmentId" });
            DropIndex("dbo.ComplaintExternalReferrals", new[] { "ReferredById" });
            DropIndex("dbo.ComplaintInvestigationDocuments", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.ComplaintInvestigationDocuments", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.ComplaintInvestigationDocuments", new[] { "DepartmentId" });
            DropIndex("dbo.ComplaintInvestigationDocuments", new[] { "UploadedById" });
            DropIndex("dbo.ComplaintInvestigationDocuments", new[] { "ComplaintInvestigationId" });
            DropIndex("dbo.ComplaintInvestigations", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.ComplaintInvestigations", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.ComplaintInvestigations", new[] { "DepartmentId" });
            DropIndex("dbo.ComplaintInvestigations", new[] { "ExternalReferralId" });
            DropIndex("dbo.ComplaintInvestigations", new[] { "InvestigatedById" });
            DropIndex("dbo.ComplaintInvestigations", new[] { "ScheduledById" });
            DropIndex("dbo.ComplaintInvestigations", new[] { "TenantComplaintId" });
            DropIndex("dbo.ComplaintEvidences", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.ComplaintEvidences", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.ComplaintEvidences", new[] { "DepartmentId" });
            DropIndex("dbo.ComplaintEvidences", new[] { "UploadedById" });
            DropIndex("dbo.ComplaintEvidences", new[] { "TenantComplaintId" });
            DropIndex("dbo.ComplaintTypes", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.ComplaintTypes", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.ComplaintTypes", new[] { "DepartmentId" });
            DropIndex("dbo.ComplaintTypes", new[] { "ComplaintCategoryId" });
            DropIndex("dbo.TenantComplaints", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.TenantComplaints", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.TenantComplaints", new[] { "DepartmentId" });
            DropIndex("dbo.TenantComplaints", new[] { "AssignedToId" });
            DropIndex("dbo.TenantComplaints", new[] { "SubmittedByCustomerId" });
            DropIndex("dbo.TenantComplaints", new[] { "StatusId" });
            DropIndex("dbo.TenantComplaints", new[] { "ComplaintTypeId" });
            DropIndex("dbo.TenantComplaints", new[] { "ComplaintCategoryId" });
            DropIndex("dbo.TenantComplaints", new[] { "RespondentComplexId" });
            DropIndex("dbo.TenantComplaints", new[] { "ComplainantComplexId" });
            DropIndex("dbo.ComplaintCategories", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.ComplaintCategories", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.ComplaintCategories", new[] { "DepartmentId" });
            DropIndex("dbo.MaintenanceJobCardTasks", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.MaintenanceJobCardTasks", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.MaintenanceJobCardTasks", new[] { "SupportingDocumentId" });
            DropIndex("dbo.MaintenanceJobCardTasks", new[] { "AllocatedUnitMaintenanceEHCId" });
            DropIndex("dbo.MaintenanceJobCardSignatures", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.MaintenanceJobCardSignatures", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.MaintenanceJobCardSignatures", new[] { "SignedByCustomerId" });
            DropIndex("dbo.MaintenanceJobCardSignatures", new[] { "AllocatedUnitMaintenanceEHCId" });
            DropIndex("dbo.AllocatedUnitMaintenanceEHCs", new[] { "AfterImageId" });
            DropIndex("dbo.AllocatedUnitMaintenanceEHCs", new[] { "BeforeImageId" });
            DropIndex("dbo.PreferredComplexAreas", new[] { "MaintenanceManagerId" });
            DropColumn("dbo.PropertyLeaseApplicationAudits", "KeyDeposit");
            DropColumn("dbo.PropertyLeaseApplicationAudits", "AccessCardDeposit");
            DropColumn("dbo.PropertyLeaseApplicationAudits", "DSTVMonthlyLevy");
            DropColumn("dbo.PropertyLeaseApplicationAudits", "DSTVActivationFee");
            DropColumn("dbo.PropertyLeaseApplicationAudits", "HasDSTV");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "TenantBranchCode");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "TenantAccountType");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "TenantAccountHolderName");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "TenantAccountNumber");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "TenantBankName");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "CommencementDay");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "HasDSTV");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "DSTVMonthlyLevy");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "DSTVActivationFee");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "KeyDeposit");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "AccessCardDeposit");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "Witness1SignatureDate");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "Witness1Name");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "Witness1Signature");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "RevenueManagerSignatureDate");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "PropertyManagerSignatureDate");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantBranchCode");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantAccountType");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantAccountHolderName");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantAccountNumber");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantBankName");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "CommencementDay");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "HasDSTV");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "DSTVMonthlyLevy");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "DSTVActivationFee");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "KeyDeposit");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "AccessCardDeposit");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "Witness1SignatureDate");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "Witness1Name");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "Witness1Signature");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "RevenueManagerSignatureDate");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "PropertyManagerSignatureDate");
            DropColumn("dbo.PreferredComplexAreaAudits", "MaintenanceManagerId");
            DropColumn("dbo.AllocatedUnitMaintenanceEHCs", "Inspection");
            DropColumn("dbo.AllocatedUnitMaintenanceEHCs", "JobCardSubmittedDate");
            DropColumn("dbo.AllocatedUnitMaintenanceEHCs", "JobCardSubmitted");
            DropColumn("dbo.AllocatedUnitMaintenanceEHCs", "AfterImageId");
            DropColumn("dbo.AllocatedUnitMaintenanceEHCs", "BeforeImageId");
            DropColumn("dbo.PropertyLeaseApplications", "KeyDeposit");
            DropColumn("dbo.PropertyLeaseApplications", "AccessCardDeposit");
            DropColumn("dbo.PropertyLeaseApplications", "DSTVMonthlyLevy");
            DropColumn("dbo.PropertyLeaseApplications", "DSTVActivationFee");
            DropColumn("dbo.PropertyLeaseApplications", "HasDSTV");
            DropColumn("dbo.PreferredComplexAreas", "MaintenanceManagerId");
            DropTable("dbo.ComplaintExternalReferrals");
            DropTable("dbo.ComplaintInvestigationDocuments");
            DropTable("dbo.ComplaintInvestigations");
            DropTable("dbo.ComplaintEvidences");
            DropTable("dbo.ComplaintTypes");
            DropTable("dbo.TenantComplaints");
            DropTable("dbo.ComplaintCategories");
            DropTable("dbo.MaintenanceJobCardTasks");
            DropTable("dbo.MaintenanceJobCardSignatures");
        }
    }
}
