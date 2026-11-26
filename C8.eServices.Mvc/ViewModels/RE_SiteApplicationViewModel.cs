using System.Collections.Generic;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.ViewModels
{
    /// <summary>DPRE UC05 Scenarios 2-4 (Outdoor Advertising, Telecommunications, Concession/Leasehold) form.</summary>
    public class RE_SiteApplicationViewModel
    {
        public RE_Application Application { get; set; }
        public RE_ApplicationSiteDetail Site { get; set; }
        public string LeaseType { get; set; }
        public string FormCode { get; set; }          // OA / TEL / LH
        public string FormTitle { get; set; }
        public string ReferencePrefix { get; set; }   // e.g. DPRE-OA
        public List<string> UploadedDocuments { get; set; }

        public RE_SiteApplicationViewModel()
        {
            UploadedDocuments = new List<string>();
        }
    }
}
