using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models.Audits
{
    public class FlagAudit :BaseModelAudit
    {
        [Column(Order = 10)]
        [Display(Name = "Name")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Description")]
        [MaxLength(500)]
        public string Description { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Key")]
        [MaxLength(100)]
        public string Key { get; set; }
        [Column(Order = 13)]
        [Display(Name = "Turn Around Time")]
        public int? TurnAroundDays { get; set; }
    

      

        [Column(Order = 14)]
        [Display(Name = "On Target Color")]
        [MaxLength(50)]
        public string OnTargetColor { get; set; }

        [Column(Order = 15)]
        [Display(Name = "Running Late Color")]
        [MaxLength(50)]
        public string RunningLateColor { get; set; }

        [Column(Order = 16)]
        [Display(Name = "Overdue Color")]
        [MaxLength(50)]
        public string OverdueColor { get; set; }
        [Column(Order = 17)]
        [Display(Name = "Responsibility Type")]
        public int ResponsibilityTypeId { get; set; }
        [ForeignKey("ResponsibilityTypeId")]
        public ResponsibilityType ResponsibilityType { get; set; }




    }
}