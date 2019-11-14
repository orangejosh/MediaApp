using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Web.Hosting;

namespace MediaApp.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            List<string> fileNames = new List<string>();
            string[] files = Directory.GetFiles(HostingEnvironment.MapPath("~/Content/Images/TitleSlide"));

            foreach(string file in files)
            {
                fileNames.Add(Path.GetFileName(file));
            }

            var rnd = new Random();
            var result = fileNames.OrderBy(item => rnd.Next());
            
            return View(result);
        }
    }
}