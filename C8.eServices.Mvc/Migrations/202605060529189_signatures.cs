namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class signatures : DbMigration
    {
        public override void Up()
        {
            // DropForeignKey("dbo.PropertyLeaseRenewalOfferAudits", "PropertyLeaseApplicationId", "dbo.PropertyLeaseApplications");
            // DropIndex("dbo.PropertyLeaseRenewalOfferAudits", new[] { "PropertyLeaseApplicationId" });
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
            // The following columns already exist in the database from a previous update
            // AddColumn("dbo.PropertyLeaseRenewalOfferAudits", "ProposedEndDate", c => c.DateTime());
            // AddColumn("dbo.PropertyLeaseRenewalOfferAudits", "ProposedRenewalNotice", c => c.DateTime());
            // AddColumn("dbo.PropertyLeaseRenewalOfferAudits", "ProposedTerminationNotice", c => c.DateTime());
            // AddColumn("dbo.PropertyLeaseRenewalOfferAudits", "CSO_Outcome", c => c.String());
            // AddColumn("dbo.PropertyLeaseRenewalOfferAudits", "CSO_Comment", c => c.String());
            // AddColumn("dbo.PropertyLeaseRenewalOfferAudits", "CSO_Date", c => c.DateTime());
            // AddColumn("dbo.PropertyLeaseRenewalOfferAudits", "CSO_SystemUserId", c => c.Int());
            // AddColumn("dbo.PropertyLeaseRenewalOfferAudits", "RM_Outcome", c => c.String());
            // AddColumn("dbo.PropertyLeaseRenewalOfferAudits", "RM_Comment", c => c.String());
            // AddColumn("dbo.PropertyLeaseRenewalOfferAudits", "RM_Date", c => c.DateTime());
            // AddColumn("dbo.PropertyLeaseRenewalOfferAudits", "RM_SystemUserId", c => c.Int());
            // AddColumn("dbo.PropertyLeaseRenewalOfferAudits", "CEO_Outcome", c => c.String());
            // AddColumn("dbo.PropertyLeaseRenewalOfferAudits", "CEO_Comment", c => c.String());
            // AddColumn("dbo.PropertyLeaseRenewalOfferAudits", "CEO_Date", c => c.DateTime());
            // AddColumn("dbo.PropertyLeaseRenewalOfferAudits", "CEO_SystemUserId", c => c.Int());
            // AddColumn("dbo.PropertyLeaseRenewalOfferAudits", "CustomerDeclineReason", c => c.String());
            // AddColumn("dbo.PropertyLeaseRenewalOfferAudits", "CustomerResponseDate", c => c.DateTime());
            // AddColumn("dbo.PropertyLeaseRenewalOffers", "ProposedEndDate", c => c.DateTime());
            // AddColumn("dbo.PropertyLeaseRenewalOffers", "ProposedRenewalNotice", c => c.DateTime());
            // AddColumn("dbo.PropertyLeaseRenewalOffers", "ProposedTerminationNotice", c => c.DateTime());
            // AddColumn("dbo.PropertyLeaseRenewalOffers", "CSO_Outcome", c => c.String());
            // AddColumn("dbo.PropertyLeaseRenewalOffers", "CSO_Comment", c => c.String());
            // AddColumn("dbo.PropertyLeaseRenewalOffers", "CSO_Date", c => c.DateTime());
            // AddColumn("dbo.PropertyLeaseRenewalOffers", "CSO_SystemUserId", c => c.Int());
            // AddColumn("dbo.PropertyLeaseRenewalOffers", "RM_Outcome", c => c.String());
            // AddColumn("dbo.PropertyLeaseRenewalOffers", "RM_Comment", c => c.String());
            // AddColumn("dbo.PropertyLeaseRenewalOffers", "RM_Date", c => c.DateTime());
            // AddColumn("dbo.PropertyLeaseRenewalOffers", "RM_SystemUserId", c => c.Int());
            // AddColumn("dbo.PropertyLeaseRenewalOffers", "CEO_Outcome", c => c.String());
            // AddColumn("dbo.PropertyLeaseRenewalOffers", "CEO_Comment", c => c.String());
            // AddColumn("dbo.PropertyLeaseRenewalOffers", "CEO_Date", c => c.DateTime());
            // AddColumn("dbo.PropertyLeaseRenewalOffers", "CEO_SystemUserId", c => c.Int());
            // AddColumn("dbo.PropertyLeaseRenewalOffers", "CustomerDeclineReason", c => c.String());
            // AddColumn("dbo.PropertyLeaseRenewalOffers", "CustomerResponseDate", c => c.DateTime());
            // CreateIndex("dbo.PropertyLeaseRenewalOffers", "CSO_SystemUserId");
            // CreateIndex("dbo.PropertyLeaseRenewalOffers", "RM_SystemUserId");
            // CreateIndex("dbo.PropertyLeaseRenewalOffers", "CEO_SystemUserId");
            // AddForeignKey("dbo.PropertyLeaseRenewalOffers", "CEO_SystemUserId", "dbo.SystemUsers", "Id");
            // AddForeignKey("dbo.PropertyLeaseRenewalOffers", "CSO_SystemUserId", "dbo.SystemUsers", "Id");
            // AddForeignKey("dbo.PropertyLeaseRenewalOffers", "RM_SystemUserId", "dbo.SystemUsers", "Id");
        }
        
        public override void Down()
        {
            // DropForeignKey("dbo.PropertyLeaseRenewalOffers", "RM_SystemUserId", "dbo.SystemUsers");
            // DropForeignKey("dbo.PropertyLeaseRenewalOffers", "CSO_SystemUserId", "dbo.SystemUsers");
            // DropForeignKey("dbo.PropertyLeaseRenewalOffers", "CEO_SystemUserId", "dbo.SystemUsers");
            // DropIndex("dbo.PropertyLeaseRenewalOffers", new[] { "CEO_SystemUserId" });
            // DropIndex("dbo.PropertyLeaseRenewalOffers", new[] { "RM_SystemUserId" });
            // DropIndex("dbo.PropertyLeaseRenewalOffers", new[] { "CSO_SystemUserId" });
            // DropColumn("dbo.PropertyLeaseRenewalOffers", "CustomerResponseDate");
            // DropColumn("dbo.PropertyLeaseRenewalOffers", "CustomerDeclineReason");
            // DropColumn("dbo.PropertyLeaseRenewalOffers", "CEO_SystemUserId");
            // DropColumn("dbo.PropertyLeaseRenewalOffers", "CEO_Date");
            // DropColumn("dbo.PropertyLeaseRenewalOffers", "CEO_Comment");
            // DropColumn("dbo.PropertyLeaseRenewalOffers", "CEO_Outcome");
            // DropColumn("dbo.PropertyLeaseRenewalOffers", "RM_SystemUserId");
            // DropColumn("dbo.PropertyLeaseRenewalOffers", "RM_Date");
            // DropColumn("dbo.PropertyLeaseRenewalOffers", "RM_Comment");
            // DropColumn("dbo.PropertyLeaseRenewalOffers", "RM_Outcome");
            // DropColumn("dbo.PropertyLeaseRenewalOffers", "CSO_SystemUserId");
            // DropColumn("dbo.PropertyLeaseRenewalOffers", "CSO_Date");
            // DropColumn("dbo.PropertyLeaseRenewalOffers", "CSO_Comment");
            // DropColumn("dbo.PropertyLeaseRenewalOffers", "CSO_Outcome");
            // DropColumn("dbo.PropertyLeaseRenewalOffers", "ProposedTerminationNotice");
            // DropColumn("dbo.PropertyLeaseRenewalOffers", "ProposedRenewalNotice");
            // DropColumn("dbo.PropertyLeaseRenewalOffers", "ProposedEndDate");
            // DropColumn("dbo.PropertyLeaseRenewalOfferAudits", "CustomerResponseDate");
            // DropColumn("dbo.PropertyLeaseRenewalOfferAudits", "CustomerDeclineReason");
            // DropColumn("dbo.PropertyLeaseRenewalOfferAudits", "CEO_SystemUserId");
            // DropColumn("dbo.PropertyLeaseRenewalOfferAudits", "CEO_Date");
            // DropColumn("dbo.PropertyLeaseRenewalOfferAudits", "CEO_Comment");
            // DropColumn("dbo.PropertyLeaseRenewalOfferAudits", "CEO_Outcome");
            // DropColumn("dbo.PropertyLeaseRenewalOfferAudits", "RM_SystemUserId");
            // DropColumn("dbo.PropertyLeaseRenewalOfferAudits", "RM_Date");
            // DropColumn("dbo.PropertyLeaseRenewalOfferAudits", "RM_Comment");
            // DropColumn("dbo.PropertyLeaseRenewalOfferAudits", "RM_Outcome");
            // DropColumn("dbo.PropertyLeaseRenewalOfferAudits", "CSO_SystemUserId");
            // DropColumn("dbo.PropertyLeaseRenewalOfferAudits", "CSO_Date");
            // DropColumn("dbo.PropertyLeaseRenewalOfferAudits", "CSO_Comment");
            // DropColumn("dbo.PropertyLeaseRenewalOfferAudits", "CSO_Outcome");
            // DropColumn("dbo.PropertyLeaseRenewalOfferAudits", "ProposedTerminationNotice");
            // DropColumn("dbo.PropertyLeaseRenewalOfferAudits", "ProposedRenewalNotice");
            // DropColumn("dbo.PropertyLeaseRenewalOfferAudits", "ProposedEndDate");
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
            // CreateIndex("dbo.PropertyLeaseRenewalOfferAudits", "PropertyLeaseApplicationId");
            // AddForeignKey("dbo.PropertyLeaseRenewalOfferAudits", "PropertyLeaseApplicationId", "dbo.PropertyLeaseApplications", "Id");
        }
    }
}
