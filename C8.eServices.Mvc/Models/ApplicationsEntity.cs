using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class ApplicationsEntity
    {
        [Column(Order = 1)]
        [Display(Name = "Id")]
        public int Id { get; set; }

        [Required]
        [Column(Order = 2)]
        [Display(Name = "Name")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Column(Order = 3)]
        [Display(Name = "Description")]
        [MaxLength(500)]
        public string Description { get; set; }

        [Column(Order = 4)]
        [Display(Name = "Key")]
        [MaxLength(100)]
        public string Key { get; set; }
    }
}