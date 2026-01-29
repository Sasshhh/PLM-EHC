using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.DataAccessLayer
{
    /// <summary>
    /// JK.20160801a - Used to intercept if the database is offline.
    /// </summary>
    /// <seealso cref="System.Data.Entity.Infrastructure.Interception.IDbCommandInterceptor" />
    public class DbConextCommandInterceptor : IDbCommandInterceptor
    {
        public void NonQueryExecuting( DbCommand command, DbCommandInterceptionContext<int> interceptionContext )
        {
            CheckDbAvailability( command );
        }

        public void NonQueryExecuted( DbCommand command, DbCommandInterceptionContext<int> interceptionContext )
        {
            //CheckDbAvailability( command );
        }

        public void ReaderExecuting( DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext )
        {
            CheckDbAvailability( command );
        }

        public void ReaderExecuted( DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext )
        {
            //CheckDbAvailability( command );
        }

        public void ScalarExecuting( DbCommand command, DbCommandInterceptionContext<object> interceptionContext )
        {
            CheckDbAvailability( command );
        }

        public void ScalarExecuted( DbCommand command, DbCommandInterceptionContext<object> interceptionContext )
        {
            //CheckDbAvailability( command );
        }

        /// <summary>
        /// Checks the database availability.
        /// </summary>
        /// <param name="command">The command.</param>
        private void CheckDbAvailability( DbCommand command )
        {
            using ( var conn = command.Connection )
            {
                try
                {
                    conn.Open();
                }
                catch ( Exception )
                {
                    command.Cancel();
                }
            }
        }
    }
}