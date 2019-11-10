using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MediaApp.Models;
using System.Data.SqlClient;
using System.IO;

namespace MediaApp.Controllers
{
    public class PeopleController : BaseController
    {
        // GET: People
        public ActionResult Index()
        {
            return View(GetPeopleList());
        }

        public ActionResult Edit(int id)
        {
            People person = new People();
            var dbConn = dbConnection();
            string queryString = "SELECT Name FROM dbo.People WHERE Id = " + id + ";";
            SqlCommand cmd = new SqlCommand(queryString, dbConn);
            dbConn.Open();

            try
            {
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    person = new People(id, reader.GetString(0));
                    reader.Close();
                    break;
                }
            } catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error: "+ e.Message);
            }
            cmd.Dispose();

            person.Movies = GetMovieList(id);
            AddImage(person);

            return View(person);
        }

        [HttpPost]
        public ActionResult Edit(People people)
        {
            var dbConn = dbConnection();

            string queryString =
                "UPDATE " +
                    "dbo.People " +
                "SET " +
                    "Name = @name " +
                "WHERE " +
                    "Id = @peopleId;";
			SqlCommand cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@name", people.Name);
			cmd.Parameters.AddWithValue("@peopleId", people.Id);
			ExecuteCmd(cmd, dbConn);

            UpdateImage(people, dbConn);

            return RedirectToAction("Index");
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
                    People person = new People(reader.GetInt32(0), reader.GetString(1));
                    person.Movies = GetMovieList(reader.GetInt32(0));
                    AddImage(person);
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
                    "m.Id, m.Title, m.Year " +
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
                    Movie movie = new Movie(reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2));
                    movieList.Add(movie);
                }
            } catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error: "+ e.Message);
            }

            return movieList;
        }

        private void AddImage(People person)
        {
            var dbConn = dbConnection();
            string queryString = 
                "SELECT " +
                    "i.Name, i.Type, i.Image " +
                "FROM " +
                    "dbo.People AS p " +
                "INNER JOIN " +
                    "dbo.Image AS i ON p.ImageId = i.Id " +
                "WHERE " +
                    "p.Id = @personId;";

            SqlCommand cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@personId", person.Id);

            dbConn.Open();

            try
            {
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Image image = new Image();
                    image.Name = reader.GetString(0);
                    image.Type = reader.GetString(1);

                    byte[] imageData = (byte[])reader[2];
                    image.Data = Convert.ToBase64String(imageData);

                    person.PeopleImage = image;
                }
                reader.Close();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error: " + e.Message);
            }
            cmd.Dispose();
            dbConn.Close();
        }

        private void UpdateImage(People person, SqlConnection dbConn)
        {
            if (person.ImageInput != null)
            {
                BinaryReader reader = new BinaryReader(person.ImageInput.InputStream);
                byte[] imageData = reader.ReadBytes(person.ImageInput.ContentLength);

                string fileName = person.ImageInput.FileName;
                string type = person.ImageInput.ContentType;

                string queryString = "INSERT INTO dbo.Image(Image, Name, Type) VALUES(@imageData, @fileName, @type)";
                SqlCommand cmd = new SqlCommand(queryString, dbConn);
                cmd.Parameters.AddWithValue("@imageData", imageData);
                cmd.Parameters.AddWithValue("@fileName", fileName);
                cmd.Parameters.AddWithValue("@type", type);
                ExecuteCmd(cmd, dbConn);

                queryString = "SELECT Id FROM dbo.Image WHERE Name = @fileName;";
                cmd = new SqlCommand(queryString, dbConn);
                cmd.Parameters.AddWithValue("@fileName", fileName);
                int imageId = GetId(cmd, dbConn);

                queryString = "UPDATE dbo.People SET ImageId = @imageId WHERE Id = @peopleId;";
                cmd = new SqlCommand(queryString, dbConn);
                cmd.Parameters.AddWithValue("@imageId", imageId);
                cmd.Parameters.AddWithValue("@peopleId", person.Id);
                ExecuteCmd(cmd, dbConn);

                reader.Dispose();
            }
        }
    }
}
