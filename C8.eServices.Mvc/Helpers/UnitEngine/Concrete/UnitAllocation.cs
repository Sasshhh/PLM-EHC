using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Helpers.UnitEngine.Abstract;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.ViewModels;
using Microsoft.BusinessData.MetadataModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace C8.eServices.Mvc.Helpers.UnitEngine.Concrete
{
    public class UnitAllocation : IUnitAllocation 
    {
        private readonly eServicesDbContext _context;
        private readonly BaseHelper baseHelper;
        public UnitAllocation(eServicesDbContext _context)
        {
            this._context = _context;
            baseHelper = new BaseHelper();
        }
        public void AuditUnitHistory(ApplicationAllocatedProperty collection, string State, string Description)
        {
            baseHelper.Initialise(_context);
            AllocatedUnitHistory allocatedUnitHistory = new AllocatedUnitHistory
            {
                PropertyLeaseApplicationId = collection.PropertyLeaseApplicationId,
                State = State,
                Description = Description,
                ByLettingOfficerId = baseHelper.SystemUser.Id,
                ApplicationAllocatedPropertyId = collection.Id
            };
            Save(collection);
            Save(allocatedUnitHistory);
        }

        public void Save<T>(T collection) where T : class
        {
            _context.Set<T>().Add(collection);
            _context.SaveChanges();
        }
        
        public void Update<T>(T collection) where T : class
        {
            _context.Set<T>().Attach(collection);
            _context.Entry(collection).State = EntityState.Modified;
        }

        public RiskAssessmentOutcome SaveRisk(CaptureViewModel collection, string RCSActionTypeKey, int PropertyId, string StatusKey)
        {
            Status status = _context.Status.FirstOrDefault(x => x.Key == StatusKey);
             
            RiskAssessmentOutcome outcome = new RiskAssessmentOutcome
            {
                StatusId = status.Id,
                FirstName = collection.RiskAssessmentOutcome.FirstName,
                LastName = collection.RiskAssessmentOutcome.LastName,
                Reason = collection.RiskAssessmentOutcome.Reason,
                Outcome = (RCSActionTypeKey == RCSActionTypeKeys.Approved) ? "Approved" : "Rejected",
                PropertyLeaseApplicationId = PropertyId
            };
            Save(outcome);
            return outcome;
        }


    }
}