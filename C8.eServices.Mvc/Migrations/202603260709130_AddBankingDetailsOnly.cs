namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBankingDetailsOnly : DbMigration
    {
        public override void Up()
        {
            // Add banking fields to PropertyLeaseAgreementMasterAudits
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantBankName", c => c.String(maxLength: 100));
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantAccountNumber", c => c.String(maxLength: 20));
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantAccountHolderName", c => c.String(maxLength: 200));
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantAccountType", c => c.String(maxLength: 50));
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantBranchCode", c => c.String(maxLength: 10));

            // Add banking fields to PropertyLeaseAgreementMasters
            AddColumn("dbo.PropertyLeaseAgreementMasters", "TenantBankName", c => c.String(maxLength: 100));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "TenantAccountNumber", c => c.String(maxLength: 20));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "TenantAccountHolderName", c => c.String(maxLength: 200));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "TenantAccountType", c => c.String(maxLength: 50));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "TenantBranchCode", c => c.String(maxLength: 10));
        }

        public override void Down()
        {
            // Remove banking fields from PropertyLeaseAgreementMasters
            DropColumn("dbo.PropertyLeaseAgreementMasters", "TenantBranchCode");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "TenantAccountType");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "TenantAccountHolderName");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "TenantAccountNumber");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "TenantBankName");

            // Remove banking fields from PropertyLeaseAgreementMasterAudits
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantBranchCode");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantAccountType");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantAccountHolderName");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantAccountNumber");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantBankName");
        }
    }
}
