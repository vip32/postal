using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Postal;

namespace Mvc2Sample.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewData["Message"] = "Welcome to ASP.NET MVC!";

            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Send()
        {
            dynamic email = new Email("Test");
            email.To = "test@test.com";
            email.From = "test@test.com";
            email.Subject = "Test";
            email.Message = "Hello, World!";
            email.Send();

            return RedirectToAction("Sent");
        }

        public ActionResult Sent()
        {
            return View();
        }
    }
}
