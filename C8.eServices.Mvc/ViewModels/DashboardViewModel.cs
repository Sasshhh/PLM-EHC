using C8.eServices.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.ViewModels
{
    public class DashboardViewModel
    {
        public List<RoundRobinQueue> RenewalFirstRecommendation { get; set; }
        public List<RoundRobinQueue> AgreementRenewalAcceptance { get; set; }
        public List<RoundRobinQueue> RenewalUploadDocs { get; set; }
        public List<RoundRobinQueue> GenerateLeaseAgreement { get; set; }
        public List<RoundRobinQueue> AgreementSignature { get; set; }
        public List<RoundRobinQueue> FinalizeAgreement { get; set; }
        public List<RoundRobinQueue> UpdateAgreement { get; set; }
        public List<RoundRobinQueue> NoticedAgreements { get; set; }
        public List<RoundRobinQueue> PendingTakeOff { get; set; }
        public List<RoundRobinQueue> PostInspection { get; set; }
        public List<RoundRobinQueue> Renewal2ndRecommendation { get; set; }
        public List<RoundRobinQueue> AgreementReview { get; set; }
        public List<RoundRobinQueue> PendingTerminationreview { get; set; }
        public List<RoundRobinQueue> RenewalReview { get; set; }
        public List<RoundRobinQueue> AgreementApproval { get; set; }
        public int AllApplications { get; set; }
        public int MasterApplications { get; set; }
        public int NewApplications { get; set; }
        public int CurrentAgreeentsSigned { get; set; }


    }
}