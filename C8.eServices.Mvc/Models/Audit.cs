using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class Audit
    {
        [Key]
        public int AuditId { get; set; }

        [Display(Name="Action")]
        public string Action { get; set; }

        [Display( Name = "Id" )]
        public int PrimaryKey { get; set; }

        [Display( Name = "Model" )]
        public string TableName { get; set; }

        [Display( Name = "Field" )]
        public string ColumnName { get; set; }

        [Display( Name = "Original" )]
        public string OriginalValue { get; set; }

        [Display( Name = "Current" )]
        public string CurrentValue { get; set; }

        [Display( Name = "User" )]
        public int? AuditBySystemUserId { get; set; }
        [ForeignKey("AuditBySystemUserId")]
        public SystemUser AuditBySystemUser { get; set; }

        [Display( Name = "Date Time" )]
        public DateTime AuditDateTime { get; set; }

        [Display(Name = "IP Address")]
        public string IPAddress { get; set; }
    }

    public class AuditEnitity
    {
        public Audit Audit { get; set; }
        public object Entity { get; set; }
    }
}