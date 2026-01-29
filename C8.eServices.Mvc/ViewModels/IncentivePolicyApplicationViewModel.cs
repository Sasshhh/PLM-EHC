using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using C8.eServices.Mvc.Controllers;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.ViewModels
{
   public class IncentivePolicyApplicationViewModel
    {
        public Property Property { get; set; } 
        public string IndustrySector { get; set; } 
        public string PropertyFullAddress { get; set; }
        public string PropertyRatesNumber { get; set; }
        public int ApplicantCustomerId { get; set; }
        public int IncentivePolicyId { get; set; }
        public IncentivePolicy IncentivePolicy { get; set; }
        public CustomerProfileViewModel Customer { get; set; }
        public PropertyInvestment PropertyInvestment { get; set; }
        public PropertyInvestmentMeasurement PropertyInvestmentMeasurement { get; set; }
        //Investment Value
        public decimal CapitalExpenditureYear1 { get; set; }
        public decimal? CapitalExpenditureYear2 { get; set; }
        public decimal? CapitalExpenditureYear3 { get; set; }
        public decimal? CapitalExpenditureYear4 { get; set; }
        public decimal? CapitalExpenditureYear5 { get; set; }
        public decimal CapitalExpenditureTotal { get; set; }

        public decimal PermanentJobsYear1 { get; set; }
        public decimal? PermanentJobsYear2 { get; set; }
        public decimal? PermanentJobsYear3 { get; set; }
        public decimal? PermanentJobsYear4 { get; set; }
        public decimal? PermanentJobsYear5 { get; set; }
        public decimal PermanentJobsTotal { get; set; }

        public decimal TempJobsYear1 { get; set; }
        public decimal? TempJobsYear2 { get; set; }
        public decimal? TempJobsYear3 { get; set; }
        public decimal? TempJobsYear4 { get; set; }
        public decimal? TempJobsYear5 { get; set; }
        public decimal TempJobsTotal { get; set; }
        //Measurements
        public int LandSize { get; set; }
        public int LandSizeUnitofMeasure { get; set; }
        public string LandSizeUnitofMeasureDisplay { get; set; }

        public int BuildingSize { get; set; }
        public int BuildingSizeUnitofMeasure { get; set; } 
        public string BuildingSizeUnitofMeasureDisplay { get; set; }

        public decimal OperationalExpanditureForeign { get; set; }
        public decimal OperationalExpanditureOutBound { get; set; }  
        public decimal OperationalExpanditureInBound { get; set; }
        public int SelectedFocusAreaId { get; set; }
        public int SelectedSubSectorId { get; set; }
        public DocumentsViewModel DocumentsViewModel { get; set; }
        public List<AdditionalPropertyContact> AdditionalPropertyContacts { get; set; }
        [Display(Name = "Notes")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [Display(Name="Statistics")]
        public ReportController.IncentivePolicyStats IncentivePolicyStats { get; set; }
    }
}
