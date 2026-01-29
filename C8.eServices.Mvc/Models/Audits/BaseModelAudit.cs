using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models.Audits
{
    public class BaseModelAudit
    {
        [Column( Order = 1 )]
        [Key]
        [Display( Name = "Audit Id" )]
        public int AuditId { get; set; }

        [Column( Order = 2 )]
        [StringLength(10)]
        [Display( Name = "Action" )]
        public string Action { get; set; }

        [Column( Order = 3 )]
        [Display( Name = "Id" )]
        public int Id { get; set; }

        [Column(Order = 100)]
        public int? DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        [Display(Name = "Department")]
        public ApplicationEntity Department { get; set; }

        [Column( Order = 101 )]
        [Display( Name = "Is Active" )]
        public bool IsActive { get; set; }

        [Column( Order = 102 )]
        [Display( Name = "Is Deleted" )]
        public bool IsDeleted { get; set; }

        [Column( Order = 103 )]
        [Display( Name = "Is Locked" )]
        public bool? IsLocked { get; set; }

        [Column( Order = 104 )]
        public int? CreatedBySystemUserId { get; set; }
        [ForeignKey( "CreatedBySystemUserId" )]
        [Display( Name = "Created By" )]
        public SystemUser CreatedBySystemUser { get; set; }

        [DisplayFormat( DataFormatString = "{0:yyyy-MM-dd hh:mm:ss}", ApplyFormatInEditMode = true )]
        [Display( Name = "Created On" )]
        [Column( Order = 105 )]
        public DateTime? CreatedDateTime { get; set; }

        [Column( Order = 106 )]
        public int? ModifiedBySystemUserId { get; set; }
        [ForeignKey( "ModifiedBySystemUserId" )]
        [Display( Name = "Modified By" )]
        public SystemUser ModifiedBySystemUser { get; set; }

        [DisplayFormat( DataFormatString = "{0:yyyy-MM-dd hh:mm:ss}", ApplyFormatInEditMode = true )]
        [Display( Name = "Modified On" )]
        [Column( Order = 107 )]
        public DateTime? ModifiedDateTime { get; set; }
    }
}