namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _5HTK : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TransferTypeAudits",
                c => new
                    {
                        AuditId = c.Int(nullable: false, identity: true),
                        Action = c.String(maxLength: 10),
                        Id = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 250),
                        Key = c.String(maxLength: 250),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsLocked = c.Boolean(),
                        CreatedBySystemUserId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedBySystemUserId = c.Int(),
                        ModifiedDateTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.AuditId)
                .ForeignKey("dbo.SystemUsers", t => t.CreatedBySystemUserId)
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId);
            
            CreateTable(
                "dbo.TransferTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 500),
                        Key = c.String(maxLength: 100),
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
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId);
            
            AddColumn("dbo.DepartmentsApprovalAudits", "DepartmentId", c => c.Int(nullable: false));
            AddColumn("dbo.DepartmentsApprovals", "DepartmentId", c => c.Int(nullable: false));
            CreateIndex("dbo.DepartmentsApprovalAudits", "DepartmentId");
            CreateIndex("dbo.DepartmentsApprovals", "DepartmentId");
            AddForeignKey("dbo.DepartmentsApprovalAudits", "DepartmentId", "dbo.RCSDepartmentTypes", "Id");
            AddForeignKey("dbo.DepartmentsApprovals", "DepartmentId", "dbo.RCSDepartmentTypes", "Id");
            DropColumn("dbo.DepartmentsApprovalAudits", "Department");
            DropColumn("dbo.DepartmentsApprovals", "Department");
        }
        
        public override void Down()
        {
            AddColumn("dbo.DepartmentsApprovals", "Department", c => c.Int(nullable: false));
            AddColumn("dbo.DepartmentsApprovalAudits", "Department", c => c.Int(nullable: false));
            DropForeignKey("dbo.TransferTypes", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.TransferTypes", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.TransferTypeAudits", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.TransferTypeAudits", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.DepartmentsApprovals", "DepartmentId", "dbo.RCSDepartmentTypes");
            DropForeignKey("dbo.DepartmentsApprovalAudits", "DepartmentId", "dbo.RCSDepartmentTypes");
            DropIndex("dbo.TransferTypes", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.TransferTypes", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.TransferTypeAudits", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.TransferTypeAudits", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.DepartmentsApprovals", new[] { "DepartmentId" });
            DropIndex("dbo.DepartmentsApprovalAudits", new[] { "DepartmentId" });
            DropColumn("dbo.DepartmentsApprovals", "DepartmentId");
            DropColumn("dbo.DepartmentsApprovalAudits", "DepartmentId");
            DropTable("dbo.TransferTypes");
            DropTable("dbo.TransferTypeAudits");
        }
    }
}
