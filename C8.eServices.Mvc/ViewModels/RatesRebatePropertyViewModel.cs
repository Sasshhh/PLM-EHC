using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.ViewModels
{
    public class RatesRebatePropertyViewModel
    {
        public int RatesRebatePropertyId { get; set; }        
        public RatesRebateProperty RatesRebateProperty { get; set; }
        public Property Property { get; set; }
        public List<AdditionalPropertyOwner> AdditionalOwners { get; set; }
        public List <PropertyAccount> PropertyAccounts { get; set; }
        public DocumentsViewModel DocumentVm { get; set; }

    }

    public class NotesViewModel
    {
        public int Id { get; set; }
        public string Note { get; set; }

        public string NoteType { get; set; }
        public string CreatedDate { get; set; }

        public string SystemUser { get; set; }
    }

    public class PropDdl
    {
        public int RatesRebatePropertyId { get; set; }
        public string PropertyDetails { get; set; }
    }

    public class AccountDetailsViewModel
    {
        public PropertyAccount PropertyAccount { get; set; }

        public DocumentsViewModel PropertyAccountDocuments { get; set; }
    }


    public class HistoryViewModel
    {
        public string CreatedDate { get; set; }

        public string ModifiedDate { get; set; }

        public string CreatedBySystemUser { get; set; }

        public string ModifiedBySystemUser { get; set; }

        public string Type { get; set; }

        public string ClerkName { get; set; }

        public string Content { get; set; }
    }
}