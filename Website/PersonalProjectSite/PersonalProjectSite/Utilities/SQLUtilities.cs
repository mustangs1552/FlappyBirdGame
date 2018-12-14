using System;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace Utilities
{
    /// <summary>
    /// Class of utilities for executing SQL queries.
    /// </summary>
    public static class SQLUtilities
    {
        /// <summary>
        /// Executes a non-parameterized query and returns result.
        /// </summary>
        /// <param name="connString">The database connection string.</param>
        /// <param name="sqlString">The SQL string to execute.</param>
        /// <param name="PopulateFunc">The function to use to populate the list of models.</param>
        /// <returns>The list of models found.</returns>
        public static List<T> PerformSQL<T>(string connString, string sqlString, Func<SqlDataReader, List<T>> PopulateFunc)
        {
            return (List<T>)Convert.ChangeType(PerformSQL<T>(connString, sqlString, null, PopulateFunc), typeof(List<T>));
        }
        /// <summary>
        /// Executes a parameterized query and returns result.
        /// </summary>
        /// <param name="connString">The database connection string.</param>
        /// <param name="sqlString">The SQL string to execute.</param>
        /// <param name="parameters">Dictionary of the parameters in the format: Key=@param, Value=value.</param>
        /// <param name="PopulateFunc">The function to use to populate the list of models.</param>
        /// <returns>The list of models found.</returns>
        public static List<T> PerformSQL<T>(string connString, string sqlString, Dictionary<string, Object> parameters, Func<SqlDataReader, List<T>> PopulateFunc)
        {
            int temp = -1;
            return (List<T>)Convert.ChangeType(PerformSQL<T>(connString, sqlString, parameters, out temp, PopulateFunc), typeof(List<T>));
        }
        /// <summary>
        /// Executes a parameterized query and returns result if the sqlString begins with "SELECT" otherwise will update rowsAddected with SQL rows affected.
        /// </summary>
        /// <param name="connString">The database connection string.</param>
        /// <param name="sqlString">The SQL string to execute.</param>
        /// <param name="parameters">Dictionary of the parameters in the format: Key=@param, Value=value.</param>
        /// <param name="rowsAffected">SQL rows affected.</param>
        /// <param name="PopulateFunc">The function to use to populate the list of models (Optional if not expecting a query back).</param>
        /// <returns>The list of models found.</returns>
        public static List<T> PerformSQL<T>(string connString, string sqlString, Dictionary<string, Object> parameters, out int rowsAffected, Func<SqlDataReader, List<T>> PopulateFunc = null)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(sqlString, conn);
                    if (parameters != null)
                    {
                        foreach (KeyValuePair<string, Object> pair in parameters)
                        {
                            cmd.Parameters.AddWithValue(pair.Key, pair.Value);
                        }
                    }

                    if (sqlString.Substring(0, 6).ToUpper() == "SELECT")
                    {
                        rowsAffected = -1;
                        if (PopulateFunc != null) return (List<T>)Convert.ChangeType(PopulateFunc(cmd.ExecuteReader()), typeof(List<T>));
                        else return new List<T>();
                    }
                    else
                    {
                        rowsAffected = cmd.ExecuteNonQuery();
                        return new List<T>();
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }
        }
    }
}
