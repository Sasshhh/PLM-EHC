using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class LeaseForm
    {
        [Key]
        public int Id { get; set; }
        public string SignedBy { get; set; }
        public DateTime? DateSigned { get; set; }

        [Required(ErrorMessage ="You need to sign the lease to optain the property.")]
        public bool? Signiturre { get; set; }
        public string ApplicationRef { get; set; }
    }
}