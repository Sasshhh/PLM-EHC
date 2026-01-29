using C8.eServices.Mvc.Helpers.Abstract;
using C8.eServices.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Helpers
{
    public class StoredProcedure : IStoredProcedure
    {
        public IEnumerable<Audit> AuditRecords()
        {
            List<Audit> records = new List<Audit>();
            SqlConnection conn = new SqlConnection();
            SqlCommand cmd = new SqlCommand();
            conn.ConnectionString = ConfigurationManager.ConnectionStrings["eServicesDbContext"].ConnectionString;
            cmd.Connection = conn;
            using (SqlCommand command = new SqlCommand("GetAuditRecords", conn))
            {
                command.CommandType = CommandType.StoredProcedure;
                conn.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        //if (records.Count > 10000) break;
                        try
                        {
                            Audit record = new Audit
                            {
                                AuditId = Convert.ToInt32(reader["AuditId"]),
                                Action = reader["Action"].ToString(),
                                PrimaryKey = Convert.ToInt32(reader["PrimaryKey"]),
                                TableName = reader["TableName"].ToString(),
                                ColumnName = reader["ColumnName"].ToString(),
                                OriginalValue = reader["OriginalValue"].ToString(),
                                CurrentValue = reader["CurrentValue"].ToString(),
                                AuditBySystemUserId = Convert.ToInt32(reader["AuditBySystemUserId"]),
                                AuditDateTime = Convert.ToDateTime(reader["AuditDateTime"]),
                                IPAddress = reader["IPAddress"].ToString(),
                            };
                            records.Add(record);
                        }
                        catch
                        {
                            //do basically nothing 
                        }
                    }
                }
            }
            return records;
        }
    }
}