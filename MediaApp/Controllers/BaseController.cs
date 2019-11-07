using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace MediaApp.Controllers
{
    public class BaseController : Controller
    {
        [NonAction]
        protected static SqlConnection dbConnection()
        {
            string connString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Josh\\source\\repos\\MediaApp\\MediaApp\\App_Data\\Database1.mdf;Integrated Security=True";
            SqlConnection conn = new SqlConnection(connString);

            return conn;
        }

        [NonAction]
        protected void ExecuteSqlString(SqlConnection dbConn, string queryString)
        {
            SqlCommand command = new SqlCommand(queryString, dbConn);
            dbConn.Open();
            try
            {
                command.ExecuteNonQuery();
            } catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error: + " + e);
            }
            command.Dispose();
            dbConn.Close();
        }

        [NonAction]
        protected int GetId(string queryString, SqlConnection dbConn)
        {
            int id = -1;
            SqlCommand command = new SqlCommand(queryString, dbConn);
            try
            {
                dbConn.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    id = reader.GetInt32(0);
                    break;
                }
                reader.Close();
            } catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error: "+ e.Message);
            }
            command.Dispose();
            dbConn.Close();

            return id;
        }
    }
}