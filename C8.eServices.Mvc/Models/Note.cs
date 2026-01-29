using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class Note:BaseModel
    {
        [Column(Order = 10)]
        [Display(Name = "NoteEntry")]
        [MaxLength(1000)]
        public string NoteEntry { get; set; }

        [Column(Order = 11)]
        [Display(Name = "NoteType")]
        public int NoteTypeId { get; set; }
        [ForeignKey("NoteTypeId")]
        public NoteType NoteType { get; set; }

        [Column(Order = 12)]
        [Display(Name = "ReferenceId")]
        public int ReferenceId { get; set; }

        [Column(Order = 13)]
        [Display(Name = "ReferenceType")]
        public int ReferenceTypeId { get; set; }
        [ForeignKey("ReferenceTypeId")]
        public ReferenceType ReferenceType { get; set; }
    }
}