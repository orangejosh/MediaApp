﻿using System;
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
        public ActionResult Index(int index, string job)
        {
            ViewBag.Index = index;
            ViewBag.Job = job;

            return View(GetPeopleList(index, job));
        }

        public ActionResult Create(string job, int index)
        {
            ViewBag.Job = job;
            ViewBag.Index = index;

            return View();
        }

        [HttpPost]
        public ActionResult Create(People person)
        {
            var dbConn = dbConnection();

            string queryString = "INSERT INTO dbo.People(Name) VALUES (@name);";
			SqlCommand cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@name", person.Name);
			ExecuteCmd(cmd, dbConn);

            queryString = "SELECT Id FROM dbo.People WHERE Name = @name;";
			cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@name", person.Name);
			person.Id = GetId(cmd, dbConn);

            UpdateMovies(person, dbConn);
            UpdateImage(person, dbConn);

            return RedirectToAction("Index", new { job = person.Job, index = person.Index });
        }

        public ActionResult Edit(int id, string job, int index)
        {
            People person = GetPerson(id, job);

            ViewBag.Index = index;
            ViewBag.Job = job;

            AddImage(person);

            return View(person);
        }

        [HttpPost]
        public ActionResult Edit(People person)
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
			cmd.Parameters.AddWithValue("@name", person.Name);
			cmd.Parameters.AddWithValue("@peopleId", person.Id);
			ExecuteCmd(cmd, dbConn);

            UpdateMovies(person, dbConn);
            UpdateImage(person, dbConn);

            return RedirectToAction("Index", new { index = person.Index, job = person.Job });
        }

        public ActionResult Delete(People person)
        {
            var dbConn = dbConnection();

            string queryString = "SELECT ImageId FROM dbo.People WHERE Id = @peopleId;";
            SqlCommand cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@peopleId", person.Id);
            int imgId = GetId(cmd, dbConn);

            queryString = "DELETE FROM dbo.Image WHERE Id = @imgId;";
            cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@imgId", imgId);
			ExecuteCmd(cmd, dbConn);

            queryString = "DELETE FROM dbo.Directors WHERE PeopleId = @peopleId;";
            cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@peopleId", person.Id);
			ExecuteCmd(cmd, dbConn);

            queryString = "DELETE FROM dbo.Actors WHERE PeopleId = @peopleId;";
            cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@peopleId", person.Id);
			ExecuteCmd(cmd, dbConn);

            queryString = "DELETE FROM dbo.People WHERE Id = @peopleId;";
            cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@peopleId", person.Id);
			ExecuteCmd(cmd, dbConn);

            return RedirectToAction("Index", new { index = person.Index, job = person.Job });
        }

        // TODO make this more efficent.
        private List<People> GetPeopleList(int index, string job)
        {
            List<People> peopleList = new List<People>();
            var dbConn = dbConnection();

            string queryString =
                "SELECT " +
                    "p.Id, p.Name " +
                "FROM " +
                    "dbo.People AS p " +
                "INNER JOIN " +
                    "dbo." + job + " AS j " +
                "ON " +
                    "p.Id = j.PeopleId " +
                "GROUP BY " +
                    "p.Id, p.Name " +
                "ORDER BY " +
                    "p.Name ASC;"; 
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
                        People person = GetPerson(reader.GetInt32(0), job);
                        peopleList.Add(person);
                    }
                    i++;
                }
                reader.Close();
            } 
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error PeopleController: "+ e.Message);
            }
            finally
            {
                cmd.Dispose();
                dbConn.Close();
            }

            return peopleList;
        }

        private People GetPerson(int id, string job)
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
                System.Diagnostics.Debug.WriteLine("Error PeopleController: "+ e.Message);
            }
            finally
            {
                cmd.Dispose();
                dbConn.Close();
            }

            person.Movies = GetMovieList(id, job);
            AddImage(person);

            return person;
        }

        private List<Movie> GetMovieList(int id, string job)
        {
            List<Movie> movieList = new List<Movie>();
            var dbConn = dbConnection();

            string queryString =
                "SELECT " +
                    "m.Id, m.Title, m.Year " +
                "FROM " +
                    "dbo.Movie AS m " +
                "INNER JOIN " +
                    "dbo." + job + " AS j ON m.Id = j.MovieId " +
                "INNER JOIN " +
                    "dbo.People AS P ON j.PeopleId = p.Id " +
                "WHERE " +
                    "p.Id = @id;";
            SqlCommand cmd = new SqlCommand(queryString, dbConn);
			cmd.Parameters.AddWithValue("@job", job);
			cmd.Parameters.AddWithValue("@id", id);

            try
            {
                dbConn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Movie movie;
                    if (reader.IsDBNull(2))
                    {
                        movie = new Movie(reader.GetInt32(0), reader.GetString(1));
                    }
                    else
                    {
                        movie = new Movie(reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2));
                    }
                    movieList.Add(movie);
                }
            } 
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error PeopleController:"+ e.Message);
            }
            finally
            {
                cmd.Dispose();
                dbConn.Close();
            }

            return movieList;
        }

        public void AddImage(People person)
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
                    if (!reader.IsDBNull(0))
                    {
                        Image image = new Image();
                        image.Name = reader.GetString(0);
                        image.Type = reader.GetString(1);

                        byte[] imageData = (byte[])reader[2];
                        image.Data = Convert.ToBase64String(imageData);

                        person.PeopleImage = image;
                    }
                    else if (!reader.IsDBNull(3))
                    {
                        person.imgURL = reader.GetString(3);
                    }
                }
                reader.Close();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error PeopleController: " + e.Message);
            }
            finally
            {
                cmd.Dispose();
                dbConn.Close();
            }
        }

        public void UpdateMovies(People person, SqlConnection dbConn)
        {
            string queryString = "DELETE FROM dbo." + person.Job + " WHERE PeopleId = @personId;";
            SqlCommand cmd = new SqlCommand(queryString, dbConn);
            cmd.Parameters.AddWithValue("@personId", person.Id);
            ExecuteCmd(cmd, dbConn);

            foreach (Movie movie in person.Movies)
            {
                if (movie.Title != null && movie.Title.Length > 0)
                {
                    queryString = "INSERT INTO dbo.Movie(Title) VALUES(@title);";
                    cmd = new SqlCommand(queryString, dbConn);
                    cmd.Parameters.AddWithValue("@title", movie.Title);
                    ExecuteCmd(cmd, dbConn);

                    queryString = "SELECT Id FROM dbo.Movie WHERE Title = @title;";
                    cmd = new SqlCommand(queryString, dbConn);
                    cmd.Parameters.AddWithValue("@title", movie.Title);
                    int movId = GetId(cmd, dbConn);

                    queryString = "INSERT INTO dbo." + person.Job + " (PeopleId, MovieId) VALUES (@peopleId, @movieId);";
                    cmd = new SqlCommand(queryString, dbConn);
                    cmd.Parameters.AddWithValue("@peopleId", person.Id);
                    cmd.Parameters.AddWithValue("@movieId", movId);
                    ExecuteCmd(cmd, dbConn);
                }
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
                int newImageId = GetId(cmd, dbConn);

                UpdateOldImage(dbConn, newImageId, person.Id);
                reader.Dispose();
            }
            else if(person.imgURL != null)
            {
                string queryString = "INSERT INTO dbo.Image(Url) VALUES(@Url)";
                SqlCommand cmd = new SqlCommand(queryString, dbConn);
                cmd.Parameters.AddWithValue("@Url", person.imgURL);
                ExecuteCmd(cmd, dbConn);

                queryString = "SELECT Id FROM dbo.Image WHERE Url = @Url;";
                cmd = new SqlCommand(queryString, dbConn);
                cmd.Parameters.AddWithValue("@Url", person.imgURL);
                int newImageId = GetId(cmd, dbConn);

                UpdateOldImage(dbConn, newImageId, person.Id);
            }
        }
        public void UpdateOldImage(SqlConnection dbConn, int newImageId, int personId)
        {
            string queryString = "SELECT ImageId FROM dbo.People WHERE Id = @id;";
            SqlCommand cmd = new SqlCommand(queryString, dbConn);
            cmd.Parameters.AddWithValue("@id", personId);
            int oldImgId = GetId(cmd, dbConn);
            
            queryString = "UPDATE dbo.People SET ImageId = @imageId WHERE Id = @peopleId;";
            cmd = new SqlCommand(queryString, dbConn);
            cmd.Parameters.AddWithValue("@imageId", newImageId);
            cmd.Parameters.AddWithValue("@peopleId", personId);
            ExecuteCmd(cmd, dbConn);

            queryString = "DELETE FROM dbo.Image WHERE Id = @imgId;";
            cmd = new SqlCommand(queryString, dbConn);
            cmd.Parameters.AddWithValue("@imgId", oldImgId);
            ExecuteCmd(cmd, dbConn);
        }
    }
}
