using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PostalDemo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Intro()
        {
            return View();
        }

        public ActionResult WrapUp()
        {
            return View();
        }

        public ActionResult Tips()
        {
            return View();
        } 
    }
}
