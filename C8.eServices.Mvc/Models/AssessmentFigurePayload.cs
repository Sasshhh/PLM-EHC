using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class AssessmentFigurePayload
    {
       
            public string AccountNumber { get; set; }
            public string OutstandingAmount { get; set; }
        public decimal FormattedOutstandingAmount { get; set; }
        public string BalanceDueDate { get; set; }
            public string AssessmentFigureExpiryDate { get; set; }
        public DateTime FormattedAssessmentFigureEndDate { get; set; }
        public DateTime FormattedAssessmentFigureExpiryDate { get; set; }
        public DateTime date { get; set; }
        public string VerifyAccountBillingCycle { get; set; }
            public string BillingCycleNumber { get; set; }
            public string BillingCycleChange { get; set; }
            public string CalculationMonths { get; set; }
        public string AccountBalance { get; set; }
        public string DtCrFlag { get; set; }
        public string PaymentBy { get; set; }
        public List<string> StatusMessages { get; set; }
        public string Status { get; set; }
    }
}