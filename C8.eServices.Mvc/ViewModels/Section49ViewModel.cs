using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace C8.eServices.Mvc.ViewModels
{
    public class Section49ViewModel
    {
        [Display(Name = "Section 49 Letter Generation Date")]
        public DateTime GenerationDate { get; set; }
        [Display(Name = "Date the Valuation was Placed on the Property")]
        public DateTime ValuationPlacedDate { get; set; }
        [Display(Name = "Effective From Date")]
        public DateTime EffectiveDateFrom { get; set; }
        [Display(Name = "Effective To Date")]
        public DateTime EffectiveDateTo { get; set; }
        [Display(Name = "Property Address")]
        public string Address { get; set; }
        [Display(Name = "Published Date")]
        public string PublishedDate { get; set; }
        [Display(Name = "General Valuation Roll No")]
        public int GVRNo { get; set; }
        [Display(Name = "Implemention Date")]
        public DateTime ImplementionDate { get; set; }
        [Display(Name = "Generation Type")]
        public string Type { get; set; }
    }
}