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
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public People Director { get; set; }
        public List<People> Cast { get; set; }
        public List<People> Crew { get; set; }
        public List<string> Genre { get; set; }
        [Required]
        public int Year { get; set; }
        public int? Rating { get; set; }
        public string Synopsis { get; set; }
        [Display(Name = "Image")]
        public HttpPostedFileBase ImageInput { get; set; }
        [Display(Name = "Image")]
        public Image MovImage { get; set; }
        public string imgURL { get; set; }
        public string Order { get; set; }
        public int Index { get; set; }

        public Movie()
        {
            Cast = new List<People>();
            Genre = new List<string>();
        }

        public Movie(int id, string title)
        {
            Id = id;
            Title = title;
            Cast = new List<People>();
            Genre = new List<string>();
        }

        public Movie(int id, string title, int year)
        {
            Id = id;
            Title = title;
            Year = year;
            Cast = new List<People>();
            Genre = new List<string>();
        }
    }
}