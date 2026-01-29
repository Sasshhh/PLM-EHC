namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedNewtableMonthlyincome : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ApplicantTypeAudits",
                c => new
                    {
                        AuditId = c.Int(nullable: false, identity: true),
                        Action = c.String(maxLength: 10),
                        Id = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 250),
                        Key = c.String(maxLength: 250),
                        DepartmentId = c.Int(),
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
                .ForeignKey("dbo.ApplicationEntities", t => t.DepartmentId)
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .Index(t => t.DepartmentId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId);
            
            CreateTable(
                "dbo.ApplicantTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 500),
                        Key = c.String(maxLength: 100),
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
                "dbo.MonthlyIncomeAudits",
                c => new
                    {
                        AuditId = c.Int(nullable: false, identity: true),
                        Action = c.String(maxLength: 10),
                        Id = c.Int(nullable: false),
                        GrossIncome = c.Decimal(precision: 18, scale: 2),
                        Allowances = c.Decimal(precision: 18, scale: 2),
                        FringeBenefits = c.Decimal(precision: 18, scale: 2),
                        OtherRegularIncome = c.Decimal(precision: 18, scale: 2),
                        TotalGrossIncome = c.Decimal(precision: 18, scale: 2),
                        PayeTaxLessDeductions = c.Decimal(precision: 18, scale: 2),
                        PensionProvidentLessDeductions = c.Decimal(precision: 18, scale: 2),
                        UIFLessDeductions = c.Decimal(precision: 18, scale: 2),
                        MedicalAidLessDeductions = c.Decimal(precision: 18, scale: 2),
                        OtherLessDeductions = c.Decimal(precision: 18, scale: 2),
                        TotalDeductions = c.Decimal(precision: 18, scale: 2),
                        NetIncome = c.Decimal(precision: 18, scale: 2),
                        OtherDividendsIncome = c.Decimal(precision: 18, scale: 2),
                        TotalNetIncome = c.Decimal(precision: 18, scale: 2),
                        PropertyLeaseApplicationId = c.Int(nullable: false),
                        ApplicantTypeId = c.Int(nullable: false),
                        DepartmentId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsLocked = c.Boolean(),
                        CreatedBySystemUserId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedBySystemUserId = c.Int(),
                        ModifiedDateTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.AuditId)
                .ForeignKey("dbo.ApplicantTypes", t => t.ApplicantTypeId)
                .ForeignKey("dbo.SystemUsers", t => t.CreatedBySystemUserId)
                .ForeignKey("dbo.ApplicationEntities", t => t.DepartmentId)
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .ForeignKey("dbo.PropertyLeaseApplications", t => t.PropertyLeaseApplicationId)
                .Index(t => t.PropertyLeaseApplicationId)
                .Index(t => t.ApplicantTypeId)
                .Index(t => t.DepartmentId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId);
            
            CreateTable(
                "dbo.MonthlyIncomes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GrossIncome = c.Decimal(precision: 18, scale: 2),
                        Allowances = c.Decimal(precision: 18, scale: 2),
                        FringeBenefits = c.Decimal(precision: 18, scale: 2),
                        OtherRegularIncome = c.Decimal(precision: 18, scale: 2),
                        TotalGrossIncome = c.Decimal(precision: 18, scale: 2),
                        PayeTaxLessDeductions = c.Decimal(precision: 18, scale: 2),
                        PensionProvidentLessDeductions = c.Decimal(precision: 18, scale: 2),
                        UIFLessDeductions = c.Decimal(precision: 18, scale: 2),
                        MedicalAidLessDeductions = c.Decimal(precision: 18, scale: 2),
                        OtherLessDeductions = c.Decimal(precision: 18, scale: 2),
                        TotalDeductions = c.Decimal(precision: 18, scale: 2),
                        NetIncome = c.Decimal(precision: 18, scale: 2),
                        OtherDividendsIncome = c.Decimal(precision: 18, scale: 2),
                        TotalNetIncome = c.Decimal(precision: 18, scale: 2),
                        PropertyLeaseApplicationId = c.Int(nullable: false),
                        ApplicantTypeId = c.Int(nullable: false),
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
                .ForeignKey("dbo.ApplicantTypes", t => t.ApplicantTypeId)
                .ForeignKey("dbo.SystemUsers", t => t.CreatedBySystemUserId)
                .ForeignKey("dbo.ApplicationEntities", t => t.DepartmentId)
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .ForeignKey("dbo.PropertyLeaseApplications", t => t.PropertyLeaseApplicationId)
                .Index(t => t.PropertyLeaseApplicationId)
                .Index(t => t.ApplicantTypeId)
                .Index(t => t.DepartmentId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MonthlyIncomes", "PropertyLeaseApplicationId", "dbo.PropertyLeaseApplications");
            DropForeignKey("dbo.MonthlyIncomes", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.MonthlyIncomes", "DepartmentId", "dbo.ApplicationEntities");
            DropForeignKey("dbo.MonthlyIncomes", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.MonthlyIncomes", "ApplicantTypeId", "dbo.ApplicantTypes");
            DropForeignKey("dbo.MonthlyIncomeAudits", "PropertyLeaseApplicationId", "dbo.PropertyLeaseApplications");
            DropForeignKey("dbo.MonthlyIncomeAudits", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.MonthlyIncomeAudits", "DepartmentId", "dbo.ApplicationEntities");
            DropForeignKey("dbo.MonthlyIncomeAudits", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.MonthlyIncomeAudits", "ApplicantTypeId", "dbo.ApplicantTypes");
            DropForeignKey("dbo.ApplicantTypes", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.ApplicantTypes", "DepartmentId", "dbo.ApplicationEntities");
            DropForeignKey("dbo.ApplicantTypes", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.ApplicantTypeAudits", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.ApplicantTypeAudits", "DepartmentId", "dbo.ApplicationEntities");
            DropForeignKey("dbo.ApplicantTypeAudits", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropIndex("dbo.MonthlyIncomes", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.MonthlyIncomes", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.MonthlyIncomes", new[] { "DepartmentId" });
            DropIndex("dbo.MonthlyIncomes", new[] { "ApplicantTypeId" });
            DropIndex("dbo.MonthlyIncomes", new[] { "PropertyLeaseApplicationId" });
            DropIndex("dbo.MonthlyIncomeAudits", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.MonthlyIncomeAudits", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.MonthlyIncomeAudits", new[] { "DepartmentId" });
            DropIndex("dbo.MonthlyIncomeAudits", new[] { "ApplicantTypeId" });
            DropIndex("dbo.MonthlyIncomeAudits", new[] { "PropertyLeaseApplicationId" });
            DropIndex("dbo.ApplicantTypes", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.ApplicantTypes", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.ApplicantTypes", new[] { "DepartmentId" });
            DropIndex("dbo.ApplicantTypeAudits", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.ApplicantTypeAudits", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.ApplicantTypeAudits", new[] { "DepartmentId" });
            DropTable("dbo.MonthlyIncomes");
            DropTable("dbo.MonthlyIncomeAudits");
            DropTable("dbo.ApplicantTypes");
            DropTable("dbo.ApplicantTypeAudits");
        }
    }
}
