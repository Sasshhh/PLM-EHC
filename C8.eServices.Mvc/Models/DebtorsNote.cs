using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class DebtorsNote
    {
        public string AccountNo { get; set; }
        public string Note { get; set; }
        public List<DebtorsNoteList> NoteList { get; set; }
        public List<string> StatusMessages { get; set; }
        public string Status{ get; set; }
    }
}