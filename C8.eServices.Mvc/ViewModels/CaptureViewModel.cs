using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.ViewModels
{
    public class CaptureViewModel
    {
        #region Document Upload
        public List<DocumentCheckList> DocumentCheckList { get; set; }
        public List<Document> Documents { get; set; }
        public DocumentsViewModel DocumentsViewModel { get; set; }
        #endregion

        public ApplicantJoint ApplicantJoint { get; set; }
        public CompanyDirectors CompanyDirectors { get; set; }
        public LeaseDetails LeaseDetails { get; set; }
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }
        public HumanSettlementApplication HumanSettlementApplication { get; set; }
        public RiskAssessmentOutcome RiskAssessmentOutcome { get; set; }
        public TransferInformation TransferInformation { get; set; }

        public PurchaserInformation PurchaserInformation { get; set; }

        public MunicipalAccountInformation MunicipalAccountInformation { get; set; }

        public SellerInformation SellerInformation { get; set; }

        public string returnurl { get; set; }

        public WalkInApplicantDetails WalkInApplicant { get; set; }

        public ConveyancingAttorneyDetail ConveyancingAttorneyDetail { get; set; }


        public ElectricityMeterInformation ElectricityMeterInformation { get; set; }

        public WaterMeterInformation WaterMeterInformation { get; set; }

        public List<ElectricityMeterInformation> ElectricityMeterList { get; set; }

        public List<WaterMeterInformation> WaterMeterList { get; set; }

        public List<PurchaserInformation> PurchaserList { get; set; }

        public string WaterSessionList { get; set; }
        public string ElectricitySessionList { get; set; }

        public string vaIDno { get; set; }

        public string spaceUnitID { get; set; }

        public int PreferredUnitID{ get; set; }
        public bool IsConsentEnabled { get; set; }

        //
        public MonthlyIncomeExpenseViewModel MonthlyIncomeExpenses { get; set; }
        public ApplicationAllocatedProperty ApplicationAllocatedProperties { get; set; }
    }
}