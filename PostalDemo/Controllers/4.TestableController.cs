using System;
using System.Web.Mvc;
using Postal;
using System.Net.Mail;

namespace PostalDemo.Controllers
{
    public class TestableController : HelpControllerBase
    {
        public TestableController(IEmailService emailService)
        {
            this.emailService = emailService;
        }

        readonly IEmailService emailService;

        public ActionResult Send(HelpRequest form)
        {
            var ticket = Guid.NewGuid();

            dynamic email = new Email("HelpRequest");
            email.UserEmailAddress = form.EmailAddress;
            email.Name = form.Name;
            email.Message = form.Message;
            email.TicketId = ticket;

            // email.Send();
            emailService.Send(email);

            return RedirectToAction("sent");
        }

        // HACK: Do not do this in real life.
        // Use an IoC container instead!
        public TestableController()
            : this(new EmailService())
        {
        }
    }
}
