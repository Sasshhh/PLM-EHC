namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _57thut : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "RoundRobinIsActive", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "RoundRobinIsActive");
        }
    }
}
