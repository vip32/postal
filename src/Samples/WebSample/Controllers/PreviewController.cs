using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Postal;

namespace WebSample.Controllers
{
    public class PreviewController : Controller
    {
        public ActionResult Simple()
        {
            dynamic email = new EmailTemplate("Simple");
            email.Date = DateTime.UtcNow.ToString();
            
            return new TemplateViewResult(email);
        }

        public ActionResult SimpleHtml()
        {
            dynamic email = new EmailTemplate("SimpleHtml");
            email.Date = DateTime.UtcNow.ToString();

            return new TemplateViewResult(email);
        }
        
        public ActionResult MultiPart()
        {
            dynamic email = new EmailTemplate("MultiPart");
            email.Date = DateTime.UtcNow.ToString();
            
            return new TemplateViewResult(email);
        }

        public ActionResult Typed()
        {
            var email = new TypedTemplate();
            email.Date = DateTime.UtcNow.ToString();
            
            return new TemplateViewResult(email);
        }
    }
}