using System;
using System.Web.Mvc;
using Postal;

namespace WebSample.Controllers
{
    public class EmailController : Controller
    {
        [HttpPost]
        public ActionResult SendSimple()
        {
            dynamic email = new EmailTemplate("Simple");
            email.Date = DateTime.UtcNow.ToString();
            email.Send();

            return RedirectToAction("Sent", "Home");
        }

        [HttpPost]
        public ActionResult SendMultiPart()
        {
            dynamic email = new EmailTemplate("MultiPart");
            email.Date = DateTime.UtcNow.ToString();
            email.Send();

            return RedirectToAction("Sent", "Home");
        }
    }

    public class TypedTemplate : Template
    {
        public string Date { get; set; }
        public override string ViewName { get; set; }
    }
}
