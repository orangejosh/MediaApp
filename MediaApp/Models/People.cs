﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace MediaApp.Models
{
    public class People
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public List<Movie> Movies { get; set; }
        [Display(Name = "Image")]
        public HttpPostedFileBase ImageInput { get; set; }
        [Display(Name = "Image")]
        public Image PeopleImage { get; set; }
        public string imgURL { get; set; }
        public int Index { get; set; }
        public string Job { get; set; }

        public People()
        {
            Movies = new List<Movie>();
        }
        
        public People(string name)
        {
            Name = name;
            Movies = new List<Movie>();
        }

        public People(int id, string name)
        {
            Id = id;
            Name = name;
            Movies = new List<Movie>();
        }
    }
}