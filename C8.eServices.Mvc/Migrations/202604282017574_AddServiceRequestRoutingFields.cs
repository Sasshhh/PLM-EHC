namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddServiceRequestRoutingFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RoundRobinQueues", "ServiceRequestId", c => c.Int());
            AddColumn("dbo.ServiceRequests", "AssignedToId", c => c.Int());
            AddColumn("dbo.ServiceRequests", "DateAssigned", c => c.DateTime());
            CreateIndex("dbo.RoundRobinQueues", "ServiceRequestId");
            CreateIndex("dbo.ServiceRequests", "AssignedToId");
            AddForeignKey("dbo.ServiceRequests", "AssignedToId", "dbo.Customers", "Id");
            AddForeignKey("dbo.RoundRobinQueues", "ServiceRequestId", "dbo.ServiceRequests", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RoundRobinQueues", "ServiceRequestId", "dbo.ServiceRequests");
            DropForeignKey("dbo.ServiceRequests", "AssignedToId", "dbo.Customers");
            DropIndex("dbo.ServiceRequests", new[] { "AssignedToId" });
            DropIndex("dbo.RoundRobinQueues", new[] { "ServiceRequestId" });
            DropColumn("dbo.ServiceRequests", "DateAssigned");
            DropColumn("dbo.ServiceRequests", "AssignedToId");
            DropColumn("dbo.RoundRobinQueues", "ServiceRequestId");
        }
    }
}
