using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
  
    public class Wso2Cache
    {
        public Int64 Id { get; set; }
        public Int64 ReferenceId { get; set; }
        public string CacheText { get; set; }
        public bool Processed { get; set; }
        public DateTime ModifiedDateTime { get; set; }
    }
}