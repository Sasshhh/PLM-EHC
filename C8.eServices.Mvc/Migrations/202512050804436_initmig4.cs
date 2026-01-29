namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initmig4 : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.PropertyLeaseWaitingLists", "PreferredComplexId");
            CreateIndex("dbo.PropertyLeaseWaitingLists", "PreferredTypologyId");
            AddForeignKey("dbo.PropertyLeaseWaitingLists", "PreferredTypologyId", "dbo.HumanEHCOptions", "Id");
            AddForeignKey("dbo.PropertyLeaseWaitingLists", "PreferredComplexId", "dbo.PreferredComplexAreas", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PropertyLeaseWaitingLists", "PreferredComplexId", "dbo.PreferredComplexAreas");
            DropForeignKey("dbo.PropertyLeaseWaitingLists", "PreferredTypologyId", "dbo.HumanEHCOptions");
            DropIndex("dbo.PropertyLeaseWaitingLists", new[] { "PreferredTypologyId" });
            DropIndex("dbo.PropertyLeaseWaitingLists", new[] { "PreferredComplexId" });
        }
    }
}
