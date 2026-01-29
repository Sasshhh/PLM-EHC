using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Keys
{
    public class ViewCodeKeys
    {
        public const int UpdateAgentCustomerProfile = 1;
        public const int RatesRebateThirdPartyCustomerProfile = 2;
        public const int CreateAgentCustomerProfile = 3;
        public const int IndividualCustomerProfile = 4;
        public const int UpdateCustomerLinkedAccount = 5;
        public const int ViewCustomerLinkedAccount = 6;
        public const int UpdateCustomerDocuments = 7;
        public const int LoadRatesRebateProperties = 8;
        public const int UpdateRatesRebateProperty = 9;
        public const int ViewUploadedFile = 10;
        public const int CreateCustomerLinkedAccount = 11;
        public const int ViewRatesRebates = 12;
        public const int ViewRatesRebateProperties = 13;
        public const int DownloadBills = 14;
        public const int UpdateCustomerIncentivePolicyApplication = 15;
        //Navigation Keys
        public const string StepOne = "PrincipaleOwner";
        public const string StepTwo1 = "CreateProperty";
        public const string StepTwo2 = "EditProperty";
        public const string StepThree = "Three";
        public const string StepFour = "Four";
        public const string StepFive = "Five";
        public const string FinalStep = "Final";
        public const string DefaultStep = "Delete";  


        public const string HumanManageOccupants = "HumanManageOccupants";  
        public const string Details = "ApplicationDetails";  
        public const string AgreementOfLeaseTransfer = "AgreementOfLeaseTransfer";  
        public const string PropertyLeaseInspections = "PropertyLeaseInspections";  
        public const string AgreementTermination = "AgreementTermination";  
        public const string PenndingTakeOffConfirmation = "PenndingTakeOffConfirmation";
        public const string CaptureApplicationEviction = "CaptureEviction";
        public const string EvictionCommitteOutcomes = "EvictionOutcomes";
        public const string EvictionCommitteOutcomeList = "EvictionCommitteOutcomeList";
        public const string CaptureEvictionDetails = "CaptureEvictionDetails";

        public const string WaitingQueueEntryView = "WaitingQueueEntryView";
        public const string WaitingQueueEntrySave = "WaitingQueueEntrySave";



        //Do Not Change
        public const string DELETE = "DELETE";
        public const string ON = "ON";
        public const string OFF = "OFF";

        public const string ApprovedCahnge = "ApprovedCahnge";
        public const string DisapprovedCahnge = "DisapprovedCahnge";
        public const string PendingTransfer = "PenndingTransfer";
        public const string PendingUnitMatch = "PendingUnitMatch";

        public const string PreInspection = "PenndingPreInspection";
        public const string PostInspection = "PenndingPostInspection";
        public const string MaintananceJobSheet = "PenndingMaintananceJobSheet";
        public const string ScheduleInspectionSlots = "AwaitingScheduleInspectionSlots";
    }
}