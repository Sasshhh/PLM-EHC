namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddComplaintsModule : DbMigration
    {
        public override void Up()
        {
            // Create ComplaintCategories table
            CreateTable(
                "dbo.ComplaintCategories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DepartmentId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        CreatedBy = c.Int(),
                        ModifiedBy = c.Int(),
                        DeletedBy = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        DeletedDateTime = c.DateTime(),
                        Name = c.String(nullable: false, maxLength: 200),
                        Key = c.String(maxLength: 100),
                        Description = c.String(maxLength: 500),
                        DisplayOrder = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Key, unique: true, name: "IX_ComplaintCategory_Key");
            
            // Create ComplaintTypes table
            CreateTable(
                "dbo.ComplaintTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DepartmentId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        CreatedBy = c.Int(),
                        ModifiedBy = c.Int(),
                        DeletedBy = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        DeletedDateTime = c.DateTime(),
                        ComplaintCategoryId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 200),
                        Key = c.String(maxLength: 100),
                        Description = c.String(maxLength: 500),
                        DisplayOrder = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ComplaintCategories", t => t.ComplaintCategoryId, cascadeDelete: false)
                .Index(t => t.ComplaintCategoryId)
                .Index(t => t.Key, unique: true, name: "IX_ComplaintType_Key");
            
            // Create TenantComplaints table
            CreateTable(
                "dbo.TenantComplaints",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DepartmentId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        CreatedBy = c.Int(),
                        ModifiedBy = c.Int(),
                        DeletedBy = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        DeletedDateTime = c.DateTime(),
                        CaseReferenceNumber = c.String(nullable: false, maxLength: 50),
                        OfficialNumber = c.String(maxLength: 50),
                        ComplainantComplexId = c.Int(),
                        ComplainantFirstName = c.String(maxLength: 100),
                        ComplainantSurname = c.String(maxLength: 100),
                        ComplainantEmail = c.String(maxLength: 100),
                        ComplainantCellphone = c.String(maxLength: 20),
                        ComplainantUnitNumber = c.String(maxLength: 20),
                        ComplainantBlockNumber = c.String(maxLength: 20),
                        RespondentComplexId = c.Int(),
                        RespondentBlockNumber = c.String(maxLength: 20),
                        RespondentUnitNumber = c.String(maxLength: 20),
                        RespondentFirstName = c.String(maxLength: 100),
                        RespondentSurname = c.String(maxLength: 100),
                        ComplaintCategoryId = c.Int(nullable: false),
                        ComplaintTypeId = c.Int(nullable: false),
                        DetailedDescription = c.String(nullable: false),
                        StatusId = c.Int(nullable: false),
                        SubmittedByCustomerId = c.Int(),
                        SubmittedByUserId = c.String(maxLength: 128),
                        DateSubmitted = c.DateTime(),
                        AssignedToId = c.Int(),
                        DateAssigned = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PreferredComplexAreas", t => t.ComplainantComplexId)
                .ForeignKey("dbo.PreferredComplexAreas", t => t.RespondentComplexId)
                .ForeignKey("dbo.ComplaintCategories", t => t.ComplaintCategoryId, cascadeDelete: false)
                .ForeignKey("dbo.ComplaintTypes", t => t.ComplaintTypeId, cascadeDelete: false)
                .ForeignKey("dbo.Status", t => t.StatusId, cascadeDelete: false)
                .ForeignKey("dbo.Customers", t => t.SubmittedByCustomerId)
                .ForeignKey("dbo.Customers", t => t.AssignedToId)
                .Index(t => t.CaseReferenceNumber, unique: true)
                .Index(t => t.ComplainantComplexId)
                .Index(t => t.RespondentComplexId)
                .Index(t => t.ComplaintCategoryId)
                .Index(t => t.ComplaintTypeId)
                .Index(t => t.StatusId)
                .Index(t => t.SubmittedByCustomerId)
                .Index(t => t.AssignedToId);
            
            // Create ComplaintEvidences table
            CreateTable(
                "dbo.ComplaintEvidences",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DepartmentId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        CreatedBy = c.Int(),
                        ModifiedBy = c.Int(),
                        DeletedBy = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        DeletedDateTime = c.DateTime(),
                        TenantComplaintId = c.Int(nullable: false),
                        FileName = c.String(nullable: false, maxLength: 255),
                        FileType = c.String(maxLength: 50),
                        FilePath = c.String(nullable: false, maxLength: 500),
                        FileSize = c.Long(nullable: false),
                        UploadedById = c.Int(),
                        UploadDate = c.DateTime(nullable: false),
                        Description = c.String(maxLength: 500),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TenantComplaints", t => t.TenantComplaintId, cascadeDelete: true)
                .ForeignKey("dbo.Customers", t => t.UploadedById)
                .Index(t => t.TenantComplaintId)
                .Index(t => t.UploadedById);
            
            // Create ComplaintExternalReferrals table
            CreateTable(
                "dbo.ComplaintExternalReferrals",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DepartmentId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        CreatedBy = c.Int(),
                        ModifiedBy = c.Int(),
                        DeletedBy = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        DeletedDateTime = c.DateTime(),
                        AgencyName = c.String(nullable: false, maxLength: 200),
                        Address = c.String(maxLength: 500),
                        ContactPerson = c.String(maxLength: 100),
                        ContactNumber = c.String(maxLength: 20),
                        ContactEmail = c.String(maxLength: 100),
                        BriefDescription = c.String(),
                        ReferredById = c.Int(),
                        DateReferred = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Customers", t => t.ReferredById)
                .Index(t => t.ReferredById);
            
            // Create ComplaintInvestigations table
            CreateTable(
                "dbo.ComplaintInvestigations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DepartmentId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        CreatedBy = c.Int(),
                        ModifiedBy = c.Int(),
                        DeletedBy = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        DeletedDateTime = c.DateTime(),
                        TenantComplaintId = c.Int(nullable: false),
                        AppointmentDate = c.DateTime(),
                        AppointmentTime = c.String(maxLength: 20),
                        ScheduledById = c.Int(),
                        DateScheduled = c.DateTime(),
                        RespondentConfirmed = c.Boolean(nullable: false),
                        DateConfirmed = c.DateTime(),
                        Outcome = c.String(maxLength: 50),
                        OutcomeDetails = c.String(),
                        OutcomeDate = c.DateTime(),
                        InvestigatedById = c.Int(),
                        ExternalReferralId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TenantComplaints", t => t.TenantComplaintId, cascadeDelete: true)
                .ForeignKey("dbo.Customers", t => t.ScheduledById)
                .ForeignKey("dbo.Customers", t => t.InvestigatedById)
                .ForeignKey("dbo.ComplaintExternalReferrals", t => t.ExternalReferralId)
                .Index(t => t.TenantComplaintId)
                .Index(t => t.ScheduledById)
                .Index(t => t.InvestigatedById)
                .Index(t => t.ExternalReferralId);
            
            // Create ComplaintInvestigationDocuments table
            CreateTable(
                "dbo.ComplaintInvestigationDocuments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DepartmentId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        CreatedBy = c.Int(),
                        ModifiedBy = c.Int(),
                        DeletedBy = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        DeletedDateTime = c.DateTime(),
                        ComplaintInvestigationId = c.Int(nullable: false),
                        FileName = c.String(nullable: false, maxLength: 255),
                        FileType = c.String(maxLength: 50),
                        FilePath = c.String(nullable: false, maxLength: 500),
                        FileSize = c.Long(nullable: false),
                        UploadedById = c.Int(),
                        UploadDate = c.DateTime(nullable: false),
                        Description = c.String(maxLength: 500),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ComplaintInvestigations", t => t.ComplaintInvestigationId, cascadeDelete: true)
                .ForeignKey("dbo.Customers", t => t.UploadedById)
                .Index(t => t.ComplaintInvestigationId)
                .Index(t => t.UploadedById);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ComplaintInvestigationDocuments", "UploadedById", "dbo.Customers");
            DropForeignKey("dbo.ComplaintInvestigationDocuments", "ComplaintInvestigationId", "dbo.ComplaintInvestigations");
            DropForeignKey("dbo.ComplaintInvestigations", "ExternalReferralId", "dbo.ComplaintExternalReferrals");
            DropForeignKey("dbo.ComplaintInvestigations", "InvestigatedById", "dbo.Customers");
            DropForeignKey("dbo.ComplaintInvestigations", "ScheduledById", "dbo.Customers");
            DropForeignKey("dbo.ComplaintInvestigations", "TenantComplaintId", "dbo.TenantComplaints");
            DropForeignKey("dbo.ComplaintExternalReferrals", "ReferredById", "dbo.Customers");
            DropForeignKey("dbo.ComplaintEvidences", "UploadedById", "dbo.Customers");
            DropForeignKey("dbo.ComplaintEvidences", "TenantComplaintId", "dbo.TenantComplaints");
            DropForeignKey("dbo.TenantComplaints", "AssignedToId", "dbo.Customers");
            DropForeignKey("dbo.TenantComplaints", "SubmittedByCustomerId", "dbo.Customers");
            DropForeignKey("dbo.TenantComplaints", "StatusId", "dbo.Status");
            DropForeignKey("dbo.TenantComplaints", "ComplaintTypeId", "dbo.ComplaintTypes");
            DropForeignKey("dbo.TenantComplaints", "ComplaintCategoryId", "dbo.ComplaintCategories");
            DropForeignKey("dbo.TenantComplaints", "RespondentComplexId", "dbo.PreferredComplexAreas");
            DropForeignKey("dbo.TenantComplaints", "ComplainantComplexId", "dbo.PreferredComplexAreas");
            DropForeignKey("dbo.ComplaintTypes", "ComplaintCategoryId", "dbo.ComplaintCategories");
            DropIndex("dbo.ComplaintInvestigationDocuments", new[] { "UploadedById" });
            DropIndex("dbo.ComplaintInvestigationDocuments", new[] { "ComplaintInvestigationId" });
            DropIndex("dbo.ComplaintInvestigations", new[] { "ExternalReferralId" });
            DropIndex("dbo.ComplaintInvestigations", new[] { "InvestigatedById" });
            DropIndex("dbo.ComplaintInvestigations", new[] { "ScheduledById" });
            DropIndex("dbo.ComplaintInvestigations", new[] { "TenantComplaintId" });
            DropIndex("dbo.ComplaintExternalReferrals", new[] { "ReferredById" });
            DropIndex("dbo.ComplaintEvidences", new[] { "UploadedById" });
            DropIndex("dbo.ComplaintEvidences", new[] { "TenantComplaintId" });
            DropIndex("dbo.TenantComplaints", new[] { "AssignedToId" });
            DropIndex("dbo.TenantComplaints", new[] { "SubmittedByCustomerId" });
            DropIndex("dbo.TenantComplaints", new[] { "StatusId" });
            DropIndex("dbo.TenantComplaints", new[] { "ComplaintTypeId" });
            DropIndex("dbo.TenantComplaints", new[] { "ComplaintCategoryId" });
            DropIndex("dbo.TenantComplaints", new[] { "RespondentComplexId" });
            DropIndex("dbo.TenantComplaints", new[] { "ComplainantComplexId" });
            DropIndex("dbo.TenantComplaints", new[] { "CaseReferenceNumber" });
            DropIndex("dbo.ComplaintTypes", "IX_ComplaintType_Key");
            DropIndex("dbo.ComplaintTypes", new[] { "ComplaintCategoryId" });
            DropIndex("dbo.ComplaintCategories", "IX_ComplaintCategory_Key");
            DropTable("dbo.ComplaintInvestigationDocuments");
            DropTable("dbo.ComplaintInvestigations");
            DropTable("dbo.ComplaintExternalReferrals");
            DropTable("dbo.ComplaintEvidences");
            DropTable("dbo.TenantComplaints");
            DropTable("dbo.ComplaintTypes");
            DropTable("dbo.ComplaintCategories");
        }
    }
}
