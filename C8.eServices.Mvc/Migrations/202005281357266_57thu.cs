namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _57thu : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RCSActionTypes",
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
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RCSActionTypes", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.RCSActionTypes", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropIndex("dbo.RCSActionTypes", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.RCSActionTypes", new[] { "CreatedBySystemUserId" });
            DropTable("dbo.RCSActionTypes");
        }
    }
}
