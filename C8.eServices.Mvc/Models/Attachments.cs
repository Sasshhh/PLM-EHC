using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class Attachments : BaseModel
    {
        [Column(Order = 11)]
        [Display(Name = "File Name")]
        [StringLength(1024)]
        public string FileName { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Type")]
        [StringLength(1024)]

        public string Type { get; set; }


        [Column(Order = 14)]
        [Display(Name = "ReferenceIDNO")]
        public string ReferenceIDNO { get; set; }

        [Column(Order = 15)]
        [Display(Name = "FilePath")]
        public string FilePath { get; set; }


        [Column(Order = 22)]
        [Display(Name = "Property Lease Application")]
        public int? PropertyLeaseApplicationId { get; set; }
        [ForeignKey("PropertyLeaseApplicationId")]
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }

        [Column(Order = 23)]
        [Display(Name = "Document Type")]
        public int? DocumentTypeId { get; set; }
        [ForeignKey("DocumentTypeId")]
        public DocumentType DocumentType { get; set; }
    }
}