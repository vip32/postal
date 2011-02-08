using System;
using System.Web.Mvc;
using Postal;

namespace PostalDemo.Controllers
{
    public class StrongController : HelpControllerBase
    {
        public ActionResult Send(HelpRequest form)
        {
            var ticket = Guid.NewGuid();

            var email = new StrongHelpRequestEmail
            {
                UserEmailAddress = form.EmailAddress,
                Name = form.Name,
                Message = form.Message,
                TicketId = ticket
            };
            email.Send();

            return RedirectToAction("sent");
        }
    }

    public class StrongHelpRequestEmail : Email
    {
        public string UserEmailAddress { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
        public Guid TicketId { get; set; }
    }
}
