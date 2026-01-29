using C8.eServices.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C8.eServices.Mvc.Helpers.Abstract
{
    public interface IStoredProcedure
    {
        IEnumerable<Audit> AuditRecords();
    }
}
