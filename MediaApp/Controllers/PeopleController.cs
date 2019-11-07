using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MediaApp.Models;
using System.Data.SqlClient;

namespace MediaApp.Controllers
{
    public class PeopleController : BaseController
    {
        // GET: People
        public ActionResult Index()
        {
            return View(GetPeopleList());
        }

        private List<People> GetPeopleList()
        {
            List<People> peopleList = new List<People>();
            var dbConn = dbConnection();
            string queryString = "SELECT Id, Name FROM dbo.People;";
            
            SqlCommand cmd = new SqlCommand(queryString, dbConn);
            dbConn.Open();

            try
            {
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    People person = new People(reader.GetString(1));
                    person.Movies = GetMovieList(reader.GetInt32(0));
                    peopleList.Add(person);
                }
                reader.Close();
            } catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error: "+ e.Message);
            }
            cmd.Dispose();
            dbConn.Close();

            return peopleList;
        }

        private List<Movie> GetMovieList(int id)
        {
            List<Movie> movieList = new List<Movie>();
            var dbConn = dbConnection();

            string queryString =
                "SELECT " +
                    "Title, Year " +
                "FROM " +
                    "dbo.Movie AS m " +
                "INNER JOIN " +
                    "dbo.MovPeople AS mp ON m.Id = mp.MovieId " +
                "INNER JOIN " +
                    "dbo.People AS p ON mp.PeopleId = p.Id " +
                "WHERE " +
                    "p.Id = @id;";

            SqlCommand cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@id", id);

            dbConn.Open();

            try
            {
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Movie movie = new Movie(reader.GetString(0), reader.GetInt32(1));
                    movieList.Add(movie);
                    System.Diagnostics.Debug.WriteLine(reader.GetString(0) + ", " + reader.GetInt32(1));
                }
            } catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error: "+ e.Message);
            }

            return movieList;
        }
    }
}
