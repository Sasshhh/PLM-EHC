namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addterminationcolumnnew : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PropertyLeaseActionComments", "LeaseTerminationLetter", c => c.Boolean(nullable: false));
            AddColumn("dbo.PropertyLeaseActionCommentsAudits", "LeaseTerminationLetter", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PropertyLeaseActionCommentsAudits", "LeaseTerminationLetter");
            DropColumn("dbo.PropertyLeaseActionComments", "LeaseTerminationLetter");
        }
    }
}
