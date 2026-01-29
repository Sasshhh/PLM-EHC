namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addnewcolumnsfortable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ApplicationAllocatedProperties", "Water", c => c.Double(nullable: false));
            AddColumn("dbo.ApplicationAllocatedProperties", "Refuse", c => c.Double(nullable: false));
            AddColumn("dbo.ApplicationAllocatedProperties", "Sewer", c => c.Double(nullable: false));
            AddColumn("dbo.ApplicationAllocatedProperties", "HumanEHCOptionId", c => c.Int());
            AddColumn("dbo.ApplicationAllocatedProperties", "TotalCharges", c => c.Double(nullable: false));
            AddColumn("dbo.ApplicationAllocatedProperties", "Inspection", c => c.Boolean(nullable: false));
            AddColumn("dbo.ApplicationAllocatedProperties", "BuildingName", c => c.String());
            AddColumn("dbo.ApplicationAllocatedProperties", "DepositRequired", c => c.Double(nullable: false));
            CreateIndex("dbo.ApplicationAllocatedProperties", "HumanEHCOptionId");
            AddForeignKey("dbo.ApplicationAllocatedProperties", "HumanEHCOptionId", "dbo.HumanEHCOptions", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ApplicationAllocatedProperties", "HumanEHCOptionId", "dbo.HumanEHCOptions");
            DropIndex("dbo.ApplicationAllocatedProperties", new[] { "HumanEHCOptionId" });
            DropColumn("dbo.ApplicationAllocatedProperties", "DepositRequired");
            DropColumn("dbo.ApplicationAllocatedProperties", "BuildingName");
            DropColumn("dbo.ApplicationAllocatedProperties", "Inspection");
            DropColumn("dbo.ApplicationAllocatedProperties", "TotalCharges");
            DropColumn("dbo.ApplicationAllocatedProperties", "HumanEHCOptionId");
            DropColumn("dbo.ApplicationAllocatedProperties", "Sewer");
            DropColumn("dbo.ApplicationAllocatedProperties", "Refuse");
            DropColumn("dbo.ApplicationAllocatedProperties", "Water");
        }
    }
}
