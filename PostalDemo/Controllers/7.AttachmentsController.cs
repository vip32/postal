using System;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Postal;

namespace PostalDemo.Controllers
{
    public class AttachmentsController : HelpControllerBase
    {
        public ActionResult Send(HelpRequest form, HttpPostedFileBase screenshot)
        {
            var ticket = Guid.NewGuid();

            dynamic email = new Email("HelpRequest");
            email.UserEmailAddress = form.EmailAddress;
            email.Name = form.Name;
            email.Message = form.Message;
            email.TicketId = ticket;

            email.Attach(new Attachment(screenshot.InputStream, screenshot.FileName));

            Task task = email.SendAsync();
            // Perhaps then combine with other Tasks and await completion?
            
            return RedirectToAction("sent");
        }

        #region The old async way - it's not nice code to read or write.

        public ActionResult OldSend()
        {
            var smtp = new SmtpClient();
            smtp.SendCompleted += (s, e) =>
            {
                if (!e.Cancelled && e.Error == null)
                {
                    // Do something now that the email was sent...
                }
                smtp.Dispose();
            };

            var message = new MailMessage();
            // build your message here...

            smtp.SendAsync(message, null);

            // Hmm, probably need a try-catch around everything to Dispose if something fails
            // before SendCompleted gets called?

            return RedirectToAction("sent");
        }

        #endregion

    }
}
