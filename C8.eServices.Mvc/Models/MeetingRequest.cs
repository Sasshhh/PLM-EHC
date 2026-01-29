using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class MeetingRequest:BaseModel
    {

        [Column(Order = 10)]
        [Display(Name = "Customer")]
        public int? CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Reference Type")]
        public int? ReferenceTypeId { get; set; }
        [ForeignKey("ReferenceTypeId")]
        public ReferenceType ReferenceType { get; set; }

        [Column(Order = 12)]
        public int ReferenceId { get; set; }

       



        [Column(Order = 16)]
        [Display(Name = "Status")]
        public int? StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }

        [Column(Order = 17)]
        public DateTime? MeetingDate { get; set; }


        [Column(Order = 18)]
        public DateTime? MeetingTime { get; set; }

        [Column(Order = 20)]
        [MaxLength(500)]
        [Display(Name = "Meeting Comments")]
        public string Comment { get; set; }

        [Column(Order = 21)]
        [MaxLength(500)]
        [Display(Name = "Meeting Venue")]
        public string MeetingVenue { get; set; }

        [Column(Order = 22)]
        [Display(Name = "Property Lease Application")]
        public int? PropertyLeaseApplicationId { get; set; }
        [ForeignKey("PropertyLeaseApplicationId")]
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }


    }
}