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
        public ActionResult Index(string order, int index)
        {
//            AddMovieData();
            ViewBag.Order = order;
            ViewBag.Index = index;
            return View(GetMovieList(order, index));
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Movie mov)
        {
            var dbConn = dbConnection();

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

            UpdateDirector(mov, dbConn);
            UpdateActor(mov, dbConn);
            UpdateGenre(mov, dbConn);
            UpdateRating(mov, dbConn);
            UpdateImage(mov, dbConn);

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            Movie mov = GetMovie(id);
            AddPeople(mov);
            AddGenre(mov);
            AddImage(mov);

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
                    "Title = @movieTitle, @year " +
                "WHERE " +
                    "Id = @movieId;";
			SqlCommand cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@movieTitle", mov.Title);
			cmd.Parameters.AddWithValue("@year", mov.Year);
			cmd.Parameters.AddWithValue("@movieId", mov.Id);
			ExecuteCmd(cmd, dbConn);

            UpdateDirector(mov, dbConn);
            UpdateActor(mov, dbConn);
            UpdateGenre(mov, dbConn);
            UpdateRating(mov, dbConn);
            UpdateImage(mov, dbConn);

            return RedirectToAction("Index");
        }

        public ActionResult Details(int id)
        {
            Movie mov = GetMovie(id);
            AddPeople(mov);
            AddGenre(mov);
            AddImage(mov);

            return View(mov);
        }

        public ActionResult Delete(Movie mov)
        {
            var dbConn = dbConnection();

            string queryString = "DELETE FROM dbo.Directors WHERE MovieId = @movieId;";
            SqlCommand cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@movieId", mov.Id);
			ExecuteCmd(cmd, dbConn);

            queryString = "DELETE FROM dbo.Actors WHERE MovieId = @movieId;";
            cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@movieId", mov.Id);
			ExecuteCmd(cmd, dbConn);

            queryString = "DELETE FROM dbo.Genre WHERE MovieId = @movieId;";
            cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@movieId", mov.Id);
			ExecuteCmd(cmd, dbConn);

            queryString = "DELETE FROM dbo.Image WHERE MovieId = @movieId;";
            cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@movieId", mov.Id);
			ExecuteCmd(cmd, dbConn);

            queryString = "DELETE FROM dbo.Movie WHERE Id = @movieId;";
            cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@movieId", mov.Id);
			ExecuteCmd(cmd, dbConn);

            return RedirectToAction("Index");
        }

        private IList<Movie> GetMovieList(string order, int index)
        {
            List<Movie> movieList = BuildMovieList(order, index);
            foreach(Movie mov in movieList)
            {
                AddPeople(mov);
                AddGenre(mov);
                AddImage(mov);
            }

            return movieList;
        }

        private List<Movie> BuildMovieList(string order, int index)
        {
            string ascDec = order == "Title" ? "ASC" : "DESC";
            var dbConn = dbConnection();
            string queryString = "Select Id FROM dbo.Movie ORDER BY " + order + " " + ascDec;
            SqlCommand cmd = new SqlCommand(queryString, dbConn);

            List<Movie> movieList = new List<Movie>();
            try
            {
                dbConn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                int i = 0;
                while (reader.Read() && i < index + 50)
                {
                    if (i >= index)
                    {
                        movieList.Add(GetMovie(reader.GetInt32(0)));
                    }
                    i++;
                }
                reader.Close();
            } 
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error MovieController: "+ e.Message);
            }
            finally
            {
                cmd.Dispose();
                dbConn.Close();
            }

            return movieList;
        }

        private Movie GetMovie(int id)
        {
            Movie movie = new Movie();
            var dbConn = dbConnection();

            string queryString = "SELECT Id, Title, Year, Rating, Synopsis FROM dbo.Movie WHERE Id = " + id + ";";
            SqlCommand cmd = new SqlCommand(queryString, dbConn);

            try
            {
                dbConn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    movie.Id = reader.GetInt32(0);
                    movie.Title = reader.GetString(1);
                    movie.Year = reader.GetInt32(2);
                    movie.Rating = reader.GetInt32(3);
                    movie.Synopsis = reader.GetString(4);
                }
                reader.Close();
            } 
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error MovieController: "+ e.Message);
            }
            finally
            {
                cmd.Dispose();
                dbConn.Close();
            }

            return movie;
        }

        private void AddPeople(Movie mov) 
        {
            string queryString =
                "SELECT " +
                    "p.Id, p.Name " +
                "FROM " +
                    "dbo.Directors AS d " +
                "INNER JOIN " +
                    "dbo.People AS p ON d.PeopleId = p.Id " +
                "WHERE " +
                    "d.MovieId = @movieId;";
            QueryPeople(mov, "Directors", queryString);

            queryString =
                "SELECT " +
                    "p.Id, p.Name " +
                "FROM " +
                    "dbo.Actors AS d " +
                "INNER JOIN " +
                    "dbo.People AS p ON d.PeopleId = p.Id " +
                "WHERE " +
                    "d.MovieId = @movieId;";
            QueryPeople(mov, "Actors", queryString);
        }

        private void QueryPeople(Movie mov, string job, string queryString)
        {
            var dbConn = dbConnection();
            SqlCommand cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@movieId", mov.Id);
            try
            {
                dbConn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (job == "Directors")
                    {
                        mov.Director = new People(reader.GetInt32(0), reader.GetString(1));
                    } 
                    else if (job == "Actors")
                    {
                        mov.Cast.Add(new People(reader.GetInt32(0), reader.GetString(1)));
                    }
                }
                reader.Close();
            } 
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error MovieController: "+ e.Message);
            }
            finally
            {
                cmd.Dispose();
                dbConn.Close();
            }
        }


        private void AddGenre(Movie mov)
        {
            var dbConn = dbConnection();

            string queryString = "SELECT Genre FROM dbo.Genre WHERE MovieId = @movieId;";
            SqlCommand cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@movieId", mov.Id);

            try
            {
                dbConn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    mov.Genre.Add(reader.GetString(0));
                }
                reader.Close();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error MovieController: " + e.Message);
            }
            finally
            {
                cmd.Dispose();
                dbConn.Close();
            }
        }

        private void AddImage(Movie mov)
        {
            var dbConn = dbConnection();
            string queryString = 
                "SELECT " +
                    "Name, Type, Image, Url " +
                "FROM " +
                    "dbo.Movie AS m " +
                "INNER JOIN " +
                    "dbo.Image AS i ON m.ImageId = i.Id " +
                "WHERE " +
                    "m.id = @movieId;";

            SqlCommand cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@movieId", mov.Id);

            try
            {
                dbConn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.GetString(3) != null)
                    {
                        mov.imgURL = reader.GetString(3);
                    } 
                    else
                    {
                        Image image = new Image();
                        image.Name = reader.GetString(0);
                        image.Type = reader.GetString(1);

                        byte[] imageData = (byte[])reader[2];
                        image.Data = Convert.ToBase64String(imageData);

                        mov.MovImage = image;
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

        public void UpdateDirector(Movie mov, SqlConnection dbConn)
        {
            string queryString = "DELETE FROM dbo.Directors WHERE MovieId = @movieId;";
            SqlCommand cmd = new SqlCommand(queryString, dbConn);
            cmd.Parameters.AddWithValue("@movieId", mov.Id);
            ExecuteCmd(cmd, dbConn);

            if (mov.Director != null && mov.Director.Name.Length > 0)
            {
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
            }
        }

        public void UpdateActor(Movie mov, SqlConnection dbConn)
        {
            string queryString = "DELETE FROM dbo.Actors WHERE MovieId = @movieId;";
            SqlCommand cmd = new SqlCommand(queryString, dbConn);
            cmd.Parameters.AddWithValue("@movieId", mov.Id);
            ExecuteCmd(cmd, dbConn);

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
                    int peopleId = GetId(cmd, dbConn);

                    queryString = "INSERT INTO dbo.Actors (PeopleId, MovieId) VALUES (@peopleId, @movieId);";
                    cmd = new SqlCommand(queryString, dbConn);
                    cmd.Parameters.AddWithValue("@peopleId", peopleId);
                    cmd.Parameters.AddWithValue("@movieId", mov.Id);
                    ExecuteCmd(cmd, dbConn);
                }
            }
        }

        public void UpdateGenre(Movie mov, SqlConnection dbConn)
        {
            string queryString = "DELETE FROM dbo.Genre WHERE MovieId = @movieId;";
            SqlCommand cmd = new SqlCommand(queryString, dbConn);
            cmd.Parameters.AddWithValue("@movieId", mov.Id);
            ExecuteCmd(cmd, dbConn);

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
        }

        public void UpdateRating(Movie mov, SqlConnection dbConn)
        {
            if (mov.Rating > 0)
            {
                string queryString = "UPDATE dbo.Movie SET Rating = @rating WHERE Id = @movieId;";
                SqlCommand cmd = new SqlCommand(queryString, dbConn);
                cmd.Parameters.AddWithValue("@rating", mov.Rating);
                cmd.Parameters.AddWithValue("@movieId", mov.Id);
                ExecuteCmd(cmd, dbConn);
            }
        }

        public void UpdateSynopsis(Movie mov, SqlConnection dbConn)
        {
            if (mov.Synopsis != null)
            {
                string queryString = "UPDATE dbo.Movie SET Synopsis = @sysnopsis WHERE Id = @movieId;";
                SqlCommand cmd = new SqlCommand(queryString, dbConn);
                cmd.Parameters.AddWithValue("@sysnopsis", mov.Synopsis);
                cmd.Parameters.AddWithValue("@movieId", mov.Id);
                ExecuteCmd(cmd, dbConn);
            }
        }

        public void UpdateImage(Movie mov, SqlConnection dbConn)
        {
            if (mov.ImageInput != null)
            {
                BinaryReader reader = new BinaryReader(mov.ImageInput.InputStream);
                byte[] imageData = reader.ReadBytes(mov.ImageInput.ContentLength);

                string fileName = mov.ImageInput.FileName;
                string type = mov.ImageInput.ContentType;

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

                queryString = "UPDATE dbo.Movie SET ImageId = @imageId WHERE Id = @movieId;";
                cmd = new SqlCommand(queryString, dbConn);
                cmd.Parameters.AddWithValue("@imageId", imageId);
                cmd.Parameters.AddWithValue("@movieId", mov.Id);
                ExecuteCmd(cmd, dbConn);

                reader.Dispose();
            } 
            else if(mov.imgURL != null)
            {
                string queryString = "INSERT INTO dbo.Image(Url) VALUES(@Url)";
                SqlCommand cmd = new SqlCommand(queryString, dbConn);
                cmd.Parameters.AddWithValue("@Url", mov.imgURL);
                ExecuteCmd(cmd, dbConn);

                queryString = "SELECT Id FROM dbo.Image WHERE Url = @Url;";
                cmd = new SqlCommand(queryString, dbConn);
                cmd.Parameters.AddWithValue("@Url", mov.imgURL);
                int imageId = GetId(cmd, dbConn);

                queryString = "UPDATE dbo.Movie SET ImageId = @imageId WHERE Id = @movieId;";
                cmd = new SqlCommand(queryString, dbConn);
                cmd.Parameters.AddWithValue("@imageId", imageId);
                cmd.Parameters.AddWithValue("@movieId", mov.Id);
                ExecuteCmd(cmd, dbConn);

            }
        }

    }
}
