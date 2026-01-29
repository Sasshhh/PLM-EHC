using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class SystemUserException
    {
        [Column(Order = 1)]
        [Key]
        public int Id { get; set; }

        [Column(Order = 10)]
        public int CustomerId { get; set; }

        [Column(Order = 11)]
        public string CustomerFirstName { get; set; }

        [Column(Order = 12)]
        public string CustomerLastName { get; set; }

        [Column(Order = 13)]
        public string PropertyName { get; set; }

        [Column(Order = 14)]
        public string ErrorMessage { get; set; }

        [Column(Order = 15)]
        public DateTime DateLogged { get; set; }
    }
}