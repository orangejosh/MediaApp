using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MediaApp.Models;
using System.Data.SqlClient;

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
            string queryString = "INSERT INTO dbo.Movie(Title, Year, Rating) VALUES ('" + mov.Title + "', " + mov.Year + ", " + mov.Rating + ")";
            ExecuteSqlString(dbConn, queryString);

            queryString = "SELECT Id FROM dbo.Movie WHERE Title = '" + mov.Title + "' AND Year = " + mov.Year + ";";
            var movieId = GetId(queryString, dbConn);

            // Create Director
            if (mov.Director.Name.Length > 0)
            {
                queryString = "INSERT INTO dbo.People(Name) VALUES ('" + mov.Director.Name + "');";
                ExecuteSqlString(dbConn, queryString);

                queryString = "Select Id FROM dbo.People WHERE Name = '" + mov.Director.Name + "';";
                var peopleId = GetId(queryString, dbConn);

                queryString = "INSERT INTO dbo.MovPeople(PeopleId, MovieId, Job) Values(" + peopleId + ", " + movieId + ", 'Director');";
                ExecuteSqlString(dbConn, queryString);
            }

            // Create Actors
            foreach (People actor in mov.Cast)
            {
                if (actor.Name.Length > 0)
                {
                    queryString = "INSERT INTO dbo.People(Name) VALUES ('" + actor.Name + "');";
                    ExecuteSqlString(dbConn, queryString);

                    queryString = "Select Id FROM dbo.People WHERE Name = '" + actor.Name + "';";
                    int peopleId = GetId(queryString, dbConn);

                    queryString = "INSERT INTO dbo.MovPeople(PeopleId, MovieId, Job) Values(" + peopleId + ", " + movieId + ", 'Actor');";
                    ExecuteSqlString(dbConn, queryString);
                }
            }

            // Create Genres
            foreach (string genre in mov.Genre)
            {
                if (genre.Length > 0)
                {
                    queryString = "INSERT INTO dbo.Genre(MovieId, Genre) VALUES(" + movieId + ", '" + genre + "');";
                    ExecuteSqlString(dbConn, queryString);
                } 
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
                    "Id = " + mov.Id;
            ExecuteSqlString(dbConn, queryString);

            queryString = "DELETE FROM dbo.MovPeople WHERE MovieId = " + mov.Id + ";";
            ExecuteSqlString(dbConn, queryString);

            queryString = "DELETE FROM dbo.Genre WHERE MovieId = " + mov.Id + ";";
            ExecuteSqlString(dbConn, queryString);

            queryString = "INSERT INTO dbo.People(Name) VALUES('" + mov.Director.Name + "');";
            ExecuteSqlString(dbConn, queryString);

            queryString = "SELECT Id FROM dbo.People WHERE Name = '" + mov.Director.Name+ "';";
            int peopleId = GetId(queryString, dbConn);

            queryString = 
                "INSERT INTO dbo.MovPeople(PeopleId, MovieId, Job) " +
                "VALUES(" + peopleId + ", " + mov.Id + ", 'Director');";
            ExecuteSqlString(dbConn, queryString);


            foreach(People actor in mov.Cast)
            {
                queryString = "INSERT INTO dbo.People(Name) VALUES('" + actor.Name + "');";
                ExecuteSqlString(dbConn, queryString);

                queryString = "SELECT Id FROM dbo.People WHERE Name = '" + actor.Name + "';";
                peopleId = GetId(queryString, dbConn);

                queryString = 
                    "INSERT INTO dbo.MovPeople(PeopleId, MovieId, Job) " +
                    "VALUES(" + peopleId + ", " + mov.Id + ", 'Actor');";
                ExecuteSqlString(dbConn, queryString);
            }

            foreach(string genre in mov.Genre)
            {
                queryString = 
                    "INSERT INTO dbo.Genre (Genre, MovieId) " +
                    "VALUES('" + genre + "', " + mov.Id + ");";
                ExecuteSqlString(dbConn, queryString);
            }

            return RedirectToAction("Index");
        }

        public ActionResult Delete(Movie mov)
        {
            var dbConn = dbConnection();

            string queryString = "DELETE FROM dbo.MovPeople WHERE MovieId = " + mov.Id + ";";
            ExecuteSqlString(dbConn, queryString);

            queryString = "DELETE FROM dbo.Genre WHERE MovieId = " + mov.Id + ";";
            ExecuteSqlString(dbConn, queryString);

            queryString = "DELETE FROM dbo.Movie WHERE Id = " + mov.Id + ";";
            ExecuteSqlString(dbConn, queryString);

            return RedirectToAction("Index");
        }

        private IList<Movie> GetMovieList()
        {
            List<Movie> movieList = BuildMovieList();
            foreach(Movie mov in movieList)
            {
                AddPeople(mov);
                AddGenre(mov);
            }

            return movieList;
        }


        private List<Movie> BuildMovieList()
        {
            var dbConn = dbConnection();
            string queryString = "Select Id FROM dbo.Movie";

            SqlCommand command = new SqlCommand(queryString, dbConn);
            dbConn.Open();

            List<Movie> movieList = new List<Movie>();
            try
            {
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    movieList.Add(GetMovie(reader.GetInt32(0)));
                }
                reader.Close();
            } catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error: "+ e.Message);
            }
            command.Dispose();
            dbConn.Close();

            return movieList;
        }

        private Movie GetMovie(int id)
        {
            Movie movie = new Movie();
            var dbConn = dbConnection();
            string queryString = "SELECT Id, Title, Year, Rating FROM dbo.Movie WHERE Id = " + id;

            SqlCommand command = new SqlCommand(queryString, dbConn);
            dbConn.Open();

            try
            {
                SqlDataReader reader = command.ExecuteReader();
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
            command.Dispose();
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
                    "mp.MovieId = " + mov.Id + ";";

            SqlCommand command = new SqlCommand(queryString, dbConn);
            dbConn.Open();

            try
            {
                SqlDataReader reader = command.ExecuteReader();
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
            command.Dispose();
            dbConn.Close();
        }

        private void AddGenre(Movie mov)
        {
            var dbConn = dbConnection();
            string queryString = "SELECT Genre FROM dbo.Genre WHERE MovieId = " + mov.Id + ";";

            SqlCommand command = new SqlCommand(queryString, dbConn);
            dbConn.Open();

            try
            {
                SqlDataReader reader = command.ExecuteReader();
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
            command.Dispose();
            dbConn.Close();
        }
    }
}