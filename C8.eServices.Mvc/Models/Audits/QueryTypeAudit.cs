using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models.Audits
{
    public class QueryTypeAudit:  BaseModelAudit
    {
        [Column(Order = 10)]
        [MaxLength(100)]
        public string Name { get; set; }

        [Column(Order = 11)]
        [MaxLength(250)]
        public string Description { get; set; }
        [Column(Order = 12)]
        [MaxLength(100)]
        public string Key { get; set; }

        [Column(Order=13)]
        public int Index { get; set; }
    }
}