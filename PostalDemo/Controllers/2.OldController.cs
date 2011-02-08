using System;
using System.Net.Mail;
using System.Web.Mvc;

namespace PostalDemo.Controllers
{
    public class OldController : HelpControllerBase
    {
        public ActionResult Send(HelpRequest form)
        {
            // TODO: Create a support ticket in a database ...
            var ticket = Guid.NewGuid();
            var ticketUrl = Url.Action("display", "ticket", new { id = ticket }, "http");

            using (var smtp = new SmtpClient())
            {
                using (var message = new MailMessage())
                {
                    message.From = new MailAddress("help-form@website.com");
                    message.To.Add(new MailAddress("support@website.com"));
                    message.Subject = "Support ticket created #" + ticket;
                    message.Body = string.Format(@"Hello admin,

{0} has requested help with the website.
Message:
{1}

You can contact the user here: {2}

View the support ticket here: {3}

Thanks!", form.Name, form.Message, form.EmailAddress, ticketUrl);

                    smtp.Send(message);
                }
            }

            return RedirectToAction("sent");
        }
    }

}
