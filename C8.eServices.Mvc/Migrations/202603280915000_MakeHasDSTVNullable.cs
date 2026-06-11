namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MakeHasDSTVNullable : DbMigration
    {
        public override void Up()
        {
            // Idempotent: Only alter if column exists and is not already nullable
            Sql(@"
                -- PropertyLeaseApplications
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PropertyLeaseApplications') AND name = 'HasDSTV' AND is_nullable = 0)
                    ALTER TABLE dbo.PropertyLeaseApplications ALTER COLUMN HasDSTV BIT NULL
                ELSE IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PropertyLeaseApplications') AND name = 'HasDSTV')
                    ALTER TABLE dbo.PropertyLeaseApplications ADD HasDSTV BIT NULL

                -- PropertyLeaseApplicationAudits
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PropertyLeaseApplicationAudits') AND name = 'HasDSTV' AND is_nullable = 0)
                    ALTER TABLE dbo.PropertyLeaseApplicationAudits ALTER COLUMN HasDSTV BIT NULL
                ELSE IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PropertyLeaseApplicationAudits') AND name = 'HasDSTV')
                    ALTER TABLE dbo.PropertyLeaseApplicationAudits ADD HasDSTV BIT NULL

                -- PropertyLeaseAgreementMasters
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PropertyLeaseAgreementMasters') AND name = 'HasDSTV' AND is_nullable = 0)
                    ALTER TABLE dbo.PropertyLeaseAgreementMasters ALTER COLUMN HasDSTV BIT NULL
                ELSE IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PropertyLeaseAgreementMasters') AND name = 'HasDSTV')
                    ALTER TABLE dbo.PropertyLeaseAgreementMasters ADD HasDSTV BIT NULL

                -- PropertyLeaseAgreementMasterAudits
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PropertyLeaseAgreementMasterAudits') AND name = 'HasDSTV' AND is_nullable = 0)
                    ALTER TABLE dbo.PropertyLeaseAgreementMasterAudits ALTER COLUMN HasDSTV BIT NULL
                ELSE IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PropertyLeaseAgreementMasterAudits') AND name = 'HasDSTV')
                    ALTER TABLE dbo.PropertyLeaseAgreementMasterAudits ADD HasDSTV BIT NULL
            ");
        }
        
        public override void Down()
        {
            // Revert HasDSTV column to non-nullable in all tables
            // Note: This will fail if NULL values exist in the database
            AlterColumn("dbo.PropertyLeaseAgreementMasterAudits", "HasDSTV", c => c.Boolean(nullable: false));
            AlterColumn("dbo.PropertyLeaseAgreementMasters", "HasDSTV", c => c.Boolean(nullable: false));
            AlterColumn("dbo.PropertyLeaseApplicationAudits", "HasDSTV", c => c.Boolean(nullable: false));
            AlterColumn("dbo.PropertyLeaseApplications", "HasDSTV", c => c.Boolean(nullable: false));
        }
    }
}
