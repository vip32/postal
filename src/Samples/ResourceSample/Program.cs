
using System;
using Postal;
using System.Web.Mvc;

namespace ResourceSample
{
    /*
    Before running this sample, please start the SMTP development server,
    found in the Postal code directory: tools\smtp4dev.exe

    Use the SMTP development server to inspect the contents of generated email (headers, content, etc).
    No email is really sent, so it's perfect for debugging.
    */

    class Program
    {
        static void Main()
        {
            var engines = new ViewEngineCollection
                          {
                              new ResourceRazorViewEngine(typeof(Program).Assembly, @"ResourceSample.Resources.Views")
                          };

            var service = new EmailService(engines);

            dynamic email = new Email("Test");
            email.To = "test1@test.com";
            email.Message = "Hello, non-asp.net world!";

            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(email));

            service.Send(email);
        }
    }

}
