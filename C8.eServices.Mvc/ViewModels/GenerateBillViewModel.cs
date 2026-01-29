using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.ViewModels
{
    public class GenerateBillViewModel
    {
        public byte[] FileContent { get; set; }
        public string FileName { get; set; }

        public string BillLocation { get; set; }
    }
}