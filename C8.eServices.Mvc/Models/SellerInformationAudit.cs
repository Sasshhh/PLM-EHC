using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;
using C8.eServices.Mvc.Models.Audits;

namespace C8.eServices.Mvc.Models
{
    public class SellerInformationAudit : BaseModelAudit
    {
        [Column(Order = 20)]
        [StringLength(50)]
        [Display(Name = "Name of Owner")]
        public string OwnerName { get; set; }

        [Column(Order = 12)]
        [Display(Name = "ID No. of Owner")]

        public int OwnerIdNumber { get; set; }

        [Display(Name = "Nationality")]
        [Column(Order = 13)]
        [StringLength(20)]
        public string Nationality { get; set; }

        [Display(Name = "Passport No")]
        [Column(Order = 14)]

        public int PassportNo { get; set; }

        [Display(Name = "Company Reg No")]
        [Column(Order = 15)]
        [StringLength(20)]
        public string CompanyRegNo { get; set; }

        [Display(Name = "Cell No")]
        [Column(Order = 16)]
        [StringLength(20)]
        public string CellNo { get; set; }

        [Display(Name = "Home No")]
        [Column(Order = 17)]
        [StringLength(20)]
        public string HomeNo { get; set; }


        [Display(Name = "Work No")]
        [Column(Order = 18)]
        [StringLength(20)]
        public string WorkNo { get; set; }


        [Display(Name = "Email Address")]
        [Column(Order = 19)]
        [StringLength(50)]
        public string EmailAddress { get; set; }

     

        [Column(Order = 21)]
        [Display(Name = "RCSApplicationStatusId")]
        public int? RCSApplicationStatusId { get; set; }
        [ForeignKey("RCSApplicationStatusId")]
        public RCSApplicationStatus RCSApplicationStatus { get; set; }
    }
}