namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBankingDetailsToLeaseAgreementMaster : DbMigration
    {
        public override void Up()
        {
            // Idempotent: Only add columns if they don't exist
            Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PropertyLeaseAgreementMasters') AND name = 'TenantBankName')
                    ALTER TABLE dbo.PropertyLeaseAgreementMasters ADD TenantBankName NVARCHAR(100) NULL

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PropertyLeaseAgreementMasters') AND name = 'TenantAccountNumber')
                    ALTER TABLE dbo.PropertyLeaseAgreementMasters ADD TenantAccountNumber NVARCHAR(20) NULL

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PropertyLeaseAgreementMasters') AND name = 'TenantAccountHolderName')
                    ALTER TABLE dbo.PropertyLeaseAgreementMasters ADD TenantAccountHolderName NVARCHAR(200) NULL

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PropertyLeaseAgreementMasters') AND name = 'TenantAccountType')
                    ALTER TABLE dbo.PropertyLeaseAgreementMasters ADD TenantAccountType NVARCHAR(50) NULL

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PropertyLeaseAgreementMasters') AND name = 'TenantBranchCode')
                    ALTER TABLE dbo.PropertyLeaseAgreementMasters ADD TenantBranchCode NVARCHAR(10) NULL

                -- Audit table
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PropertyLeaseAgreementMasterAudits') AND name = 'TenantBankName')
                    ALTER TABLE dbo.PropertyLeaseAgreementMasterAudits ADD TenantBankName NVARCHAR(100) NULL

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PropertyLeaseAgreementMasterAudits') AND name = 'TenantAccountNumber')
                    ALTER TABLE dbo.PropertyLeaseAgreementMasterAudits ADD TenantAccountNumber NVARCHAR(20) NULL

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PropertyLeaseAgreementMasterAudits') AND name = 'TenantAccountHolderName')
                    ALTER TABLE dbo.PropertyLeaseAgreementMasterAudits ADD TenantAccountHolderName NVARCHAR(200) NULL

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PropertyLeaseAgreementMasterAudits') AND name = 'TenantAccountType')
                    ALTER TABLE dbo.PropertyLeaseAgreementMasterAudits ADD TenantAccountType NVARCHAR(50) NULL

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PropertyLeaseAgreementMasterAudits') AND name = 'TenantBranchCode')
                    ALTER TABLE dbo.PropertyLeaseAgreementMasterAudits ADD TenantBranchCode NVARCHAR(10) NULL
            ");
        }
        
        public override void Down()
        {
            // Remove banking fields from PropertyLeaseAgreementMasterAudit
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantBranchCode");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantAccountType");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantAccountHolderName");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantAccountNumber");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantBankName");
            
            // Remove banking fields from PropertyLeaseAgreementMaster
            DropColumn("dbo.PropertyLeaseAgreementMasters", "TenantBranchCode");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "TenantAccountType");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "TenantAccountHolderName");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "TenantAccountNumber");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "TenantBankName");
        }
    }
}