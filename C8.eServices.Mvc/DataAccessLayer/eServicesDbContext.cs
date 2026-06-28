using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Web;
using C8.eServices.Mvc.Controllers;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.Models.Audits;
using Microsoft.AspNet.Identity.EntityFramework;
using EntityType = C8.eServices.Mvc.Models.EntityType;

namespace C8.eServices.Mvc.DataAccessLayer
{
    public class eServicesDbContext : IdentityDbContext<SystemIdentityUser>
    {
        static eServicesDbContext()
        {
            Database.SetInitializer<eServicesDbContext>(null);
        }

        public static eServicesDbContext Create()
        {
            return new eServicesDbContext();
        }
        public eServicesDbContext()
            : base("eServicesDbContext")
        {
            // TODO: JK.20160801a - Improve database availability check.
            //DbInterception.Add( new DbConextCommandInterceptor() );
        }


        public eServicesDbContext(SystemUser currentSystemUser)
            : base("eServicesDbContext")
        {
            CurrentSystemUser = currentSystemUser;
            // TODO: JK.20160801a - Improve database availability check.
            //DbInterception.Add( new DbConextCommandInterceptor() );
        }

        public SystemUser CurrentSystemUser { get; set; }
        public string CurrentUserName { get; set; }
        public List<Claim> UserClaims { get; set; }


        //HSD tables
        public DbSet<HSUnitTypology> HSUnitTypologies { get; set; }
        public DbSet<HSUnitCategory> HSUnitCategories { get; set; }
        public DbSet<HSIncomeBrackets> HSIncomeBrackets { get; set; }
        public DbSet<HSEscalationYears> HSEscalationYears { get; set; }
        public DbSet<EscalationMaster> EscalationMaster { get; set; }
        public DbSet<HSLeaseAgreementMaster> HSLeaseAgreementMasters { get; set; }
        public DbSet<HSUnitOccupant> HSUnitOccupants { get; set; }
        public DbSet<HSRenewalAction> HSRenewalActions { get; set; }
        public DbSet<OneTimePin> OneTimePins { get; set; }
        public DbSet<WaitingListQueueHuman> WaitingListQueueHumans { get; set; }
        public DbSet<UnitHumanSettlement> UnitHumanSettlements { get; set; }
        public DbSet<WaitingQueueChangeRequest> WaitingQueueChangeRequests { get; set; }
        public DbSet<UnitsHumanSettlement01> UnitsHumanSettlement01s { get; set; }
        //End HSD Tables


        // PLM Classes

        public DbSet<IncomeSource> IncomeSources { get; set; }
        public DbSet<HumanEHCOptions> humanEHCOptions { get; set; }
        public DbSet<EnvisagedUsage> envisagedUsages { get; set; }
        public DbSet<CompanyType> companyTypes { get; set; }
        public DbSet<OccupationType> OccupationTypes { get; set; }
        public DbSet<PreferredComplexArea> PreferredComplexAreas { get; set; }
        public DbSet<MeetingRequest> MeetingRequests { get; set; }
        public DbSet<WaitingListQue> waitingListQues { get; set; }
        public DbSet<Units> Units { get; set; }
        public DbSet<RiskAssessmentOutcome> RiskAssessmentOutcomes { get; set; }
        public DbSet<Outcome> outcomes { get; set; }
        public DbSet<CommitteeName> committeeNames { get; set; }
        public DbSet<CommitteeNameAudit> committeeNameAudits { get; set; }
        public DbSet<CommitteeOutcome> committeeOutcomes { get; set; }
        public DbSet<Recommendation> recommendations { get; set; }
        public DbSet<ApplicantUnit> ApplicantUnits { get; set; }
        public DbSet<MatchedUnits> MatchedUnits { get; set; }
        public DbSet<LeaseDetails> LeaseDetails { get; set; }
        public DbSet<DepartmentsCoE> DepartmentsCoEs { get; set; }
        public DbSet<DepartmentalComments> DepartmentalComments { get; set; }
        public DbSet<DocumentsLease> DocumentsLeases { get; set; }
        public DbSet<REAC> REACs { get; set; }
        public DbSet<HoD> HoDs { get; set; }
        public DbSet<ApplicationDepartment> ApplicationDepart { get; set; }
        public DbSet<PLMApplicationHistortyLog> PLMApplicationHistortyLogs { get; set; }
        public DbSet<HousingType> HousingTypes { get; set; }
        public DbSet<LeaseCaptureSheet> LeaseCaptureSheets { get; set; }
        public DbSet<LeaseCaptureAddressContact> LeaseCaptureAddressContacts { get; set; }
        public DbSet<PropertyResident> PropertyResidents { get; set; }
        public DbSet<LeaseTermination> LeaseTerminations { get; set; }
        public DbSet<CompanyDirectors> CompanyDirectors { get; set; }
        public DbSet<ApplicantJoint> ApplicantJoints { get; set; }
        public DbSet<ConductUnitInspection> conductUnitInspections { get; set; }
        public DbSet<DebitOrderRegistration> DebitOrderRegistrations { get; set; }
        public DbSet<PropertyManager> PropertyManagers { get; set; }
        public DbSet<TimeSlot> TimeSlots { get; set; }
        public DbSet<DateToSchedule> DateToSchedules { get; set; }
        public DbSet<InspectionSchedule> InspectionSchedules { get; set; }
        public DbSet<PropertyLeaseActionComments> propertyLeaseActionComments { get; set; }
        public DbSet<PropertyLeaseRenewalOffer> PropertyLeaseRenewalOffers { get; set; }
        public DbSet<PropertyLeaseAgreementMaster> propertyLeaseAgreementMasters { get; set; }
        public DbSet<UnitsEkurhuleniHousingCompany> UnitsEkurhuleniHousingCompany { get; set; }
        public DbSet<UnitsEkurhuleniHousingCompany2> UnitsEkurhuleniHousingCompany2 { get; set; }
        public DbSet<ScheduledInspection> ScheduledInspections { get; set; }
        public DbSet<TemporaryDisplayModel> TemporaryDisplayModels { get; set; }
        public DbSet<ApplicationEntity> ApplicationEntities { get; set; }
        public DbSet<ApplicationsEntity> ApplicationEntities2 { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<RegionType> RegionTypes { get; set; }
        public DbSet<RegionAudit> RegionAudits { get; set; }
        public DbSet<RegionTypeAudit> RegionTypeAudits { get; set; }
        public DbSet<HumanSettlementApplicationAudit> HumanSettlementApplicationAudits { get; set; }
        public DbSet<HumanSettlementLeaseMaster> HumanSettlementLeaseMasters { get; set; }
        public DbSet<HumanSettlementLeaseDetails> HumanSettlementLeaseDetails { get; set; }
        public DbSet<HumanSettlementApplication> HumanSettlementApplications { get; set; }
        public DbSet<HumanSettlementAgreementMaster> HumanSettlementAgreementMasters { get; set; }
        public DbSet<UserWorkAllocation> UserWorkAllocations { get; set; }
        public DbSet<RE_Application> RE_Applications { get; set; }
        public DbSet<RE_FacilityCategory> RE_FacilityCategories { get; set; }
        public DbSet<RE_Facility> RE_Facilities { get; set; }
        public DbSet<RE_FacilityUnit> RE_FacilityUnits { get; set; }
        public DbSet<RE_DepartmentalComment> RE_DepartmentalComments { get; set; }
        public DbSet<RE_ApplicationAudit> RE_ApplicationAudits { get; set; }
        public DbSet<RE_FacilityAudit> RE_FacilityAudits { get; set; }
        public DbSet<RE_FacilityCategoryAudit> RE_FacilityCategoryAudits { get; set; }
        public DbSet<RE_FacilityUnitAudit> RE_FacilityUnitAudits { get; set; }
        public DbSet<RE_DepartmentalCommentAudit> RE_DepartmentalCommentAudits { get; set; }
        //Audits 
        public DbSet<ScheduledInspectionAudit> ScheduledInspectionAudits { get; set; }
        public DbSet<UnitsEkurhuleniHousingCompanyAudit> UnitsEkurhuleniHousingCompanyAudits { get; set; }
        public DbSet<PropertyLeaseAgreementMasterAudit> PropertyLeaseAgreementMasterAudits { get; set; }
        public DbSet<PropertyLeaseRenewalOfferAudit> PropertyLeaseRenewalOfferAudits { get; set; }
        public DbSet<PropertyLeaseActionCommentsAudit> PropertyLeaseActionCommentsAudits { get; set; }
        public DbSet<InspectionScheduleAudit> InspectionScheduleAudits { get; set; }
        public DbSet<DateToScheduleAudit> DateToScheduleAudits { get; set; }
        public DbSet<TimeSlotAudit> TimeSlotAudits { get; set; }
        public DbSet<PropertyManagerAudit> PropertyManagerAudits { get; set; }
        public DbSet<DebitOrderRegistrationAudit> DebitOrderRegistrationAudits { get; set; }

        //End of Audits 



        //PLM audit classes
        public DbSet<IncomeSourceAudit> IncomeSourceAudits { get; set; }
        public DbSet<HumanEHCOptionsAudit> HumanEHCOptionsAudits { get; set; }
        public DbSet<PreferredComplexAreaAudit> PreferredComplexAreaAudits { get; set; }
        public DbSet<MeetingRequestAudit> MeetingRequestAudits { get; set; }
        public DbSet<WaitingListQueAudit> WaitingListQueAudits { get; set; }
        public DbSet<UnitsAudit> UnitsAudits { get; set; }
        public DbSet<RiskAssessmentOutcomeAudit> RiskAssessmentOutcomeAudits { get; set; }
        public DbSet<ApplicantUnitAudit> ApplicantUnitAudits { get; set; }
        public DbSet<MatchedUnitsAudit> MatchedUnitsAudits { get; set; }
        public DbSet<LeaseDetailsAudit> LeaseDetailsAudits { get; set; }
        public DbSet<DocumentsLeaseAudit> DocumentsLeaseAudits { get; set; }
        public DbSet<PLMApplicationHistortyLogAudit> PLMApplicationHistortyLogAudits { get; set; }
        public DbSet<PropertyResidentAudit> PropertyResidentAudits { get; set; }
        public DbSet<LeaseTerminationAudit> LeaseTerminationAudits { get; set; }
        public DbSet<ApplicantJointAudit> ApplicantJointAudits { get; set; }
        public DbSet<ConductUnitInspectionAudit> ConductUnitInspectionAudits { get; set; }
        public DbSet<AllocatedUnitHistory> AllocatedUnitHistory { get; set; }
        public DbSet<ApplicationAllocatedProperty> ApplicationAllocatedProperty { get; set; }
        public DbSet<AllocatedUnitMaintenanceEHC> allocatedUnitMaintenanceEHCs { get; set; }
        public DbSet<MaintenanceJobCardTask> MaintenanceJobCardTasks { get; set; }
        public DbSet<MaintenanceJobCardSignature> MaintenanceJobCardSignatures { get; set; }

        public DbSet<LeaseReviewComment> LeaseReviewComments { get; set; }

        // Training and Examination Tables
        public DbSet<TenantTraining> TenantTrainings { get; set; }
        public DbSet<TrainingSlide> TrainingSlides { get; set; }
        public DbSet<ExaminationQuestion> ExaminationQuestions { get; set; }
        public DbSet<TenantExamAnswer> TenantExamAnswers { get; set; }

        // Training and Examination Audits
        public DbSet<TenantTrainingAudit> TenantTrainingAudits { get; set; }
        public DbSet<TrainingSlideAudit> TrainingSlideAudits { get; set; }
        public DbSet<ExaminationQuestionAudit> ExaminationQuestionAudits { get; set; }
        public DbSet<TenantExamAnswerAudit> TenantExamAnswerAudits { get; set; }

        // Complaints Module Tables
        public DbSet<TenantComplaint> TenantComplaints { get; set; }
        public DbSet<ComplaintCategory> ComplaintCategories { get; set; }
        public DbSet<ComplaintType> ComplaintTypes { get; set; }
        public DbSet<ComplaintEvidence> ComplaintEvidences { get; set; }
        public DbSet<ComplaintInvestigation> ComplaintInvestigations { get; set; }
        public DbSet<ComplaintExternalReferral> ComplaintExternalReferrals { get; set; }
        public DbSet<ComplaintInvestigationDocument> ComplaintInvestigationDocuments { get; set; }
        public DbSet<ComplaintAuditLog> ComplaintAuditLogs { get; set; }

        // Payment Transgressions Module Tables (UC17C)
        public DbSet<PaymentTransgression> PaymentTransgressions { get; set; }
        public DbSet<PaymentTransgressionCategory> PaymentTransgressionCategories { get; set; }
        public DbSet<PaymentTransgressionType> PaymentTransgressionTypes { get; set; }
        public DbSet<PaymentTransgressionSeverity> PaymentTransgressionSeverities { get; set; }
        public DbSet<PaymentTransgressionDocument> PaymentTransgressionDocuments { get; set; }
        public DbSet<PaymentTransgressionAuditLog> PaymentTransgressionAuditLogs { get; set; }

        // Service Requests Module Tables (UC17D/UC17E)
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<ServiceRequestCategory> ServiceRequestCategories { get; set; }
        public DbSet<ServiceRequestPriority> ServiceRequestPriorities { get; set; }
        public DbSet<ServiceRequestDocument> ServiceRequestDocuments { get; set; }
        public DbSet<ServiceRequestAuditLog> ServiceRequestAuditLogs { get; set; }

        // UC025 — Serve Eviction Notice & Proof of Service
        public DbSet<EvictionServiceRecord> EvictionServiceRecords { get; set; }

        // UC026 — Manage Disputes
        public DbSet<LeaseDispute> LeaseDisputes { get; set; }

        // Entity Collections.
        public DbSet<Account> Accounts { get; set; }
        public DbSet<AccountInformation> AccountInformations { get; set; }
        public DbSet<ActivityTrackerMessage> ActivityTrackerMessages { get; set; }
        public DbSet<ActOnBehalfType> ActOnBehalfTypes { get; set; }
        public DbSet<AccountType> AccountTypes { get; set; }
        //public DbSet<AdditionalPropertyContact> AdditionalPropertyContacts { get; set; }
        //public DbSet<AdditionalPropertyOwner> AdditionalPropertyOwners { get; set; }
        public DbSet<Agent> Agents { get; set; }
        public DbSet<AgentCustomer> AgentCustomers { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<ApplicationRole> ApplicationRoles { get; set; }
        public DbSet<ApplicationUserRole> ApplicationUserRoles { get; set; }
        public DbSet<AppUserRole> AppUserRoles { get; set; }
        public DbSet<AppSetting> AppSettings { get; set; }

        public DbSet<AssessmentPaymentRequest> AssessmentPaymentRequests { get; set; }
        public DbSet<AssessmentPaymentTransaction> AssessmentPaymentTransactions { get; set; }
        public DbSet<Audit> Audits { get; set; }
        public DbSet<Bank> Banks { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<BankAccountType> BankAccountTypes { get; set; }
        public DbSet<BankCheck> BankChecks { get; set; }
        public DbSet<BillDirectory> BillDirectories { get; set; }
        public DbSet<CategoryType> CategoryTypes { get; set; }
        public DbSet<CCC> CCCs { get; set; }
        public DbSet<CCCType> CCCTypes { get; set; }

        public DbSet<ClerkRegistration> ClerkRegistrations { get; set; }
        public DbSet<ClerkRole> ClerkRoles { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Communication> Communications { get; set; }
        public DbSet<CommunicationType> CommunicationTypes { get; set; }
        public DbSet<CommunicationTypeAudit> CommunicationTypeAudits { get; set; }
        public DbSet<CommunicationAudit> CommunicationAudits { get; set; }
        public DbSet<Country> Countries { get; set; }

        public DbSet<ConveyancingAttorneyDetail> ConveyancingAttorneyDetails { get; set; }
        public DbSet<ConveyancingAttorneyDetailAudit> ConveyancingAttorneyDetailAudits { get; set; }

        public DbSet<Customer> Customers { get; set; }
        // eKurhuleni Siyakhokha Customer Accounts Data Dump.
        public DbSet<CustomerAccount> CustomerAccounts { get; set; }
        public DbSet<CustomerType> CustomerTypes { get; set; }
        //public DbSet<DebitOrder> DebitOrders { get; set; }
        //public DbSet<DebitOrderStatus> DebitOrderStatuses { get; set; }
        public DbSet<DepartmentsApproval> DepartmentsApprovals { get; set; }
        public DbSet<DepartmentsApprovalAudit> DepartmentsApprovalAudits { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentCheckList> DocumentCheckLists { get; set; }
        public DbSet<DocumentReference> DocumentReferences { get; set; }
        public DbSet<DocumentType> DocumentTypes { get; set; }
        //public DbSet<Domicilium> Domiciliums { get; set; }
        public DbSet<EmailTrail> EmailTrails { get; set; }
        public DbSet<EmisCase> EmisCases { get; set; }

        public DbSet<EmailContentType> EmailContentTypes { get; set; }

        public DbSet<EmailContentTypeAudit> EmailContentTypeAudits { get; set; }
        public DbSet<ElectricityMeterInformation> ElectricityMeterInformations { get; set; }
        public DbSet<ElectricityMeterInformationAudit> ElectricityMeterInformationAudits { get; set; }
        public DbSet<Entity> Entities { get; set; }
        public DbSet<EntityAgent> EntityAgents { get; set; }
        public DbSet<EntityType> EntityTypes { get; set; }
        //public DbSet<Executor> Executors { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<Flag> Flags { get; set; }
        public DbSet<FlagAudit> FlagAudits { get; set; }
        // RMS Integration Collections.
        //public DbSet<FocusArea> FocusAreas { get; set; }
        public DbSet<IdentificationType> IdentificationTypes { get; set; }
        //public DbSet<IncentivePolicy> IncentivePolicies { get; set; }
        //public DbSet<IncentivePolicyProperty> IncentivePolicyProperties { get; set; }
        //public DbSet<Industry> Industries { get; set; }
        public DbSet<InstantEFT> InstantEFTs { get; set; }
        public DbSet<InstantEFTTransaction> InstantEFTTransactions { get; set; }

        public DbSet<InstructionContent> InstructionContents { get; set; }
        public DbSet<InstructionContentAudit> InstructionContentAudits { get; set; }

        //public DbSet<InvestmentOperationalExpenditure> InvestmentOperationalExpenditures { get; set; }
        //public DbSet<InvestmentValue> InvestmentValues { get; set; }
        //public DbSet<InvestmentValueType> InvestmentValueTypes { get; set; }
        public DbSet<LinkedAccount> LinkedAccounts { get; set; }
        public DbSet<LinkedAccountType> LinkedAccountTypes { get; set; }
        public DbSet<LinkedEmail> LinkedEmails { get; set; }
        public DbSet<LinkedMobile> LinkedMobiles { get; set; }
        public DbSet<LocationType> LocationTypes { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<RoundRobinLog> RoundRobinLogs { get; set; }
        public DbSet<LogType> LogTypes { get; set; }
        public DbSet<MasterpassRequest> MasterpassRequests { get; set; }
        public DbSet<MasterpassTransaction> MasterpassTransactions { get; set; }

        public DbSet<MunicipalAccountInformation> MunicipalAccountInformations { get; set; }

        public DbSet<MunicipalAccountInformationAudit> MunicipalAccountInformationAudits { get; set; }
        //public DbSet<MeasurementType> MeasurementTypes { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<NoteType> NoteTypes { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationSubscription> NotificationSubscriptions { get; set; }
        public DbSet<Wso2Cache> Wso2Caches { get; set; }
        public DbSet<NotificationType> NotificationTypes { get; set; }
        public DbSet<PaymentHistory> PaymentHistories { get; set; }
        //public DbSet<Property> Properties { get; set; }
        //public DbSet<PropertyAccount> PropertyAccounts { get; set; }
        //public DbSet<PropertyInvestment> PropertyInvestments { get; set; }
        //public DbSet<PropertyInvestmentMeasurement> PropertyInvestmentMeasurements { get; set; }
        //public DbSet<PropertyInvestmentValue> PropertyInvestmentValues { get; set; }

        public DbSet<PurchaserInformation> PurchaserInformations { get; set; }
        public DbSet<PurchaserInformationAudit> PurchaserInformationAudits { get; set; }

        public DbSet<Query> Queries { get; set; }
        public DbSet<QueryType> QueryTypes { get; set; }
        //public DbSet<RatesRebate> RatesRebates { get; set; }
        //public DbSet<RatesRebateProperty> RatesRebateProperties { get; set; }
        public DbSet<RCSUserMessage> RCSUserMessages { get; set; }
        public DbSet<RCSApplicationHistoryLog> RCSApplicationHistoryLogs { get; set; }
        public DbSet<RCSApplicationHistoryLogAudit> RCSApplicationHistoryLogAudits { get; set; }
        public DbSet<RecipientType> RecipientTypes { get; set; }
        public DbSet<ReportSetting> ReportSettings { get; set; }
        public DbSet<ReportSettingAudit> ReportSettingAudits { get; set; }
        public DbSet<ReferenceType> ReferenceTypes { get; set; }

        public DbSet<RefundApplication> RefundApplications { get; set; }
        public DbSet<RefundApplicationAudit> RefundApplicationAudits { get; set; }
        public DbSet<ResponsibilityType> ResponsibilityTypes { get; set; }
        public DbSet<ResponsibilityTypeAudit> ResponsibilityTypeAudits { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<RoundRobinQueue> RoundRobinQueues { get; set; }
        public DbSet<RoundRobinQueueAudit> RoundRobinQueueAudits { get; set; }
        //public DbSet<RmsTemporaryRatesRebate> RmsTemporaryRatesRebates { get; set; }
        public DbSet<Status> Status { get; set; }
        public DbSet<StatusType> StatusTypes { get; set; }
        //public DbSet<SubSector> SubSectors { get; set; }
        public DbSet<Support> SupportQueries { get; set; }
        public DbSet<SystemUser> SystemUsers { get; set; }
        public DbSet<SystemUserException> SystemUserException { get; set; }
        public DbSet<SystemUserLogTime> SystemUserLogTimes { get; set; }
        public DbSet<SystemUserType> SystemUserTypes { get; set; }
        public DbSet<TitleType> TitleTypes { get; set; }

        public  DbSet<TransferInformation> TransferInformations { get; set; }

        public DbSet<TransferType> TransferTypes { get; set; }

        public DbSet<TransferInformationAudit> TransferInformationAudits { get; set; }
        //public DbSet<UnitOfmeasureType> UnitOfmeasureTypes { get; set; }

        //TODO: Include on eServices Db in future
        //public DbSet<Correspondance> Correspondances { get; set; } 
        // Audit Collections.
        public DbSet<AccountAudit> AccountAudits { get; set; }

        public DbSet<ActOnBehalfTypeAudit> ActOnBehalfTypeAudits { get; set; }
        public DbSet<AccountTypeAudit> AccountTypeAudits { get; set; }
        //public DbSet<AdditionalPropertyContactAudit> AdditionalPropertyContactAudits { get; set; }
        //public DbSet<AdditionalPropertyOwnerAudit> AdditionalPropertyOwnerAudits { get; set; }
        public DbSet<AgentAudit> AgentAudits { get; set; }
        public DbSet<AgentCustomerAudit> AgentCustomerAudits { get; set; }
        public DbSet<ApplicationAudit> ApplicationAudits { get; set; }
        public DbSet<ApplicationRoleAudit> ApplicationRoleAudits { get; set; }
        public DbSet<ApplicationUserRoleAudit> ApplicationUserRoleAudits { get; set; }
        public DbSet<AppUserRoleAudit> AppUserRoleAudits { get; set; }
        public DbSet<AppSettingAudit> AppSettingAudits { get; set; }

        public DbSet<AssessmentPaymentRequestAudit> AssessmentPaymentRequestAudits { get; set; }
        public DbSet<BankAccountAudit> BankAccountAudits { get; set; }
        public DbSet<BankAudit> BankAudits { get; set; }
        public DbSet<BankCheckAudit> BankCheckAudits { get; set; }
        public DbSet<CategoryTypeAudit> CategoryTypeAudits { get; set; }
        public DbSet<ClerkRegistrationAudit> ClerkRegistrationAudits { get; set; }
        public DbSet<ClerkRoleAudit> ClerkRoleAudits { get; set; }
        public DbSet<CommentAudit> CommentAudits { get; set; }
        public DbSet<CountryAudit> CountryAudits { get; set; }
        public DbSet<CustomerAudit> CustomerAudits { get; set; }
        public DbSet<CustomerTypeAudit> CustomerTypeAudits { get; set; }

        public DbSet<CCCAudit> CCCAudits { get; set; }
        public DbSet<CCCTypeAudit> CCCTypeAudits { get; set; }
        //public DbSet<DebitOrderAudit> DebitOrderAudits { get; set; }
        //public DbSet<DebitOrderStatusAudit> DebitOrderStatusAudits { get; set; }
        public DbSet<DocumentAudit> DocumentAudits { get; set; }
        public DbSet<DocumentCheckListAudit> DocumentCheckListAudits { get; set; }
        public DbSet<DocumentReferenceAudit> DocumentReferenceAudits { get; set; }
        public DbSet<DocumentTypeAudit> DocumentTypeAudits { get; set; }
        //public DbSet<DomiciliumAudit> DomiciliumAudits { get; set; }
        public DbSet<EntityAgentAudit> EntityAgentAudits { get; set; }
        public DbSet<EntityAudit> EntityAudits { get; set; }
        public DbSet<EntityTypeAudit> EntityTypeAudits { get; set; }
        //public DbSet<ExecutorAudit> ExecutorAudits { get; set; }
        //public DbSet<FocusAreaAudit> FocusAreaAudits { get; set; }
        public DbSet<IdentificationTypeAudit> IdentificationTypeAudits { get; set; }
        //public DbSet<IncentivePolicyAudit> IncentivePolicyAudits { get; set; }
        //public DbSet<IncentivePolicyPropertyAudit> IncentivePolicyPropertyAudits { get; set; }
        //public DbSet<IndustryAudit> IndustryAudits { get; set; }
        public DbSet<InstantEFTAudit> InstantEFTAudits { get; set; }
        public DbSet<InstantEFTTransactionAudit> InstantEFTTransactionAudits { get; set; }
        //public DbSet<InvestmentOperationalExpenditureAudit> InvestmentOperationalExpenditureAudits { get; set; }
        //public DbSet<InvestmentValueAudit> InvestmentValueAudits { get; set; }
        //public DbSet<InvestmentValueTypeAudit> InvestmentValueTypeAudits { get; set; }
        public DbSet<LinkedAccountAudit> LinkedAccountAudits { get; set; }
        public DbSet<LinkedAccountTypeAudit> LinkedAccountTypeAudits { get; set; }
        public DbSet<LinkedEmailAudit> LinkedEmailAudits { get; set; }
        public DbSet<LocationTypeAudit> LocationTypeAudits { get; set; }
        public DbSet<LogAudit> LogAudits { get; set; }

        public DbSet<RoundRobinLogAudit> RoundRobinLogAudits { get; set; }
        public DbSet<LogTypeAudit> LogTypeAudits { get; set; }
        //public DbSet<MeasurementTypeAudit> MeasurementTypeAudits { get; set; }
        public DbSet<NoteAudit> NoteAudits { get; set; }
        public DbSet<NoteTypeAudit> NoteTypeAudits { get; set; }
        public DbSet<NotificationAudit> NotificationAudits { get; set; }
        public DbSet<NotificationSubscriptionAudit> NotificationSubscriptionAudits { get; set; }
        public DbSet<NotificationTypeAudit> NotificationTypeAudits { get; set; }
        //public DbSet<PropertyAccountAudit> PropertyAccountAudits { get; set; }
        //public DbSet<PropertyAudit> PropertyAudits { get; set; }
        //public DbSet<PropertyInvestmentAudit> PropertyInvestmentAudits { get; set; }
        //public DbSet<PropertyInvestmentMeasurementAudit> PropertyInvestmentMeasurementAudits { get; set; }
        //public DbSet<PropertyInvestmentValueAudit> PropertyInvestmentValueAudits { get; set; }
        public DbSet<QueryAudit> QueryAudits { get; set; }
        public DbSet<QueryTypeAudit> QueryTypeAudits { get; set; }
        //public DbSet<RatesRebateAudit> RatesRebateAudits { get; set; }
        //public DbSet<RatesRebatePropertyAudit> RatesRebatePropertyAudits { get; set; }
        public DbSet<RCSApplicationStatus> RCSApplicationStatus { get; set; }
        //public DbSet<RCSApplicationAudit> RCSApplicationAudits { get; set; }
        public DbSet<RCSApplicationStatusAudit> RCSApplicationStatusAudits { get; set; }
        public DbSet<RCSDepartmentType> RCSDepartmentTypes { get; set; }
        public DbSet<RCSActionTypeAudit> RCSActionTypeAudits { get; set; }
        public DbSet<RCSDepartmentTypeAudit> RCSDepartmentTypeAudits { get; set; }
        public DbSet<RCSUserMessageAudit> RCSUserMessageAudits { get; set; }
        public DbSet<RecipientTypeAudit> RecipientTypeAudits { get; set; }
        public DbSet<ReferenceTypeAudit> ReferenceTypeAudits { get; set; }
        public DbSet<RequestAudit> RequestAudits { get; set; }

        public DbSet<PropertyLeaseApplication> PropertyLeaseApplications { get; set; }
        public DbSet<PropertyLeaseApplicationAudit> PropertyLeaseApplicationAudits { get; set; }
        public DbSet<SellerInformation> SellerInformations { get; set; }
        public DbSet<SellerInformationAudit> SellerInformationAudits { get; set; }


        public DbSet<StatusAudit> StatusAudits { get; set; }
        public DbSet<StatusTypeAudit> StatusTypeAudits { get; set; }
        //public DbSet<SubSectorAudit> SubSectorAudits { get; set; }
        public DbSet<SystemUserAudit> SystemUserAudits { get; set; }
        public DbSet<SystemUserTypeAudit> SystemUserTypeAudits { get; set; }
        public DbSet<TitleTypeAudit> TitleTypeAudits { get; set; }
        public DbSet<PurchaserType> PurchaserType { get; set; }
        public DbSet<RCSType> RCSTypes { get; set; }
        public DbSet<RCSTypeAudit> RCSTypeAudits { get; set; }
        public DbSet<TransferTypeAudits> TransferTypeAudits { get; set; }
        public DbSet<RCSActionType> RCSActionTypes { get; set; }

        public DbSet<WaterMeterInformation> WaterMeterInformations { get; set; }

        public DbSet<WaterMeterInformationAudit> WaterMeterInformationAudits { get; set; }
        public DbSet<WalkInApplicantDetails> WalkInApplicantDetails { get; set; }

        public DbSet<WalkInApplicantDetailsAudit> WalkInApplicantDetailsAudits { get; set; }
        public DbSet<Attachments> Attachments { get; set; }

        public DbSet<MonthlyIncome> MonthlyIncomes { get; set; }
        public DbSet<MonthlyIncomeAudit> MonthlyIncomeAudits { get; set; }
        public DbSet<ApplicantType> ApplicantTypes { get; set; }
        public DbSet<ApplicantTypeAudit> ApplicantTypeAudits { get; set; }

        public DbSet<MonthlyExpense> MonthlyExpenses { get; set; }
        public DbSet<MonthlyExpenseAudit> MonthlyExpenseAudits { get; set; }
        public DbSet<PlmManualApplication> PlmManualApplications { get; set; }
        public DbSet<PlmManualDataApplication> PlmManualDataApplications { get; set; }
        public DbSet<PropertyLeaseWaitingList> PropertyLeaseWaitingLists { get; set; }
        public string GetIP()
        {
            string Str = "";
            Str = System.Net.Dns.GetHostName();
            IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(Str);
            IPAddress[] addr = ipEntry.AddressList;
            return addr[addr.Length - 1].ToString();
        }
        public override int SaveChanges()
        {
            var changeCount = 0;
            string entityState = string.Empty;
            PropertyLeaseApplicationController cc = new PropertyLeaseApplicationController();
            try
            {
                var transactionDateTime = DateTime.Now;
                var changeSet = ChangeTracker.Entries();
                int? currentUserId = CurrentSystemUser != null ? CurrentSystemUser.Id : (int?)null;
                string tableName = string.Empty;
                List<AuditEnitity> singleTableAudit = new List<AuditEnitity>();
                List<AuditEnitity> perTableAudits = new List<AuditEnitity>();
                this.Database.CommandTimeout = 180;
                
                foreach (var entry in changeSet.Where(e => e.Entity is IAuditable))
                {
                    Type entryEntityType = entry.Entity.GetType();
                    string primaryKeyName = GetPrimaryKeyProperty(entryEntityType);
                    int primaryKey = Convert.ToInt32(entryEntityType.GetProperty(primaryKeyName).GetValue(entry.Entity));
                    entityState = entry.State.ToString();
                    tableName = GetTableName(entryEntityType);

                    ((IAuditable)entry.Entity).ModifiedBySystemUserId = currentUserId;
                    ((IAuditable)entry.Entity).ModifiedDateTime = transactionDateTime;

                    switch (entry.State)
                    {
                        case EntityState.Added:

                            ((IAuditable)entry.Entity).CreatedBySystemUserId = currentUserId;
                            ((IAuditable)entry.Entity).CreatedDateTime = transactionDateTime;
                            ((BaseModel)entry.Entity).IsActive = true;
                            ((BaseModel)entry.Entity).IsDeleted = false;
                            ((BaseModel)entry.Entity).IsLocked = false;

                            foreach (var propertyName in entry.CurrentValues.PropertyNames)
                            {
                                singleTableAudit.Add(new AuditEnitity()
                                {
                                    Audit = new Audit()
                                    {
                                        Action = entityState,
                                        TableName = tableName,
                                        PrimaryKey = primaryKey,
                                        ColumnName = propertyName,
                                        CurrentValue = Convert.ToString(entry.CurrentValues[propertyName]),
                                        OriginalValue = string.Empty,
                                        AuditBySystemUserId = currentUserId,
                                        AuditDateTime = transactionDateTime,
                                        IPAddress = GetIP()?? HttpContext.Current.Request.UserHostAddress,

                                    },
                                    Entity = entry.Entity
                                });
                            }

                            perTableAudits.Add(new AuditEnitity()
                            {
                                Audit = new Audit()
                                {
                                    Action = entityState
                                },
                                Entity = entry.Entity
                            });

                            break;
                        case EntityState.Modified:
                        case EntityState.Deleted:
                            ((IAuditable)entry.Entity).CreatedBySystemUserId = ((IAuditable)entry.Entity).CreatedBySystemUserId;
                            ((IAuditable) entry.Entity).CreatedDateTime = ((IAuditable) entry.Entity).CreatedDateTime;

                            foreach (var propertyName in entry.CurrentValues.PropertyNames)
                            {
                                if ((entry.CurrentValues[propertyName] != null && entry.GetDatabaseValues()[propertyName] != null) && !entry.CurrentValues[propertyName].Equals(entry.GetDatabaseValues()[propertyName]))
                                    singleTableAudit.Add(new AuditEnitity()
                                    {
                                        Audit = new Audit()
                                        {
                                            Action = entityState,
                                            TableName = tableName,
                                            PrimaryKey = primaryKey,
                                            ColumnName = propertyName,
                                            CurrentValue = Convert.ToString(entry.CurrentValues[propertyName]),
                                            OriginalValue = Convert.ToString(entry.OriginalValues[propertyName]),
                                            AuditBySystemUserId = currentUserId,
                                            AuditDateTime = transactionDateTime,
                                            IPAddress = GetIP()?? HttpContext.Current.Request.UserHostAddress,
                                        },
                                        Entity = entry.Entity
                                    });
                            }

                            perTableAudits.Add(new AuditEnitity()
                            {
                                Audit = new Audit()
                                {
                                    Action = entityState
                                },
                                Entity = entry.Entity
                            });

                            break;
                    }
                }

                // var changeCount = base.SaveChanges();
                changeCount = base.SaveChanges();

                // JK.20140902a - Auditing is processed here.
                foreach (var auditEnitity in singleTableAudit)
                {
                    if (auditEnitity.Entity != null)
                    {
                        Type entryEntityType = auditEnitity.Entity.GetType();
                        string primaryKeyName = GetPrimaryKeyProperty(entryEntityType);
                        int primaryKey =
                            Convert.ToInt32(entryEntityType.GetProperty(primaryKeyName).GetValue(auditEnitity.Entity));

                        auditEnitity.Audit.PrimaryKey = primaryKey;
                    }

                    // Single table audit.
                    Audits.Add(auditEnitity.Audit);
                }

                foreach (var e in perTableAudits)
                {
                    SaveAudit(e);
                }

                //Save audit trail.
                base.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    string entityName = validationErrors.Entry.Entity.GetType().Name;
                    string controller = HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
                    string action = HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");

                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        //Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                        //EventLogHelper.Log(string.Format("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage));
                        SecurityHelper.LogError(new Exception(string.Format("Entity Name: {0} State: {1} Controller: {2} Action: {3} Property: {4} Error: {5}", entityName, entityState,
                            controller, action, validationError.PropertyName, validationError.ErrorMessage)), null);
                    }
                }
            }
            return changeCount;
        }



        static string GetPublicIp(string serviceUrl = "https://ipinfo.io/ip")
        {
            return System.Net.IPAddress.Parse(new System.Net.WebClient().DownloadString(serviceUrl)).ToString();
        }



        /// <summary>
        /// Saves the audits for per table auditing.
        /// </summary>
        /// <param name="entry">The entry.</param>
        private void SaveAudit(AuditEnitity entry)
        {
            try
            {
                var name = entry.Entity.GetType().Name;
                var space = entry.Entity.GetType().Namespace;
                var type = Type.GetType(string.Format("{0}.Audits.{1}Audit", space, name));

                object audit = null;

                if (type != null)
                {
                    audit = Activator.CreateInstance(type);

                    var props =
                        entry.Entity.GetType()
                            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

                    audit.GetType().GetProperty("Action").SetValue(audit, entry.Audit.Action);

                    foreach (var prop in props)
                    {
                        if (prop.CanWrite && prop.CanRead)
                            switch (prop.PropertyType.ToString())
                            {
                                case "System.String":
                                case "System.string":
                                case "System.Int32":
                                case "System.Nullable`1[System.Int32]":
                                case "System.Int64":
                                case "System.bool":
                                case "System.Boolean":
                                case "System.Nullable`1[System.Boolean]":
                                case "System.DateTime":
                                case "System.Nullable`1[System.DateTime]":
                                case "System.Nullable`1[System.Decimal]":
                                //case "System.NullReferenceException":
                                    //LM.20160906a - Handles null properties
                                    if (prop.GetValue(entry.Entity, null) != null)
                                    {
                                        try
                                        {
                                            var auditProp = audit.GetType().GetProperty(prop.Name);
                                            if (auditProp != null)
                                                auditProp.SetValue(audit, prop.GetValue(entry.Entity));
                                        }
                                        catch (Exception)
                                        {

                                           //
                                        }
                                                                             
                                    }
                                    break;
                            }
                    }

                    // JK.20160509a - TODO: Replace with reflection.
                    switch (name)
                    {
                        case "Account":
                            AccountAudits.Add((AccountAudit)audit);
                            break;
                        case "AccountType":
                            AccountTypeAudits.Add((AccountTypeAudit)audit);
                            break;
                        case "ActOnBehalfType":
                            ActOnBehalfTypeAudits.Add((ActOnBehalfTypeAudit)audit);
                            break;
                        case "Agent":
                            AgentAudits.Add((AgentAudit)audit);
                            break;
                        case "AgentCustomer":
                            AgentCustomerAudits.Add((AgentCustomerAudit)audit);
                            break;
                        case "Application":
                            ApplicationAudits.Add((ApplicationAudit)audit);
                            break;
                        case "RE_Application":
                            var reApp = (RE_Application)entry.Entity;
                            var reAudit = (RE_ApplicationAudit)audit;
                            reAudit.Id = 0;
                            reAudit.RE_ApplicationId = reApp.Id;
                            RE_ApplicationAudits.Add(reAudit);
                            break;
                        case "RE_Facility":
                            RE_FacilityAudits.Add((RE_FacilityAudit)audit);
                            break;
                        case "RE_FacilityCategory":
                            RE_FacilityCategoryAudits.Add((RE_FacilityCategoryAudit)audit);
                            break;
                        case "RE_FacilityUnit":
                            RE_FacilityUnitAudits.Add((RE_FacilityUnitAudit)audit);
                            break;
                        case "RE_DepartmentalComment":
                            RE_DepartmentalCommentAudits.Add((RE_DepartmentalCommentAudit)audit);
                            break;
                        case "ApplicationRole":
                            ApplicationRoleAudits.Add((ApplicationRoleAudit)audit);
                            break;
                        case "ApplicationUserRole":
                            ApplicationUserRoleAudits.Add((ApplicationUserRoleAudit)audit);
                            break;
                        case "AppSetting":
                            AppSettingAudits.Add((AppSettingAudit)audit);
                            break;
                        case "AppUserRole":
                            AppUserRoleAudits.Add((AppUserRoleAudit)audit);
                            break;
                        case "AssessmentPaymentRequest":
                            AssessmentPaymentRequestAudits.Add((AssessmentPaymentRequestAudit)audit);
                            break;
                        case "BankAccount":
                            BankAccountAudits.Add((BankAccountAudit)audit);
                            break;
                        case "Bank":
                            BankAudits.Add((BankAudit)audit);
                            break;
                        case "BankCheck":
                            BankCheckAudits.Add((BankCheckAudit)audit);
                            break;
                        case "CategoryTypeAudit":
                            CategoryTypeAudits.Add((CategoryTypeAudit) audit);
                            break;
                        case "CCC":
                            CCCAudits.Add((CCCAudit)audit);
                            break;
                        case "CCCType":
                            CCCTypeAudits.Add((CCCTypeAudit)audit);
                            break;
                        case "ClerkRegistration":
                            ClerkRegistrationAudits.Add((ClerkRegistrationAudit)audit);
                            break;
                        case "ClerkRole":
                            ClerkRoleAudits.Add((ClerkRoleAudit)audit);
                            break;
                        case "Comment":
                            CommentAudits.Add((CommentAudit)audit);
                            break;
                        case "Communications":
                            CommunicationAudits.Add((CommunicationAudit)audit);
                            break;
                        case "CommunicationType":
                            CommunicationTypeAudits.Add((CommunicationTypeAudit)audit);
                            break;
                        case "ConveyancingAttorneyDetail":
                            ConveyancingAttorneyDetailAudits.Add((ConveyancingAttorneyDetailAudit)audit);
                            break;
                        case "Country":
                            CountryAudits.Add((CountryAudit)audit);
                            break;
                        case "Customer":
                            CustomerAudits.Add((CustomerAudit)audit); 
                            break;
                        case "CustomerType":
                            CustomerTypeAudits.Add((CustomerTypeAudit)audit);
                            break;
                        case "DepartmentApproval":
                            DepartmentsApprovalAudits.Add((DepartmentsApprovalAudit)audit);
                            break;
                        case "Document":
                            DocumentAudits.Add((DocumentAudit)audit);
                            break;
                        case "DocumentType":
                            DocumentTypeAudits.Add((DocumentTypeAudit)audit);
                            break;
                        case "DocumentCheckList":
                            DocumentCheckListAudits.Add((DocumentCheckListAudit)audit);
                            break;
                        case "DocumentReference":
                            DocumentReferenceAudits.Add((DocumentReferenceAudit)audit);
                            break;
                        case "ElectricityMeterInformation":
                            ElectricityMeterInformationAudits.Add((ElectricityMeterInformationAudit)audit);
                            break;
                        case "Entity":
                            EntityAudits.Add((EntityAudit)audit);
                            break;
                        case "EntityType":
                            EntityTypeAudits.Add((EntityTypeAudit)audit);
                            break;
                        case "EmailContentType":
                            EmailContentTypeAudits.Add((EmailContentTypeAudit)audit);
                            break;
                        case "Flag":
                            FlagAudits.Add((FlagAudit)audit);
                            break;
                        case "IdentificationType":
                            IdentificationTypeAudits.Add((IdentificationTypeAudit)audit);
                            break;
                        case "InstantEFT":
                            InstantEFTAudits.Add((InstantEFTAudit)audit);
                            break;
                        case "InstantEFTTransaction":
                            InstantEFTTransactionAudits.Add((InstantEFTTransactionAudit)audit);
                            break;
                        case "LinkedAccount":
                            LinkedAccountAudits.Add((LinkedAccountAudit)audit);
                            break;
                        case "LinkedAccountType":
                            LinkedAccountTypeAudits.Add((LinkedAccountTypeAudit)audit);
                            break;
                        case "LinkedEmail":
                            LinkedEmailAudits.Add((LinkedEmailAudit)audit);
                            break;
                        case "LocationType":
                            LocationTypeAudits.Add((LocationTypeAudit)audit);
                            break;
                        case "Log":
                            LogAudits.Add((LogAudit)audit);
                            break;
                        case "LogType":
                            LogTypeAudits.Add((LogTypeAudit)audit);
                            break;
                        case "MunicipalAccountInformation":
                            MunicipalAccountInformationAudits.Add((MunicipalAccountInformationAudit)audit);
                            break;
                        case "Notification":
                            NotificationAudits.Add((NotificationAudit)audit);
                            break;
                        case "NotificationType":
                            NotificationTypeAudits.Add((NotificationTypeAudit)audit);
                            break;
                        case "NotificationSubscription":
                            NotificationSubscriptionAudits.Add((NotificationSubscriptionAudit)audit);
                            break;
                        case "Note":
                            NoteAudits.Add((NoteAudit)audit);
                            break;
                        case "NoteType":
                            NoteTypeAudits.Add((NoteTypeAudit)audit);
                            break;
                        case "PropertyLeaseApplication":
                            PropertyLeaseApplicationAudits.Add((PropertyLeaseApplicationAudit)audit);
                            break;
                        case "RecipientType":
                            RecipientTypeAudits.Add((RecipientTypeAudit)audit);
                            break;
                        case "ReferenceType":
                            ReferenceTypeAudits.Add((ReferenceTypeAudit)audit);
                            break;
                        case "RefundApplication":
                            RefundApplicationAudits.Add((RefundApplicationAudit)audit);
                            break;
                        case "ReportSetting":
                            ReportSettingAudits.Add((ReportSettingAudit)audit);
                            break;
                        case "Request":
                            RequestAudits.Add((RequestAudit)audit);
                            break;
                        case "ResponsibilityType":
                            ResponsibilityTypeAudits.Add((ResponsibilityTypeAudit)audit);
                            break;
                        case "RCSDepartmentType":
                            RCSDepartmentTypeAudits.Add((RCSDepartmentTypeAudit)audit);
                            break;
                        case "RCSType":
                            RCSTypeAudits.Add((RCSTypeAudit)audit);
                            break;
                        case "RCSActionType":
                            RCSActionTypeAudits.Add((RCSActionTypeAudit)audit);
                            break;
                        case "RCSUserMessage":
                            RCSUserMessageAudits.Add((RCSUserMessageAudit)audit);
                            break;

                        case "RCSApplicationStatus":
                            RCSApplicationStatusAudits.Add((RCSApplicationStatusAudit)audit);
                            break;
                        case "RoundRobinLog":
                            RoundRobinLogAudits.Add((RoundRobinLogAudit)audit);
                            break;
                        case "RoundRobinQueue":
                            RoundRobinQueueAudits.Add((RoundRobinQueueAudit)audit);
                            break;

                        case "PurchaserInformation":
                            PurchaserInformationAudits.Add((PurchaserInformationAudit)audit);
                            break;
                        case "SellerInformation":
                            SellerInformationAudits.Add((SellerInformationAudit)audit);
                            break;
                        case "Status":
                            StatusAudits.Add((StatusAudit)audit);
                            break;
                        case "StatusType":
                            StatusTypeAudits.Add((StatusTypeAudit)audit);
                            break;
                        case "SystemUser":
                            SystemUserAudits.Add((SystemUserAudit)audit);
                            break;
                        case "SystemUserType":
                            SystemUserTypeAudits.Add((SystemUserTypeAudit)audit);
                            break;
                        case "TitleType":
                            TitleTypeAudits.Add((TitleTypeAudit)audit);
                            break;
                        case "TransferInformation":
                            TransferInformationAudits.Add((TransferInformationAudit)audit);
                            break;
                        case "TransferType":
                            TransferTypeAudits.Add((TransferTypeAudits)audit);
                            break;
                        case "Query":
                            QueryAudits.Add((QueryAudit)audit);
                            break;
                        case "WalkInApplicantDetails":
                            WalkInApplicantDetailsAudits.Add((WalkInApplicantDetailsAudit)audit);
                            break;
                        case "WaterMeterInformation":
                            WaterMeterInformationAudits.Add((WaterMeterInformationAudit)audit);
                            break;
                        case "IncomeSource":
                            IncomeSourceAudits.Add((IncomeSourceAudit)audit);
                            break;
                        case "HumanEHCOptions":
                            HumanEHCOptionsAudits.Add((HumanEHCOptionsAudit)audit);
                            break;
                        case "PreferredComplexArea":
                            PreferredComplexAreaAudits.Add((PreferredComplexAreaAudit)audit);
                            break;
                        case "MeetingRequest":
                            MeetingRequestAudits.Add((MeetingRequestAudit)audit);
                            break;
                        case "WaitingListQue":
                            WaitingListQueAudits.Add((WaitingListQueAudit)audit);
                            break;
                        case "Units":
                            UnitsAudits.Add((UnitsAudit)audit);
                            break;
                        case "RiskAssessmentOutcome":
                            RiskAssessmentOutcomeAudits.Add((RiskAssessmentOutcomeAudit)audit);
                            break;
                        case "ApplicantUnit":
                            ApplicantUnitAudits.Add((ApplicantUnitAudit)audit);
                            break;
                        case "MatchedUnits":
                            MatchedUnitsAudits.Add((MatchedUnitsAudit)audit);
                            break;
                        case "LeaseDetails":
                            LeaseDetailsAudits.Add((LeaseDetailsAudit)audit);
                            break;
                        case "DocumentsLease":
                            DocumentsLeaseAudits.Add((DocumentsLeaseAudit)audit);
                            break;
                        case "PLMApplicationHistortyLog":
                            PLMApplicationHistortyLogAudits.Add((PLMApplicationHistortyLogAudit)audit);
                            break;
                        case "PropertyResident":
                            PropertyResidentAudits.Add((PropertyResidentAudit)audit);
                            break;
                        case "LeaseTermination":
                            LeaseTerminationAudits.Add((LeaseTerminationAudit)audit);
                            break;
                        case "ApplicantJoint":
                            ApplicantJointAudits.Add((ApplicantJointAudit)audit);
                            break;
                        case "ConductUnitInspection":
                            ConductUnitInspectionAudits.Add((ConductUnitInspectionAudit)audit);
                            break;
                        case "ScheduledInspection":
                            ScheduledInspectionAudits.Add((ScheduledInspectionAudit)audit);
                            break; 
                        case "UnitsEkurhuleniHousingCompany":
                            UnitsEkurhuleniHousingCompanyAudits.Add((UnitsEkurhuleniHousingCompanyAudit)audit);
                            break;
                        case "PropertyLeaseAgreementMaster":
                            PropertyLeaseAgreementMasterAudits.Add((PropertyLeaseAgreementMasterAudit)audit);
                            break;        
                        case "PropertyLeaseRenewalOffer":
                            PropertyLeaseRenewalOfferAudits.Add((PropertyLeaseRenewalOfferAudit)audit);
                            break;
                        case "PropertyLeaseActionComments":
                            PropertyLeaseActionCommentsAudits.Add((PropertyLeaseActionCommentsAudit)audit);
                            break;
                        case "InspectionSchedule":
                            InspectionScheduleAudits.Add((InspectionScheduleAudit)audit);
                            break;   
                        case "DateToSchedule":
                            DateToScheduleAudits.Add((DateToScheduleAudit)audit);
                            break;
                        case "TimeSlot":
                            TimeSlotAudits.Add((TimeSlotAudit)audit);
                            break;     
                        case "PropertyManager":
                            PropertyManagerAudits.Add((PropertyManagerAudit)audit);
                            break; 
                        case "DebitOrderRegistration":
                            DebitOrderRegistrationAudits.Add((DebitOrderRegistrationAudit)audit);
                            break;
                        case "Region":
                            RegionAudits.Add((RegionAudit)audit);
                            break;
                        case "RegionType":
                            RegionTypeAudits.Add((RegionTypeAudit)audit);
                            break;
                        case "HumanSettlementApplication":
                            HumanSettlementApplicationAudits.Add((HumanSettlementApplicationAudit)audit);
                            break;

                        case "MonthlyIncome":
                            MonthlyIncomeAudits.Add((MonthlyIncomeAudit)audit);
                            break;
                        case "MonthlyExpense":
                            MonthlyExpenseAudits.Add((MonthlyExpenseAudit)audit);
                            break;
                        case "ApplicantType":
                            ApplicantTypeAudits.Add((ApplicantTypeAudit)audit);
                            break;

                        case "TenantTraining":
                            TenantTrainingAudits.Add((TenantTrainingAudit)audit);
                            break;
                        case "TrainingSlide":
                            TrainingSlideAudits.Add((TrainingSlideAudit)audit);
                            break;
                        case "ExaminationQuestion":
                            ExaminationQuestionAudits.Add((ExaminationQuestionAudit)audit);
                            break;
                        case "TenantExamAnswer":
                            TenantExamAnswerAudits.Add((TenantExamAnswerAudit)audit);
                            break;

                    }
                }
            }
            catch (Exception x)
            {
                throw x;
            }
        }

        private static Dictionary<Type, EntitySetBase> _mappingCache = new Dictionary<Type, EntitySetBase>();

        /// <summary>
        /// Gets the entity set.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Entity type not found in GetTableName</exception>
        private EntitySetBase GetEntitySet(Type type)
        {
            if (!_mappingCache.ContainsKey(type))
            {
                ObjectContext octx = ((IObjectContextAdapter)this).ObjectContext;
                string typeName = ObjectContext.GetObjectType(type).Name;
                var es =
                    octx.MetadataWorkspace.GetItemCollection(DataSpace.SSpace).GetItems<EntityContainer>().SelectMany(
                        c => c.BaseEntitySets.Where(e => e.Name == typeName)).FirstOrDefault();

                if (es == null)
                    throw new ArgumentException("Entity type not found in GetTableName", typeName);

                _mappingCache.Add(type, es);
            }

            return _mappingCache[type];
        }

        /// <summary>
        /// Gets the name of the table of the entity type.
        /// Used for auditing entities.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private string GetTableName(Type type)
        {
            EntitySetBase es = GetEntitySet(type);
            return string.Format("{0}", es.MetadataProperties["Table"].Value);
        }

        /// <summary>
        /// Gets the primary key property of an entity type.
        /// Used to get the value of the primary key for auditing.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private string GetPrimaryKeyProperty(Type type)
        {
            EntitySetBase es = GetEntitySet(type);
            return es.ElementType.KeyMembers[0].Name;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // JK.20140902a - Include this to remove cascade deletions.
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

            //modelBuilder.Entity<Sheet>()
            //    .HasOptional(f => f.BulkSheet)
            //    .WithMany()
            //    .HasForeignKey(f => f.SheetId);

            //modelBuilder.Entity<Sheet>()
            //    .HasOptional(f => f.SheetType)
            //    .WithMany()
            //    .HasForeignKey(f => f.SheetId);

            modelBuilder.Entity<SystemUser>()
                .HasOptional(f => f.CreatedBySystemUser)
                .WithMany()
                .HasForeignKey(f => f.CreatedBySystemUserId);

            modelBuilder.Entity<SystemUser>()
                .HasOptional(f => f.ModifiedBySystemUser)
                .WithMany()
                .HasForeignKey(f => f.ModifiedBySystemUserId);

            // Configure decimal properties for MaintenanceJobCardTask
            modelBuilder.Entity<MaintenanceJobCardTask>()
                .Property(p => p.QuantityUsed)
                .HasPrecision(18, 2);

            modelBuilder.Entity<MaintenanceJobCardTask>()
                .Property(p => p.TotalCosts)
                .HasPrecision(18, 2);

            // Configure decimal properties for PropertyLeaseAgreementMaster enhancements
            modelBuilder.Entity<PropertyLeaseAgreementMaster>()
                .Property(p => p.AccessCardDeposit)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PropertyLeaseAgreementMaster>()
                .Property(p => p.KeyDeposit)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PropertyLeaseAgreementMaster>()
                .Property(p => p.DSTVActivationFee)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PropertyLeaseAgreementMaster>()
                .Property(p => p.DSTVMonthlyLevy)
                .HasPrecision(18, 2);
        }

        
    }
}