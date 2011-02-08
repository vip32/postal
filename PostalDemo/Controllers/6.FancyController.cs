using System;
using System.Web.Mvc;
using Postal;
using System.Net.Mail;
using System.IO;
using System.Net.Mime;

namespace PostalDemo.Controllers
{
    public class FancyController : HelpControllerBase
    {
        #region For the love of Gu, don't write code like this...

        // I won't even dignify this code by running it!
        public ActionResult BadSend(HelpRequest form)
        {
            var ticket = Guid.NewGuid();
            var ticketUrl = Url.Action("display", "ticket", new { id = ticket }, "http");

            using (var smtp = new SmtpClient())
            {
                using (var message = new MailMessage())
                {
                    message.From = new MailAddress("help-form@website.com");
                    message.To.Add(new MailAddress("support@website.com"));
                    message.Subject = "Support ticket created #" + ticket;

                    var textBody = string.Format(@"Hello admin,

{0} has requested help with the website.
Message:
{1}

You can contact the user here: {2}

View the support ticket here: {3}

Thanks!", form.Name, form.Message, form.EmailAddress, ticketUrl);
                    
                    message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(textBody, new ContentType("text/plain")));
                    
                    // But wait, there's more...
                    var htmlBody = string.Format(@"<html>
<body>
    <p>
        Hello admin,</p>
    <p>{0} has requested help with the website.</p>
    <p>
        Message:<br />
        {1}
    </p>
    <p>
        You can contact the user here: {2}
    </p>
    <p>
        <a href=""{3}"">View the support ticket</a>
    </p>
    <p>
        Thanks!</p>
</body>
</html>", form.Name, form.Message, form.EmailAddress, ticketUrl);

                    message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(htmlBody, new ContentType("text/html")));

                    // ...finally!
                    smtp.Send(message);
                }
            }
            return RedirectToAction("sent");
        }

        #endregion

        #region Use Postal instead!

        public ActionResult Send(HelpRequest form)
        {
            var ticket = Guid.NewGuid();

            dynamic email = new Email("Fancy");
            email.UserEmailAddress = form.EmailAddress;
            email.Name = form.Name;
            email.Message = form.Message;
            email.TicketId = ticket;

            email.Send();

            return RedirectToAction("sent");
        }

        #endregion

    }
}
