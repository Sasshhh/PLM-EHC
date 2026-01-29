using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.Models
{
    public class RatesRebate : BaseModel
    {
        [Column( Order = 10 )]
        [MaxLength(100)]
        [Display(Name = "Application No")]
        public string ApplicationNumber { get; set; }

        [Column( Order = 11 )]
        [Display( Name = "Applicant" )]
        public int ApplicantCustomerId { get; set; }
        [ForeignKey( "ApplicantCustomerId" )]
        public Customer ApplicantCustomer { get; set; }

        [Column( Order = 12 )]
        [Display( Name = "Owner" )]
        public int OwnerCustomerId { get; set; }
        [ForeignKey( "OwnerCustomerId" )]
        public Customer OwnerCustomer { get; set; }

        [Column( Order = 13 )]
        [Display( Name = "Executor" )]
        public int? ExecutorCustomerId { get; set; }
        [ForeignKey( "ExecutorCustomerId" )]
        public Customer ExecutorCustomer { get; set; }

        [Column( Order = 14 )]
        [Display( Name = "Executor" )]
        public int? ExecutorId { get; set; }
        [ForeignKey( "ExecutorId" )]
        public Executor Executor { get; set; }

        [Column( Order = 15 )]
        [Display( Name = "Domicilium" )]
        public int DomiciliumId { get; set; }
        [ForeignKey( "DomiciliumId" )]
        public Domicilium Domicilium { get; set; }

        [Column( Order = 16 )]
        [Display( Name = "In your own capacity" )]
        public bool InOwnCapacity { get; set; }

        [Column( Order = 17 )]
        [Display( Name = "Declared" )]
        public bool IsDeclared { get; set; }

        [Column( Order = 18 )]
        [Display( Name = "Status" )]
        public int StatusId { get; set; }
        [ForeignKey( "StatusId" )]
        public Status Status { get; set; }

        [Column(Order = 19)]
        public bool IsManual { get; set; }

        [Column(Order = 20)]
        public int? ClerkUserId { get; set; }
        [ForeignKey("ClerkUserId")]
        [Display(Name = "Clerk Id")]
        public SystemUser ClerkId { get; set; }

        [Column(Order = 21)]
        public bool IsConflict { get; set; }

        [Display(Name = "ApplicationType")]
        public string ApplicationType
        {
            get
            {
                if (IsManual)
                    return "Manual";
                else
                    return "Electronic";
            }
        }
    }
}