using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class QueryType : BaseType
    {
        [Column( Order = 20 )]
        [Display( Name = "Order" )]
        public int Index { get; set; }
    }
}