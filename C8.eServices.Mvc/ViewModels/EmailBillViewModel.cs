using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.ViewModels
{
    public class EmailBillViewModel
    {
        [DisplayName("Account Number")]
        public string AccountNumber { get; set; }

        [DisplayName( "Start Date" )]
        public DateTime ? StartDateTime { get; set; }

        [DisplayName( "End Date" )]
        public DateTime ? EndDateTime { get; set; }

        [DisplayName( "Email To" )]
        public string EmailTo { get; set; }

        [DisplayName( "Email Cc" )]
        public string EmailCc { get; set; }

        [DisplayName( "Message" )]
        [DataType( DataType.MultilineText )]
        public string Message { get; set; }
    }
}