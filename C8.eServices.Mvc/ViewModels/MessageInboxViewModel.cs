using C8.eServices.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.ViewModels
{
    public class MessageInboxViewModel
    {
      

        public List<Comment> Comments { get; set; }
        public List<Communication> Communications { get; set; }
    }
}