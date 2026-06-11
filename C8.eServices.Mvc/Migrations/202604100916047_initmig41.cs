namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initmig41 : DbMigration
    {
        public override void Up()
        {
            // ComplaintAuditLogs was created by SQL script and is missing the BaseModel columns.
            // Using conditional SQL so this is safe to run even if columns were already added directly.
            Sql(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ComplaintAuditLogs' AND COLUMN_NAME = 'IsLocked')
                    ALTER TABLE dbo.ComplaintAuditLogs ADD IsLocked bit NULL;
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ComplaintAuditLogs' AND COLUMN_NAME = 'CreatedBySystemUserId')
                    ALTER TABLE dbo.ComplaintAuditLogs ADD CreatedBySystemUserId int NULL;
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ComplaintAuditLogs' AND COLUMN_NAME = 'ModifiedBySystemUserId')
                    ALTER TABLE dbo.ComplaintAuditLogs ADD ModifiedBySystemUserId int NULL;
            ");
        }

        public override void Down()
        {
            DropColumn("dbo.ComplaintAuditLogs", "ModifiedBySystemUserId");
            DropColumn("dbo.ComplaintAuditLogs", "CreatedBySystemUserId");
            DropColumn("dbo.ComplaintAuditLogs", "IsLocked");
        }
    }
}
