using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.ViewModels;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C8.eServices.Mvc.Helpers.UnitEngine.Abstract
{
    public interface IUnitAllocation
    {
        void AuditUnitHistory(ApplicationAllocatedProperty collection, String State, String Description);
        void Save<T>(T collection) where T : class;
        void Update<T>(T collection) where T : class;
        RiskAssessmentOutcome SaveRisk(CaptureViewModel collection, String RCSActionTypeKey, Int32 PropertyId, String StatusKey);
    }
}
