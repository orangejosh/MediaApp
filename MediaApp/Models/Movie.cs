using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace MediaApp.Models
{
    public class Movie
    {
        public Movie()
        {
            Cast = new List<People>();
            Genre = new List<string>();
        }

        public Movie(string title, int year)
        {
            Title = title;
            Year = year;
        }

        public Movie(int id, string title, int year, int rating)
        {
            Id = id;
            Title = title;
            Year = year;
            Cast = new List<People>();
            Genre = new List<string>();
            Rating = rating;
        }
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public People Director { get; set; }
        public List<People> Cast { get; set; }
        public List<People> Crew { get; set; }
        public List<string> Genre { get; set; }
        [Required]
        public int Year { get; set; }
        public int Rating { get; set; }
        public HttpPostedFileBase ImageInput { get; set; }
        public Image MovImage { get; set; }
    }
}