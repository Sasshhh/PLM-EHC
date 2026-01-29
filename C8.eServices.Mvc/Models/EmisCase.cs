using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{

    public class EmisCase : BaseModel
    {
        /// <summary>
        /// CAS-794311-K8M4N8
        /// </summary>
        /// [Required]
        [Column(Order = 10)]
        [Display(Name = "Case No.")]
        [MaxLength(32)]
        public string CaseNumber { get; set; }

        /// <summary>
        /// ROADS & STORMWATER
        /// </summary>
        [Column(Order = 11)]
        [Display(Name = "Department")]
        [MaxLength(64)]
        public string Department { get; set; }

        /// <summary>
        /// Maintenance
        /// </summary>
        [Column(Order = 12)]
        [Display(Name = "Division")]
        [MaxLength(32)]
        public string Division { get; set; }

        /// <summary>
        /// R1 - Road Infrastructure
        /// </summary>
        [Column(Order = 13)]
        [Display(Name = "Category")]
        [MaxLength(32)]
        public string Category { get; set; }

        /// <summary>
        /// Repair Pothole in major road
        /// </summary>
        [Column(Order = 14)]
        [Display(Name = "Call Type")]
        [MaxLength(32)]
        public string CallType { get; set; }

        /// <summary>
        /// Pothole corner of Honeysuckle and Poinsettia
        /// </summary>
        [Column(Order = 15)]
        [Display(Name = "Description")]
        [MaxLength(256)]
        public string Description { get; set; }

        /// <summary>
        /// 0000
        /// </summary>
        [Column(Order = 16)]
        [Display(Name = "Street No.")]
        [MaxLength(8)]
        public string StreetNumber { get; set; }

        /// <summary>
        /// HONEYSUCKLE STREET
        /// </summary>
        [Column(Order = 17)]
        [Display(Name = "Street Name")]
        [MaxLength(64)]
        public string StreetName { get; set; }

        /// <summary>
        /// MEYERSDAL
        /// </summary>
        [Column(Order = 18)]
        [Display(Name = "Township")]
        [MaxLength(32)]
        public string Township { get; set; }

        /// <summary>
        /// 0829503541
        /// </summary>
        [Column(Order = 19)]
        [Display(Name = "Telephone No.")]
        [MaxLength(12)]
        public string TelephoneNumber { get; set; }

        /// <summary>
        /// ALBERTON
        /// </summary>
        [Column(Order = 20)]
        [Display(Name = "Nearest CCC")]
        [MaxLength(32)]
        public string NearestCCC { get; set; }

        /// <summary>
        /// 24
        /// </summary>
        [Column(Order = 21)]
        [Display(Name = "SLA")]
        [MaxLength(2)]
        public string ServiceLevelAgreement { get; set; }

        [Column(Order = 22)]
        [Display(Name = "Status")]
        public int StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }

        [Column(Order = 23)]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }
    }
}