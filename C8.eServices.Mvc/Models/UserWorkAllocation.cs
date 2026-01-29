using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace C8.eServices.Mvc.Models
{
    public class UserWorkAllocation : BaseModel
    {
        //[Column(Order = 1)]
        //[Display(Name = "Id")]
        //public int Id { get; set; }

        [Column(Order = 10)]
        [Display(Name = "Role(s)")]
        public string Roles { get; set; }

        [Column(Order = 11)]
        public int? SystemUserId { get; set; }
        [ForeignKey("SystemUserId")]
        [Display(Name = "Assigned To")]
        [ScriptIgnore]
        public SystemUser SystemUser { get; set; }

        [Column(Order = 12)]
        public int? PreferredComplexAreaId { get; set; }
        [ForeignKey("PreferredComplexAreaId")]
        [Display(Name = "Preferred Complex/Area")]
        [ScriptIgnore]
        public PreferredComplexArea PreferredComplexArea { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Role(s)")]
        public bool RRActive { get; set; }
    }
}