using System;
using System.Web.Mvc;
using Postal;

namespace PostalDemo.Controllers
{
    public class BasicController : HelpControllerBase
    {
        public ActionResult Send(HelpRequest form)
        {
            // TODO: Create a support ticket in a database ...
            var ticket = Guid.NewGuid();

            dynamic email = new Email("HelpRequest");
            // Convention: view is located at "~\Views\Emails\HelpRequest.cshtml"

            // Treat the email object as a 'view bag'.
            // Add anything you want to use in the view
            email.UserEmailAddress = form.EmailAddress;
            email.Name = form.Name;
            email.Message = form.Message;
            email.TicketId = ticket;

            email.Send();
            // That one line of code...
            //  * Renders the view
            //  * Parses the output
            //  * Creates a System.Net.Mail.MailMessage
            //  * Creates a System.Net.Mail.SmtpClient
            //  * Sends the MailMessage using the SmtpClient
            // Nifty!

            return RedirectToAction("sent");
        }
    }
}
