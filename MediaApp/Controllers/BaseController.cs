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
		protected void ExecuteCmd(SqlCommand cmd, SqlConnection dbConn)
		{
            try
            {
                dbConn.Open();
                cmd.ExecuteNonQuery();
            } 
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error: + " + e);
            }
            finally
            {
                cmd.Dispose();
                dbConn.Close();
            }
		}

		[NonAction]
		protected int GetId(SqlCommand cmd, SqlConnection dbConn){
            int id = -1;
            try
            {
                dbConn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    id = reader.GetInt32(0);
                    break;
                }
                reader.Close();
            } 
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error: "+ e.Message);
            }
            finally
            {
                cmd.Dispose();
                dbConn.Close();
            }

            return id;
		}
    }
}
