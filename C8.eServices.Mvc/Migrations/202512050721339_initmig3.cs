namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initmig3 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PropertyLeaseWaitingLists",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DepartmentId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsLocked = c.Boolean(),
                        CreatedBySystemUserId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedBySystemUserId = c.Int(),
                        ModifiedDateTime = c.DateTime(),
                        PropertyLeaseApplicationId = c.Int(nullable: false),
                        PreferredComplexId = c.Int(nullable: false),
                        PreferredTypologyId = c.Int(nullable: false),
                        DateAdded = c.DateTime(nullable: false),
                        QueueStatus = c.String(),
                        OfferedUnitId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SystemUsers", t => t.CreatedBySystemUserId)
                .ForeignKey("dbo.ApplicationEntities", t => t.DepartmentId)
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .ForeignKey("dbo.PropertyLeaseApplications", t => t.PropertyLeaseApplicationId)
                .Index(t => t.DepartmentId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId)
                .Index(t => t.PropertyLeaseApplicationId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PropertyLeaseWaitingLists", "PropertyLeaseApplicationId", "dbo.PropertyLeaseApplications");
            DropForeignKey("dbo.PropertyLeaseWaitingLists", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.PropertyLeaseWaitingLists", "DepartmentId", "dbo.ApplicationEntities");
            DropForeignKey("dbo.PropertyLeaseWaitingLists", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropIndex("dbo.PropertyLeaseWaitingLists", new[] { "PropertyLeaseApplicationId" });
            DropIndex("dbo.PropertyLeaseWaitingLists", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.PropertyLeaseWaitingLists", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.PropertyLeaseWaitingLists", new[] { "DepartmentId" });
            DropTable("dbo.PropertyLeaseWaitingLists");
        }
    }
}
