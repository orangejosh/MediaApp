using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace MediaApp.Models
{
    public class People
    {
        [Required]
        public string Name { get; set; }
        public List<Movie> Movies { get; set; }
        public HttpPostedFileBase Image { get; set; }

        public People()
        {
            Movies = new List<Movie>();
        }

        public People(string name)
        {
            Name = name;
            Movies = new List<Movie>();
        }
    }
}