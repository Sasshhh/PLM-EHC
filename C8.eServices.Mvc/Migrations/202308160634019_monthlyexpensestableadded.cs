namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class monthlyexpensestableadded : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MonthlyExpenseAudits",
                c => new
                    {
                        AuditId = c.Int(nullable: false, identity: true),
                        Action = c.String(maxLength: 10),
                        Id = c.Int(nullable: false),
                        PropertyLeaseApplicationId = c.Int(nullable: false),
                        ApplicantTypeId = c.Int(nullable: false),
                        HEAccommodation = c.Decimal(precision: 18, scale: 2),
                        HEInsurances = c.Decimal(precision: 18, scale: 2),
                        HERatesTaxes = c.Decimal(precision: 18, scale: 2),
                        HESecurity = c.Decimal(precision: 18, scale: 2),
                        HEUpkeep = c.Decimal(precision: 18, scale: 2),
                        HEUtilitiesElectricity = c.Decimal(precision: 18, scale: 2),
                        HEUtilitiesWater = c.Decimal(precision: 18, scale: 2),
                        HEOthers = c.Decimal(precision: 18, scale: 2),
                        VEFuel = c.Decimal(precision: 18, scale: 2),
                        VEInsurance = c.Decimal(precision: 18, scale: 2),
                        VEMaintenance = c.Decimal(precision: 18, scale: 2),
                        VEVehicleFinance = c.Decimal(precision: 18, scale: 2),
                        ELifeAssurances = c.Decimal(precision: 18, scale: 2),
                        EShortTermInsurances = c.Decimal(precision: 18, scale: 2),
                        EOtherInsurancesFuneral = c.Decimal(precision: 18, scale: 2),
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
                "dbo.MonthlyExpenses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PropertyLeaseApplicationId = c.Int(nullable: false),
                        ApplicantTypeId = c.Int(nullable: false),
                        HEAccommodation = c.Decimal(precision: 18, scale: 2),
                        HEInsurances = c.Decimal(precision: 18, scale: 2),
                        HERatesTaxes = c.Decimal(precision: 18, scale: 2),
                        HESecurity = c.Decimal(precision: 18, scale: 2),
                        HEUpkeep = c.Decimal(precision: 18, scale: 2),
                        HEUtilitiesElectricity = c.Decimal(precision: 18, scale: 2),
                        HEUtilitiesWater = c.Decimal(precision: 18, scale: 2),
                        HEOthers = c.Decimal(precision: 18, scale: 2),
                        VEFuel = c.Decimal(precision: 18, scale: 2),
                        VEInsurance = c.Decimal(precision: 18, scale: 2),
                        VEMaintenance = c.Decimal(precision: 18, scale: 2),
                        VEVehicleFinance = c.Decimal(precision: 18, scale: 2),
                        ELifeAssurances = c.Decimal(precision: 18, scale: 2),
                        EShortTermInsurances = c.Decimal(precision: 18, scale: 2),
                        EOtherInsurancesFuneral = c.Decimal(precision: 18, scale: 2),
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
            
            AlterColumn("dbo.PropertyLeaseApplications", "PrefArea", c => c.String());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MonthlyExpenses", "PropertyLeaseApplicationId", "dbo.PropertyLeaseApplications");
            DropForeignKey("dbo.MonthlyExpenses", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.MonthlyExpenses", "DepartmentId", "dbo.ApplicationEntities");
            DropForeignKey("dbo.MonthlyExpenses", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.MonthlyExpenses", "ApplicantTypeId", "dbo.ApplicantTypes");
            DropForeignKey("dbo.MonthlyExpenseAudits", "PropertyLeaseApplicationId", "dbo.PropertyLeaseApplications");
            DropForeignKey("dbo.MonthlyExpenseAudits", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.MonthlyExpenseAudits", "DepartmentId", "dbo.ApplicationEntities");
            DropForeignKey("dbo.MonthlyExpenseAudits", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.MonthlyExpenseAudits", "ApplicantTypeId", "dbo.ApplicantTypes");
            DropIndex("dbo.MonthlyExpenses", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.MonthlyExpenses", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.MonthlyExpenses", new[] { "DepartmentId" });
            DropIndex("dbo.MonthlyExpenses", new[] { "ApplicantTypeId" });
            DropIndex("dbo.MonthlyExpenses", new[] { "PropertyLeaseApplicationId" });
            DropIndex("dbo.MonthlyExpenseAudits", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.MonthlyExpenseAudits", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.MonthlyExpenseAudits", new[] { "DepartmentId" });
            DropIndex("dbo.MonthlyExpenseAudits", new[] { "ApplicantTypeId" });
            DropIndex("dbo.MonthlyExpenseAudits", new[] { "PropertyLeaseApplicationId" });
            AlterColumn("dbo.PropertyLeaseApplications", "PrefArea", c => c.String(maxLength: 20));
            DropTable("dbo.MonthlyExpenses");
            DropTable("dbo.MonthlyExpenseAudits");
        }
    }
}
