using C8.eServices.Mvc.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class CommunicationType : BaseType
    {
        #region Business Logic Methods
        readonly eServicesDbContext dbContext = new eServicesDbContext();

        public IEnumerable<CommunicationType> GetCommunications()
        {

            return dbContext.CommunicationTypes.ToList();
        }

        public IEnumerable<CommunicationType> GetAllCommunicationTypes()
        {
            return GetCommunications().Where(x => x.IsActive == true && !x.IsDeleted).ToList();
        }

        public IEnumerable<CommunicationType> GetAllCommunicationActive()
        {
            var test = GetAllCommunicationTypes().ToList();/*.Where(x => x.IsActive == true && !x.IsDeleted)*/
            return test;
        }


        #endregion
    }
}