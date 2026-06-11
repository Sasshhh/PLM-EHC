namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initmigdevtoprodprelive2 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.PropertyLeaseRenewalOfferAudits", "PropertyLeaseApplicationId", "dbo.PropertyLeaseApplications");
            DropIndex("dbo.PropertyLeaseRenewalOfferAudits", new[] { "PropertyLeaseApplicationId" });
            AddColumn("dbo.TenantComplaints", "EscalationTriggered", c => c.Boolean(nullable: false));
            AddColumn("dbo.TenantComplaints", "EscalationDate", c => c.DateTime());
            AddColumn("dbo.DateToSchedules", "TenantComplaintId", c => c.Int());
            AddColumn("dbo.InspectionSchedules", "TenantComplaintId", c => c.Int());
            AddColumn("dbo.PropertyLeaseRenewalOfferAudits", "ProposedEndDate", c => c.DateTime());
            AddColumn("dbo.PropertyLeaseRenewalOfferAudits", "ProposedRenewalNotice", c => c.DateTime());
            AddColumn("dbo.PropertyLeaseRenewalOfferAudits", "ProposedTerminationNotice", c => c.DateTime());
            AddColumn("dbo.PropertyLeaseRenewalOfferAudits", "CSO_Outcome", c => c.String());
            AddColumn("dbo.PropertyLeaseRenewalOfferAudits", "CSO_Comment", c => c.String());
            AddColumn("dbo.PropertyLeaseRenewalOfferAudits", "CSO_Date", c => c.DateTime());
            AddColumn("dbo.PropertyLeaseRenewalOfferAudits", "CSO_SystemUserId", c => c.Int());
            AddColumn("dbo.PropertyLeaseRenewalOfferAudits", "RM_Outcome", c => c.String());
            AddColumn("dbo.PropertyLeaseRenewalOfferAudits", "RM_Comment", c => c.String());
            AddColumn("dbo.PropertyLeaseRenewalOfferAudits", "RM_Date", c => c.DateTime());
            AddColumn("dbo.PropertyLeaseRenewalOfferAudits", "RM_SystemUserId", c => c.Int());
            AddColumn("dbo.PropertyLeaseRenewalOfferAudits", "CEO_Outcome", c => c.String());
            AddColumn("dbo.PropertyLeaseRenewalOfferAudits", "CEO_Comment", c => c.String());
            AddColumn("dbo.PropertyLeaseRenewalOfferAudits", "CEO_Date", c => c.DateTime());
            AddColumn("dbo.PropertyLeaseRenewalOfferAudits", "CEO_SystemUserId", c => c.Int());
            AddColumn("dbo.PropertyLeaseRenewalOfferAudits", "CustomerDeclineReason", c => c.String());
            AddColumn("dbo.PropertyLeaseRenewalOfferAudits", "CustomerResponseDate", c => c.DateTime());
            AddColumn("dbo.PropertyLeaseRenewalOffers", "ProposedEndDate", c => c.DateTime());
            AddColumn("dbo.PropertyLeaseRenewalOffers", "ProposedRenewalNotice", c => c.DateTime());
            AddColumn("dbo.PropertyLeaseRenewalOffers", "ProposedTerminationNotice", c => c.DateTime());
            AddColumn("dbo.PropertyLeaseRenewalOffers", "CSO_Outcome", c => c.String());
            AddColumn("dbo.PropertyLeaseRenewalOffers", "CSO_Comment", c => c.String());
            AddColumn("dbo.PropertyLeaseRenewalOffers", "CSO_Date", c => c.DateTime());
            AddColumn("dbo.PropertyLeaseRenewalOffers", "CSO_SystemUserId", c => c.Int());
            AddColumn("dbo.PropertyLeaseRenewalOffers", "RM_Outcome", c => c.String());
            AddColumn("dbo.PropertyLeaseRenewalOffers", "RM_Comment", c => c.String());
            AddColumn("dbo.PropertyLeaseRenewalOffers", "RM_Date", c => c.DateTime());
            AddColumn("dbo.PropertyLeaseRenewalOffers", "RM_SystemUserId", c => c.Int());
            AddColumn("dbo.PropertyLeaseRenewalOffers", "CEO_Outcome", c => c.String());
            AddColumn("dbo.PropertyLeaseRenewalOffers", "CEO_Comment", c => c.String());
            AddColumn("dbo.PropertyLeaseRenewalOffers", "CEO_Date", c => c.DateTime());
            AddColumn("dbo.PropertyLeaseRenewalOffers", "CEO_SystemUserId", c => c.Int());
            AddColumn("dbo.PropertyLeaseRenewalOffers", "CustomerDeclineReason", c => c.String());
            AddColumn("dbo.PropertyLeaseRenewalOffers", "CustomerResponseDate", c => c.DateTime());
            AddColumn("dbo.RoundRobinQueues", "ServiceRequestId", c => c.Int());
            AddColumn("dbo.ServiceRequests", "AssignedToId", c => c.Int());
            AddColumn("dbo.ServiceRequests", "DateAssigned", c => c.DateTime());
            CreateIndex("dbo.DateToSchedules", "TenantComplaintId");
            CreateIndex("dbo.InspectionSchedules", "TenantComplaintId");
            CreateIndex("dbo.PropertyLeaseRenewalOffers", "CSO_SystemUserId");
            CreateIndex("dbo.PropertyLeaseRenewalOffers", "RM_SystemUserId");
            CreateIndex("dbo.PropertyLeaseRenewalOffers", "CEO_SystemUserId");
            CreateIndex("dbo.RoundRobinQueues", "ServiceRequestId");
            CreateIndex("dbo.ServiceRequests", "AssignedToId");
            AddForeignKey("dbo.DateToSchedules", "TenantComplaintId", "dbo.TenantComplaints", "Id");
            AddForeignKey("dbo.InspectionSchedules", "TenantComplaintId", "dbo.TenantComplaints", "Id");
            AddForeignKey("dbo.PropertyLeaseRenewalOffers", "CEO_SystemUserId", "dbo.SystemUsers", "Id");
            AddForeignKey("dbo.PropertyLeaseRenewalOffers", "CSO_SystemUserId", "dbo.SystemUsers", "Id");
            AddForeignKey("dbo.PropertyLeaseRenewalOffers", "RM_SystemUserId", "dbo.SystemUsers", "Id");
            AddForeignKey("dbo.ServiceRequests", "AssignedToId", "dbo.Customers", "Id");
            AddForeignKey("dbo.RoundRobinQueues", "ServiceRequestId", "dbo.ServiceRequests", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RoundRobinQueues", "ServiceRequestId", "dbo.ServiceRequests");
            DropForeignKey("dbo.ServiceRequests", "AssignedToId", "dbo.Customers");
            DropForeignKey("dbo.PropertyLeaseRenewalOffers", "RM_SystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.PropertyLeaseRenewalOffers", "CSO_SystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.PropertyLeaseRenewalOffers", "CEO_SystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.InspectionSchedules", "TenantComplaintId", "dbo.TenantComplaints");
            DropForeignKey("dbo.DateToSchedules", "TenantComplaintId", "dbo.TenantComplaints");
            DropIndex("dbo.ServiceRequests", new[] { "AssignedToId" });
            DropIndex("dbo.RoundRobinQueues", new[] { "ServiceRequestId" });
            DropIndex("dbo.PropertyLeaseRenewalOffers", new[] { "CEO_SystemUserId" });
            DropIndex("dbo.PropertyLeaseRenewalOffers", new[] { "RM_SystemUserId" });
            DropIndex("dbo.PropertyLeaseRenewalOffers", new[] { "CSO_SystemUserId" });
            DropIndex("dbo.InspectionSchedules", new[] { "TenantComplaintId" });
            DropIndex("dbo.DateToSchedules", new[] { "TenantComplaintId" });
            DropColumn("dbo.ServiceRequests", "DateAssigned");
            DropColumn("dbo.ServiceRequests", "AssignedToId");
            DropColumn("dbo.RoundRobinQueues", "ServiceRequestId");
            DropColumn("dbo.PropertyLeaseRenewalOffers", "CustomerResponseDate");
            DropColumn("dbo.PropertyLeaseRenewalOffers", "CustomerDeclineReason");
            DropColumn("dbo.PropertyLeaseRenewalOffers", "CEO_SystemUserId");
            DropColumn("dbo.PropertyLeaseRenewalOffers", "CEO_Date");
            DropColumn("dbo.PropertyLeaseRenewalOffers", "CEO_Comment");
            DropColumn("dbo.PropertyLeaseRenewalOffers", "CEO_Outcome");
            DropColumn("dbo.PropertyLeaseRenewalOffers", "RM_SystemUserId");
            DropColumn("dbo.PropertyLeaseRenewalOffers", "RM_Date");
            DropColumn("dbo.PropertyLeaseRenewalOffers", "RM_Comment");
            DropColumn("dbo.PropertyLeaseRenewalOffers", "RM_Outcome");
            DropColumn("dbo.PropertyLeaseRenewalOffers", "CSO_SystemUserId");
            DropColumn("dbo.PropertyLeaseRenewalOffers", "CSO_Date");
            DropColumn("dbo.PropertyLeaseRenewalOffers", "CSO_Comment");
            DropColumn("dbo.PropertyLeaseRenewalOffers", "CSO_Outcome");
            DropColumn("dbo.PropertyLeaseRenewalOffers", "ProposedTerminationNotice");
            DropColumn("dbo.PropertyLeaseRenewalOffers", "ProposedRenewalNotice");
            DropColumn("dbo.PropertyLeaseRenewalOffers", "ProposedEndDate");
            DropColumn("dbo.PropertyLeaseRenewalOfferAudits", "CustomerResponseDate");
            DropColumn("dbo.PropertyLeaseRenewalOfferAudits", "CustomerDeclineReason");
            DropColumn("dbo.PropertyLeaseRenewalOfferAudits", "CEO_SystemUserId");
            DropColumn("dbo.PropertyLeaseRenewalOfferAudits", "CEO_Date");
            DropColumn("dbo.PropertyLeaseRenewalOfferAudits", "CEO_Comment");
            DropColumn("dbo.PropertyLeaseRenewalOfferAudits", "CEO_Outcome");
            DropColumn("dbo.PropertyLeaseRenewalOfferAudits", "RM_SystemUserId");
            DropColumn("dbo.PropertyLeaseRenewalOfferAudits", "RM_Date");
            DropColumn("dbo.PropertyLeaseRenewalOfferAudits", "RM_Comment");
            DropColumn("dbo.PropertyLeaseRenewalOfferAudits", "RM_Outcome");
            DropColumn("dbo.PropertyLeaseRenewalOfferAudits", "CSO_SystemUserId");
            DropColumn("dbo.PropertyLeaseRenewalOfferAudits", "CSO_Date");
            DropColumn("dbo.PropertyLeaseRenewalOfferAudits", "CSO_Comment");
            DropColumn("dbo.PropertyLeaseRenewalOfferAudits", "CSO_Outcome");
            DropColumn("dbo.PropertyLeaseRenewalOfferAudits", "ProposedTerminationNotice");
            DropColumn("dbo.PropertyLeaseRenewalOfferAudits", "ProposedRenewalNotice");
            DropColumn("dbo.PropertyLeaseRenewalOfferAudits", "ProposedEndDate");
            DropColumn("dbo.InspectionSchedules", "TenantComplaintId");
            DropColumn("dbo.DateToSchedules", "TenantComplaintId");
            DropColumn("dbo.TenantComplaints", "EscalationDate");
            DropColumn("dbo.TenantComplaints", "EscalationTriggered");
            CreateIndex("dbo.PropertyLeaseRenewalOfferAudits", "PropertyLeaseApplicationId");
            AddForeignKey("dbo.PropertyLeaseRenewalOfferAudits", "PropertyLeaseApplicationId", "dbo.PropertyLeaseApplications", "Id");
        }
    }
}
