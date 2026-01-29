using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
namespace C8.eServices.Mvc.Models
{
    public class IncomeSource : BaseModel
    {
        [Column(Order = 11)]
        [Display(Name = "Source of Income")]
        public string SourceOfIncome { get; set; }
    }
}