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
        public ActionResult Index(int index)
        {
//            DeleteEverything();
//            AddPeopleImages();
            ViewBag.Index = index;
            return View(GetPeopleList(index));
        }

        public ActionResult Edit(int id)
        {
            People person = new People();
            var dbConn = dbConnection();
            string queryString = "SELECT Name FROM dbo.People WHERE Id = " + id + ";";
            SqlCommand cmd = new SqlCommand(queryString, dbConn);

            try
            {
                dbConn.Open();
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
            finally
            {
                cmd.Dispose();
                dbConn.Close();
            }

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

        private List<People> GetPeopleList(int index)
        {
            List<People> peopleList = new List<People>();
            var dbConn = dbConnection();
            string queryString = "SELECT Id, Name FROM dbo.People ORDER BY Name ASC;";
            SqlCommand cmd = new SqlCommand(queryString, dbConn);

            try
            {
                dbConn.Open();
                int i = 0;
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read() && i < index + 50)
                {
                    if (i >= index)
                    {
                        People person = new People(reader.GetInt32(0), reader.GetString(1));
                        person.Movies = GetMovieList(reader.GetInt32(0));
                        AddImage(person);
                        peopleList.Add(person);
                    }
                    i++;
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

            try
            {
                dbConn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Movie movie = new Movie(reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2));
                    movieList.Add(movie);
                }
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

            return movieList;
        }

        private void AddImage(People person)
        {
            var dbConn = dbConnection();
            string queryString = 
                "SELECT " +
                    "i.Name, i.Type, i.Image, i.Url " +
                "FROM " +
                    "dbo.People AS p " +
                "INNER JOIN " +
                    "dbo.Image AS i ON p.ImageId = i.Id " +
                "WHERE " +
                    "p.Id = @personId;";

            SqlCommand cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@personId", person.Id);

            try
            {
                dbConn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {

                    if (reader.GetString(3) != null)
                    {
                        person.imgURL = reader.GetString(3);
                    } 
                    else
                    {
                        Image image = new Image();
                        image.Name = reader.GetString(0);
                        image.Type = reader.GetString(1);

                        byte[] imageData = (byte[])reader[2];
                        image.Data = Convert.ToBase64String(imageData);

                        person.PeopleImage = image;
                    }
                }
                reader.Close();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error: " + e.Message);
            }
            finally
            {
                cmd.Dispose();
                dbConn.Close();
            }
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

        private void AddPeopleImages()
        {
            var fileContents = System.IO.File.ReadAllText(Server.MapPath(@"~/App_Data/imageList.txt"));
            string[] images = fileContents.Split('\n');
            foreach (var imgData in images)
            {
                string[] data = imgData.Split(';');
                string name = data[0];
                string imgUrl = data[1];

                var dbConn = dbConnection();

                string queryString = "SELECT Id FROM dbo.People WHERE Name = @Name;";
                SqlCommand cmd = new SqlCommand(queryString, dbConn);
                cmd.Parameters.AddWithValue("@Name", name);
                int peopleId = GetId(cmd, dbConn);

                if (peopleId < 0 || imgUrl == "NULL\r")
                {
                    continue;
                }

                queryString = "INSERT INTO dbo.Image(Url) VALUES(@Url)";
                cmd = new SqlCommand(queryString, dbConn);
                cmd.Parameters.AddWithValue("@Url", imgUrl);
                ExecuteCmd(cmd, dbConn);

                queryString = "SELECT Id FROM dbo.Image WHERE Url = @Url;";
                cmd = new SqlCommand(queryString, dbConn);
                cmd.Parameters.AddWithValue("@Url", imgUrl);
                int imageId = GetId(cmd, dbConn);

                queryString = "UPDATE dbo.People SET ImageId = @imageId WHERE Id = @peopleId;";
                cmd = new SqlCommand(queryString, dbConn);
                cmd.Parameters.AddWithValue("@imageId", imageId);
                cmd.Parameters.AddWithValue("@peopleId", peopleId);
                ExecuteCmd(cmd, dbConn);
            }
        }

        private void DeleteEverything()
        {
            var dbConn = dbConnection();

            string queryString = "DELETE FROM dbo.MovPeople;";
            SqlCommand cmd = new SqlCommand(queryString, dbConn);
			ExecuteCmd(cmd, dbConn);

            queryString = "DELETE FROM dbo.Genre;";
            cmd = new SqlCommand(queryString, dbConn);
			ExecuteCmd(cmd, dbConn);

            queryString = "DELETE FROM dbo.Image;";
            cmd = new SqlCommand(queryString, dbConn);
			ExecuteCmd(cmd, dbConn);

            queryString = "DELETE FROM dbo.People;";
            cmd = new SqlCommand(queryString, dbConn);
			ExecuteCmd(cmd, dbConn);

            queryString = "DELETE FROM dbo.Movie;";
            cmd = new SqlCommand(queryString, dbConn);
			ExecuteCmd(cmd, dbConn);
        }

    }
}
