using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MediaApp.Models;
using System.Data.SqlClient;

namespace MediaApp.Controllers
{
    public class UploadController : BaseController
    {
        // GET: Upload
        public ActionResult Index()
        {
            System.Diagnostics.Debug.WriteLine("UploadController Run");
            
            DeleteEverything();
            UploadMovies();
            UploadPeopleImages();

            return View();
        }
        private void DeleteEverything()
        {
            var dbConn = dbConnection();

            string queryString = "DELETE FROM dbo.Directors;";
            SqlCommand cmd = new SqlCommand(queryString, dbConn);
			ExecuteCmd(cmd, dbConn);

            queryString = "DELETE FROM dbo.Actors;";
            cmd = new SqlCommand(queryString, dbConn);
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

        public void UploadMovies()
        {
            var fileContents = System.IO.File.ReadAllText(Server.MapPath(@"~/App_Data/movieList.txt"));
            string[] movies = fileContents.Split('\n');
            foreach (var movie in movies)
            {
                string[] data = movie.Split(';');
                if (data == null || data[0] == "\r" || data[0] == "")
                {
                    continue;
                }
                Movie mov = new Movie();

                mov.Title = data[0];
                mov.Year = Int32.Parse(data[1]);
                mov.Director = new People(data[2]);

                string[] actors = data[3].Split(',');
                foreach(var actor in actors)
                {
                    People person = new People(actor.Trim());
                    mov.Cast.Add(person);
                }

                string[] genres = data[4].Split(',');
                foreach(var genre in genres)
                {
                    mov.Genre.Add(genre.Trim());
                }

                mov.Rating = (int)(float.Parse(data[5]) * 10);
                mov.Synopsis = data[6];
                mov.imgURL = data[7];

                var dbConn = dbConnection();

                try
                {
                    string queryString = "INSERT INTO dbo.Movie(Title, Year) VALUES (@title, @year);";
                    SqlCommand cmd = new SqlCommand(queryString, dbConn);
                    cmd.Parameters.AddWithValue("@title", mov.Title);
                    cmd.Parameters.AddWithValue("@year", mov.Year);
                    ExecuteCmd(cmd, dbConn);

                    queryString = "SELECT Id FROM dbo.Movie WHERE Title = @title AND Year = @year;";
                    cmd = new SqlCommand(queryString, dbConn);
                    cmd.Parameters.AddWithValue("@title", mov.Title);
                    cmd.Parameters.AddWithValue("@year", mov.Year);
                    mov.Id = GetId(cmd, dbConn);

                    //Update Director
                    queryString = "INSERT INTO dbo.People(Name) VALUES(@name);";
                    cmd = new SqlCommand(queryString, dbConn);
                    cmd.Parameters.AddWithValue("@name", mov.Director.Name);
                    ExecuteCmd(cmd, dbConn);

                    queryString = "SELECT Id FROM dbo.People WHERE Name = @name;";
                    cmd = new SqlCommand(queryString, dbConn);
                    cmd.Parameters.AddWithValue("@name", mov.Director.Name);
                    int peopleId = GetId(cmd, dbConn);

                    queryString = "INSERT INTO dbo.Directors (PeopleId, MovieId) VALUES (@peopleId, @movieId);";
                    cmd = new SqlCommand(queryString, dbConn);
                    cmd.Parameters.AddWithValue("@peopleId", peopleId);
                    cmd.Parameters.AddWithValue("@movieId", mov.Id);
                    ExecuteCmd(cmd, dbConn);

                    //Update Actors
                    foreach (People actor in mov.Cast)
                    {
                        if (actor.Name != null && actor.Name.Length > 0)
                        {
                            queryString = "INSERT INTO dbo.People(Name) VALUES(@name);";
                            cmd = new SqlCommand(queryString, dbConn);
                            cmd.Parameters.AddWithValue("@name", actor.Name);
                            ExecuteCmd(cmd, dbConn);

                            queryString = "SELECT Id FROM dbo.People WHERE Name = @name;";
                            cmd = new SqlCommand(queryString, dbConn);
                            cmd.Parameters.AddWithValue("@name", actor.Name);
                            peopleId = GetId(cmd, dbConn);

                            queryString = "INSERT INTO dbo.Actors (PeopleId, MovieId) VALUES (@peopleId, @movieId);";
                            cmd = new SqlCommand(queryString, dbConn);
                            cmd.Parameters.AddWithValue("@peopleId", peopleId);
                            cmd.Parameters.AddWithValue("@movieId", mov.Id);
                            ExecuteCmd(cmd, dbConn);
                        }
                    }

                    //Update Genre
                    foreach (string genre in mov.Genre)
                    {
                        if (genre.Length > 0)
                        {
                            queryString = 
                                "INSERT INTO dbo.Genre (Genre, MovieId) " +
                                "VALUES(@genre, @movieId);";
                            cmd = new SqlCommand(queryString, dbConn);
                            cmd.Parameters.AddWithValue("@genre", genre);
                            cmd.Parameters.AddWithValue("@movieId", mov.Id);
                            ExecuteCmd(cmd, dbConn);
                        } 
                    }

                    var movController = DependencyResolver.Current.GetService<MovieController>();
                    movController.ControllerContext = new ControllerContext(this.Request.RequestContext, movController);

                    movController.UpdateRating(mov, dbConn);
                    movController.UpdateSynopsis(mov, dbConn);
                    movController.UpdateImage(mov, dbConn);
                }
                catch(Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Error UploadController: "+ e.Message);
                }
            }
        }
        private void UploadPeopleImages()
        {
            var fileContents = System.IO.File.ReadAllText(Server.MapPath(@"~/App_Data/imageList.txt"));
            string[] images = fileContents.Split('\n');
            foreach (var imgData in images)
            {
                string[] data = imgData.Split(';');

                if (data == null || data[0] == "\r" || data[0] == "")
                {
                    continue;
                }
                
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

                try
                {
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
                catch(Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Error UploadController: "+ e.Message);
                }
            }
        }
    }
}