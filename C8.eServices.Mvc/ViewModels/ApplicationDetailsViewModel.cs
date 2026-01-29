using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.ViewModels
{
    public class ApplicationDetailsViewModel
    {
        public TitleType TitleType { get; set; }
        public ApplicantJoint ApplicantJoint { get; set; }
        public CompanyType CompanyType { get; set; }
        public IdentificationType IdentificationType { get; set; }
        public List<HoD> hodList { get; set; }
        public List<DepartmentalComments> DepartmentalCommentsList { get; set; }
        public List<PropertyResident> PropertyResidentList { get; set; }
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }
        //public HSLeaseAgreementMaster HSLeaseAgreementMaster { get; set; }
        public HumanSettlementLeaseMaster HumanSettlementLeaseMaster { get; set; }
        public HumanSettlementLeaseDetails HumanSettlementLeaseDetails { get; set; }
        public HumanSettlementApplication HumanSettlementApplication { get; set; }
        public List<PropertyLeaseApplication> PropertyLeaseApplicationList { get; set; }
        public List<HSRenewalAction> RenewalActions { get; set; }
        public HSRenewalAction SingleAction { get; set; }
        public PLMApplicationHistortyLog AuditAction { get; set; }
        public LeaseDetails LeaseDetails { get; set; }
        public List<LeaseDetails> LeaseDetailsList { get; set; }
        public Units Units { get; set; }
        public MatchedUnits MatchedUnits { get; set; }
        public HumanEHCOptions HumanEHCOptions { get; set; }
        public IncomeSource IncomeSource { get; set; }
        public PurchaserType PurchaserType { get; set; }
        public PreferredComplexArea PreferredComplexArea { get; set; }
        public PreferredComplexArea PreferredComplexAreaSecondOpt { get; set; }
        public OccupationType OccupationType { get; set; }
        public EnvisagedUsage EnvisagedUsage { get; set; }
        public PLMApplicationHistortyLog PLMApplicationHistortyLog { get; set; }
        public List<PLMApplicationHistortyLog> PLMApplicationHistortyLogList { get; set; }
        public List<CommitteeOutcome> CommitteeOutcomeList { get; set; }   
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
        public List<MonthlyIncome> MonthlyIncomeList { get; set; }
        public List<MonthlyExpense> MonthlyExpenseList { get; set; }
        public List<WaterMeterInformation> WaterMeterList { get; set; }
        public List<PurchaserInformation> PurchaserList { get; set; }
        public List<RCSApplicationHistoryLog> RCSApplicationHistoryLogs { get; set; }
        public DocumentsViewModel Document { get; set; }
        public DocumentsViewModel DocumentRiskAssessment { get; set; }
        public DocumentsViewModel DocumentUnitInspection { get; set; }
        public DocumentsViewModel DocumentMaintananceJobSheet { get; set; }
        public DocumentsViewModel DocumentTenantLease { get; set; }
        public DocumentsViewModel DocumentTenantAccoutValidation { get; set; }
        public DocumentsViewModel DocumentPropertyEviction { get; set; }
        public DocumentsViewModel DocumentEvictionCommittee { get; set; }
        public DocumentsViewModel DocumentTenantRiskAssessment { get; set; }
        public DocumentsViewModel DocumentExitInspection { get; set; }
        public DocumentsViewModel DocumentLeaseAgreement { get; set; }
        public DocumentsViewModel DocumentDebitOrder { get; set; }
        public Customer HousingSupervisor { get; set; }
        public Customer LettingOfficer { get; set; }
        public ApplicantUnit ApplicantUnit { get; set; }
        public UnitsEkurhuleniHousingCompany UnitsEkurhuleniHousingCompany { get; set; }
        //public DocumentsViewModel DocumentLeaseWarningLetter { get; set; }
        public DepartmentsApprovalViewModel DocumentLeaseWarningLetterDetails { get; set; }
        public ApplicationAllocatedProperty ApplicationAllocatedProperty { get; set; }
    }
}