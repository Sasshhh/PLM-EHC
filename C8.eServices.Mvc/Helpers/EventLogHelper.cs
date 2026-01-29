using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.Helpers
{

    public class EventLogHelper
    {
        public static void Log(string logEntry)
        {
            if (!EventLog.SourceExists(EventLogKeys.Source))
                EventLog.CreateEventSource(EventLogKeys.Source, EventLogKeys.Log);

            EventLog.WriteEntry(EventLogKeys.Source, logEntry);
        }
        public void LogTryCatchException(Exception e, eServicesDbContext db)
        {
            int logTypeId = db.LogTypes.FirstOrDefault(x => x.Key == LogTypeKeys.TryCatchException).Id;
            int referenceTypeId = db.ReferenceTypes.FirstOrDefault(x => x.Key == ReferenceTypeKeys.ExceptionLog).Id;
            //db.Logs.Add(new Log()
            //{
            //    LogTypeId = logTypeId,
            //    //LogEntry = (e.ToString() != null) ? e.ToString() : "An error occured while saving data to the db",
            //    LogEntry = "An error occured while saving data to the db",
            //    ReferenceId = 0,
            //    ReferenceTypeId = referenceTypeId,
            //    IsActive = true,
            //    IsDeleted = false,
            //    IsLocked = false
            //});
            //db.SaveChanges();
            LogSystemError(e.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
        }
        public static void LogDBCError(string logEntry, string exceptionEntry)
        {
            #region Text Error Log
            var txtErrorLog = string.Format("ReferenceId: {0} ReferenceTypeId: {1} LogEntry: {2} LogTypeId: {3} Active: {4} Deleted: {5} SystemError: {6} CreatedDateTime: {7}", 1, 4, exceptionEntry, 2, true, false, logEntry, DateTime.Now);
            //var root = HttpContext.Current.Server.MapPath("~/Logs/");
            //string fileName = "DBC_Error_Log.txt";
            //var path = System.IO.Path.Combine(root, fileName);
            //path = System.IO.Path.GetFullPath(path);
            //System.IO.File.WriteAllText(path, txtErrorLog);
            DocumentHelper.WriteToTextFile("DBC_Error_Log.txt", txtErrorLog);
            #endregion
        }
        public static void LogSystemError(string logEntry, string logType, string referenceType)
        {
            #region Text Error Log
            using (var context = new eServicesDbContext())
            {
                try
                {
                    int rType = context.ReferenceTypes.FirstOrDefault(o => o.Key == referenceType).Id;
                    int lType = context.LogTypes.FirstOrDefault(o => o.Key == logType).Id;

                    var txtErrorLog = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}", 1, rType, logEntry, lType, true, false, DateTime.Now);
                    DocumentHelper.WriteToTextFile("System_Error_Log.txt", txtErrorLog);

                    context.Logs.Add(new Log()
                    {
                        ReferenceId = 2,
                        ReferenceTypeId = rType,
                        LogEntry = logEntry,
                        LogTypeId = lType,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedDateTime = DateTime.Now
                    });
                    context.SaveChanges();
                }
                catch (Exception noConn)
                {
                    LogDBCError(logEntry, noConn.Message);
                }
            }
            #endregion
        }
    }
    //public class EventLogHelper
    //{

    //    //public static void Log( string logEntry )
    //    //{
    //    //    if ( !EventLog.SourceExists( EventLogKeys.Source ) )
    //    //        EventLog.CreateEventSource( EventLogKeys.Source, EventLogKeys.Log );

    //    //    EventLog.WriteEntry( EventLogKeys.Source, logEntry );
    //    //}
    //}
}