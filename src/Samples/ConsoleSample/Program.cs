using System.IO;
using System.Web.Mvc;
using Postal;

namespace ConsoleSample
{
    /*
    Before running this sample, please start the SMTP development server,
    found in the Postal code directory: tools\smtp4dev.exe

    Use the SMTP development server to inspect the contents of generated email (headers, content, etc).
    No email is really sent, so it's perfect for debugging.
    */

    class Program // That's right, no asp.net runtime required!
    {
        static void Main(string[] args)
        {
            // Get the path to the directory containing views
            var viewsPath = Path.GetFullPath(@"..\..\Views");

            var engines = new ViewEngineCollection();
            engines.Add(new FileSystemRazorViewEngine(viewsPath));

            var service = new SmtpMessagingService(engines);

            dynamic email = new EmailTemplate("Test");
            email.Message = "Hello, non-asp.net world!";
            service.Send(email);

            // Alternatively, set the service factory like this:
            /*
            Template.CreateEmailService = () => new SmtpMessagingService(engines);

            dynamic email = new Template("Test");
            email.Message = "Hello, non-asp.net world!";
            email.Send();
            */
        }
    }

}
