namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initmig4 : DbMigration
    {
        public override void Up()
        {
            // Column already exists in DB (was added by comtables migration which ran before this file was written).
            // Guard prevents duplicate column error on Update-Database.
            Sql(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'RoundRobinQueueAudits' AND COLUMN_NAME = 'TenantComplaintId')
                    ALTER TABLE dbo.RoundRobinQueueAudits ADD TenantComplaintId int NULL;
            ");
        }

        public override void Down()
        {
            DropColumn("dbo.RoundRobinQueueAudits", "TenantComplaintId");
        }
    }
}
