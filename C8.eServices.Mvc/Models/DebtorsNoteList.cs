using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class DebtorsNoteList
    {
        public string NoteDate { get; set; }
        public DateTime ConvertedDate { get; set; }
        public int NoteTime { get; set; }
        public string Note1 { get; set; }
        public string Note2 { get; set; }
        public string NoteType { get; set; }
        public string NoteUser { get; set; }

    }
}