using C8.eServices.Mvc.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.ViewModels
{
    public class MasterpassPaymentDTO
    {
            public string Account { get; set; }
            public string Name { get; set; }
            public string CustomerID { get; set; }
            public string Email { get; set; }
            public string Number { get; set; }
            public DateTime CreatedDateTime { get; set; }
            public string Code { get; set; }
            public decimal Amount { get; set; }
            public string Status { get; set; }
        
        }
    }