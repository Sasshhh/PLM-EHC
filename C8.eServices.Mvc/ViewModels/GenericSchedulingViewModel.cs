using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace C8.eServices.Mvc.ViewModels
{
    public class GenericSchedulingViewModel
    {
        public int ReferenceId { get; set; }
        public string ReferenceType { get; set; } // e.g. "Complaint" or "PropertyLease"
        
        // Display Details
        public string ReferenceNumber { get; set; }
        public string TargetName { get; set; } // Name of Respondent or Applicant
        public string HeaderTitle { get; set; }
        
        // Form Binding
        public DateTime? DateToSchedule { get; set; }
        public IEnumerable<SelectListItem> AvailableTimeSlots { get; set; }
        
        // Existing Data
        public List<Models.InspectionSchedule> ExistingSchedules { get; set; }

        public GenericSchedulingViewModel()
        {
            ExistingSchedules = new List<Models.InspectionSchedule>();
        }
    }
}
