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
    public class MovieController : BaseController
    {
        // GET: Movie
        public ActionResult Index()
        {
            return View(GetMovieList());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Movie mov)
        {
            var dbConn = dbConnection();

            // Create Movie
            string queryString = "INSERT INTO dbo.Movie(Title, Year, Rating) VALUES (@title, @year, @rating);";
			SqlCommand cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@title", mov.Title);
			cmd.Parameters.AddWithValue("@year", mov.Year);
			cmd.Parameters.AddWithValue("@rating", mov.Rating);
			ExecuteCmd(cmd, dbConn);

            queryString = "SELECT Id FROM dbo.Movie WHERE Title = @title AND Year = @year;";
			SqlCommand cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@title", mov.Title);
			cmd.Parameters.AddWithValue("@year", mov.Year);
			int movieId = GetId(cmd, dbConn);

            // Create Director
            if (mov.Director != null && mov.Director.Name.Length > 0)
            {
                queryString = "INSERT INTO dbo.People(Name) VALUES (@name);";
				SqlCommand cmd = new SqlCommand(queryString, dbConn);
				cmd.Parameters.AddWithValue("@name", mov.Director.Name);
				ExecuteCmd(cmd, dbConn);

                queryString = "SELECT Id FROM dbo.People WHERE Name = @name;";
				SqlCommand cmd = new SqlCommand(queryString, dbConn);
				cmd.Parameters.AddWithValue("@name", mov.Director.Name);
				int peopleId = GetId(cmd, dbConn);

                queryString = "INSERT INTO dbo.MovPeople(PeopleId, MovieId, Job) Values(@peopleId, @movieId, 'Director');";
				SqlCommand cmd = new SqlCommand(queryString, dbConn);
				cmd.Parameters.AddWithValue("@peopleId", peopleId);
				cmd.Parameters.AddWithValue("@movieId", movieId);
				ExecuteCmd(cmd, dbConn);
            }

            // Create Actors
            foreach (People actor in mov.Cast)
            {
                if (actor.Name != null && actor.Name.Length > 0)
                {
                    queryString = "INSERT INTO dbo.People(Name) VALUES (@name);";
					SqlCommand cmd = new SqlCommand(queryString, dbConn);
					cmd.Parameters.AddWithValue("@name", actor.Name);
					ExecuteCmd(cmd, dbConn);

                    queryString = "Select Id FROM dbo.People WHERE Name = @name;";
					SqlCommand cmd = new SqlCommand(queryString, dbConn);
					cmd.Parameters.AddWithValue("@name", actor.Name);
                    int peopleId = GetId(cmd, dbConn);

                    queryString = "INSERT INTO dbo.MovPeople(PeopleId, MovieId, Job) Values(@peopleId, @movieId, 'Actor');";
					SqlCommand cmd = new SqlCommand(queryString, dbConn);
					cmd.Parameters.AddWithValue("@peopleId", peopleId);
					cmd.Parameters.AddWithValue("@movieId", movieId);
					ExecuteCmd(cmd, dbConn);
                }
            }

            // Create Genres
            foreach (string genre in mov.Genre)
            {
                if (genre.Length > 0)
                {
					queryString = "INSERT INTO dbo.Genre(MovieId, Genre) VALUES(@movieId, @genre);"
					SqlCommand cmd = new SqlCommand(queryString, dbConn);
					cmd.Parameters.AddWithValue("@movieId", movieId);
					cmd.Parameters.AddWithValue("@genre", genre);
					ExecuteCmd(cmd, dbConn);
                } 
            }

			// Create Image
            if (mov.ImageInput != null)
            {
                BinaryReader reader = new BinaryReader(mov.ImageInput.InputStream);
                byte[] imageData = reader.ReadBytes(mov.ImageInput.ContentLength);

                string fileName = mov.ImageInput.FileName;
                string type = mov.ImageInput.ContentType;

                queryString = "INSERT INTO dbo.Image(Image, Name, Type) VALUES(@imageData, @fileName, @type)";
                SqlCommand cmd = new SqlCommand(queryString, dbConn);
                cmd.Parameters.AddWithValue("@imageData", imageData);
                cmd.Parameters.AddWithValue("@fileName", fileName);
                cmd.Parameters.AddWithValue("@type", type);
				ExecuteCmd(cmd, dbConn);

                queryString = "SELECT Id FROM dbo.Image WHERE Name = @fileName;";
                SqlCommand cmd = new SqlCommand(queryString, dbConn);
                cmd.Parameters.AddWithValue("@fileName", fileName);
				int imageId = GetId(cmd, dbConn);

                queryString = "UPDATE dbo.Movie SET ImageId = @imageId WHERE Id = @movieId;";
                SqlCommand cmd = new SqlCommand(queryString, dbConn);
                cmd.Parameters.AddWithValue("@imageId", imageId);
                cmd.Parameters.AddWithValue("@movieId", movieId);
				ExecuteCmd(cmd, dbConn);
            }


            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            Movie mov = GetMovie(id);
            AddPeople(mov);
            AddGenre(mov);

            return View(mov);
        }

        [HttpPost]
        public ActionResult Edit(Movie mov)
        {
            var dbConn = dbConnection();

            string queryString =
                "UPDATE " +
                    "dbo.Movie " +
                "SET " +
                    "Title = '" + mov.Title + "', Year = " + mov.Year + ", Rating = " + mov.Rating + " " +
                "WHERE " +
                    "Id = @movieId;";
			SqlCommand cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@movieId", mov.Id);
			ExecuteCmd(cmd, dbConn);

            queryString = "DELETE FROM dbo.MovPeople WHERE MovieId = @movieId;";
			SqlCommand cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@movieId", mov.Id);
			ExecuteCmd(cmd, dbConn);

            queryString = "DELETE FROM dbo.Genre WHERE MovieId = @movieId;";
			SqlCommand cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@movieId", mov.Id);
			ExecuteCmd(cmd, dbConn);

            queryString = "INSERT INTO dbo.People(Name) VALUES(@name);";
			SqlCommand cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@name", mov.Director.Name);
			ExecuteCmd(cmd, dbConn);

            queryString = "SELECT Id FROM dbo.People WHERE Name = @name;";
			SqlCommand cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@name", mov.Director.Name);
			int peopleId = GetId(cmd, dbConn);

            queryString = 
                "INSERT INTO dbo.MovPeople(PeopleId, MovieId, Job) " +
                "VALUES(@peopleId, @movieId, 'Director');";
			SqlCommand cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@peopleId", peopleId);
			cmd.Parameters.AddWithValue("@movieId", movieId);
			ExecuteCmd(cmd, dbConn);


            foreach(People actor in mov.Cast)
            {
                queryString = "INSERT INTO dbo.People(Name) VALUES(@name);";
				SqlCommand cmd = new SqlCommand(queryString, dbConn);
				cmd.Parameters.AddWithValue("@name", actor.Name);
				ExecuteCmd(cmd, dbConn);

                queryString = "SELECT Id FROM dbo.People WHERE Name = @name;";
				SqlCommand cmd = new SqlCommand(queryString, dbConn);
				cmd.Parameters.AddWithValue("@name", actor.Name);
				int peopleId = GetId(cmd, dbConn);

                queryString = 
                    "INSERT INTO dbo.MovPeople(PeopleId, MovieId, Job) " +
                    "VALUES(@peopleId, @movieId, 'Actor');";
				SqlCommand cmd = new SqlCommand(queryString, dbConn);
				cmd.Parameters.AddWithValue("@peopleId", peopleId);
				cmd.Parameters.AddWithValue("@movieId", mov.Id);
				ExecuteCmd(cmd, dbConn);
            }

            foreach(string genre in mov.Genre)
            {
                queryString = 
                    "INSERT INTO dbo.Genre (Genre, MovieId) " +
                    "VALUES(@genre, @movieId);";
				SqlCommand cmd = new SqlCommand(queryString, dbConn);
				cmd.Parameters.AddWithValue("@genre", genre);
				cmd.Parameters.AddWithValue("@movieId", mov.Id);
				ExecuteCmd(cmd, dbConn);
            }

            return RedirectToAction("Index");
        }

        public ActionResult Details(int id)
        {
            Movie mov = GetMovie(id);
            AddPeople(mov);
            AddGenre(mov);
            return View(mov);
        }

        public ActionResult Delete(Movie mov)
        {
            var dbConn = dbConnection();

            string queryString = "DELETE FROM dbo.MovPeople WHERE MovieId = @movieId;";
            SqlCommand cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@movieId", mov.Id);
			ExecuteCmd(cmd, dbConn);

            queryString = "DELETE FROM dbo.Genre WHERE MovieId = @movieId;";
            SqlCommand cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@movieId", mov.Id);
			ExecuteCmd(cmd, dbConn);

            queryString = "DELETE FROM dbo.Movie WHERE Id = @movieId;";
            SqlCommand cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@movieId", mov.Id);
			ExecuteCmd(cmd, dbConn);

            return RedirectToAction("Index");
        }

        private IList<Movie> GetMovieList()
        {
            List<Movie> movieList = BuildMovieList();
            foreach(Movie mov in movieList)
            {
                AddPeople(mov);
                AddGenre(mov);
                AddImage(mov);
            }

            return movieList;
        }


        private List<Movie> BuildMovieList()
        {
            var dbConn = dbConnection();
            string queryString = "Select Id FROM dbo.Movie";

            SqlCommand cmd = new SqlCommand(queryString, dbConn);
            dbConn.Open();

            List<Movie> movieList = new List<Movie>();
            try
            {
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    movieList.Add(GetMovie(reader.GetInt32(0)));
                }
                reader.Close();
            } catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error: "+ e.Message);
            }
            cmd.Dispose();
            dbConn.Close();

            return movieList;
        }

        private Movie GetMovie(int id)
        {
            Movie movie = new Movie();
            var dbConn = dbConnection();

            string queryString = "SELECT Id, Title, Year, Rating FROM dbo.Movie WHERE Id = @id;";
            SqlCommand cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@movieId", id);

            dbConn.Open();

            try
            {
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    movie.Id = reader.GetInt32(0);
                    movie.Title = reader.GetString(1);
                    movie.Year = reader.GetInt32(2);
                    movie.Rating = reader.GetInt32(3);
                }
                reader.Close();
            } catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error: "+ e.Message);
            }
            cmd.Dispose();
            dbConn.Close();

            return movie;
        }

        private void AddPeople(Movie mov) 
        {
            var dbConn = dbConnection();

            string queryString =
                "SELECT " +
                    "p.Name, mp.Job " +
                "FROM " +
                    "dbo.MovPeople AS mp " +
                "INNER JOIN " +
                    "dbo.People AS p ON mp.PeopleId = p.Id " +
                "WHERE " +
                    "mp.MovieId = @movieId;";

            SqlCommand cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@movieId", mov.Id);

            dbConn.Open();

            try
            {
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.GetString(1) == "Director")
                    {
                        mov.Director = new People(reader.GetString(0));
                    } else if (reader.GetString(1) == "Actor")
                    {
                        mov.Cast.Add(new People(reader.GetString(0)));
                    }
                }
                reader.Close();
            } catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error: "+ e.Message);
            }
            cmd.Dispose();
            dbConn.Close();
        }

        private void AddGenre(Movie mov)
        {
            var dbConn = dbConnection();

            string queryString = "SELECT Genre FROM dbo.Genre WHERE MovieId = @movieId;";
            SqlCommand cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@movieId", mov.Id);

            dbConn.Open();

            try
            {
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    mov.Genre.Add(reader.GetString(0));
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

        private void AddImage(Movie mov)
        {
            var dbConn = dbConnection();
            string queryString = 
                "SELECT " +
                    "Name, Type, Image " +
                "FROM " +
                    "dbo.Movie AS m " +
                "INNER JOIN " +
                    "dbo.Image AS i ON m.ImageId = i.Id " +
                "WHERE " +
                    "m.id = @movieId;";

            SqlCommand cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@movieId", mov.Id);

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

                    mov.MovImage = image;
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
    }
}
