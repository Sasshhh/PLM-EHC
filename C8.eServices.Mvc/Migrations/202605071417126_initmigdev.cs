namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initmigdev : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PropertyLeaseAgreementMasters", "RenewalTenantSignature", c => c.String());
            AddColumn("dbo.PropertyLeaseAgreementMasters", "RenewalTenantSigned", c => c.Boolean(nullable: false));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "RenewalTenantSignatureDate", c => c.DateTime());
            AddColumn("dbo.PropertyLeaseAgreementMasters", "RenewalWitness1Signature", c => c.String());
            AddColumn("dbo.PropertyLeaseAgreementMasters", "RenewalWitness1Name", c => c.String(maxLength: 200));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "RenewalWitness1SignatureDate", c => c.DateTime());
            AddColumn("dbo.PropertyLeaseAgreementMasters", "RenewalPropertyManagersSignature", c => c.String());
            AddColumn("dbo.PropertyLeaseAgreementMasters", "RenewalPropertyManagerSigned", c => c.Boolean(nullable: false));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "RenewalPropertyManagerSignatureDate", c => c.DateTime());
            AddColumn("dbo.PropertyLeaseAgreementMasters", "RenewalRevenueManagersSignature", c => c.String());
            AddColumn("dbo.PropertyLeaseAgreementMasters", "RenewalRevenueManagerSigned", c => c.Boolean(nullable: false));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "RenewalRevenueManagerSignatureDate", c => c.DateTime());
            AddColumn("dbo.PropertyLeaseAgreementMasters", "RenewalSignatureMainLessee", c => c.String());
            AddColumn("dbo.PropertyLeaseAgreementMasters", "RenewalMainLesseeSigned", c => c.Boolean(nullable: false));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "RenewalMainLesseeSignatureDate", c => c.DateTime());
            AddColumn("dbo.PropertyLeaseAgreementMasters", "RenewalSignatureOfSpouse", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PropertyLeaseAgreementMasters", "RenewalSignatureOfSpouse");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "RenewalMainLesseeSignatureDate");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "RenewalMainLesseeSigned");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "RenewalSignatureMainLessee");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "RenewalRevenueManagerSignatureDate");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "RenewalRevenueManagerSigned");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "RenewalRevenueManagersSignature");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "RenewalPropertyManagerSignatureDate");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "RenewalPropertyManagerSigned");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "RenewalPropertyManagersSignature");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "RenewalWitness1SignatureDate");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "RenewalWitness1Name");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "RenewalWitness1Signature");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "RenewalTenantSignatureDate");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "RenewalTenantSigned");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "RenewalTenantSignature");
        }
    }
}
