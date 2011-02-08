using System.Web.Mvc;

namespace PostalDemo.Controllers
{
    public abstract class HelpControllerBase : Controller
    {
        public ActionResult Index()
        {
            return View("Help");
        }

        public ActionResult Sent()
        {
            return View();
        }
    }
    
    public class HelpRequest
    {
        public string EmailAddress { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
    }
}
