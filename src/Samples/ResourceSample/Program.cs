

using System;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Web.Mvc;
using Newtonsoft.Json;
using Postal;

namespace ResourceSample
{
    /*
    Before running this sample, please start the SMTP development server,
    found in the Postal code directory: tools\smtp4dev.exe

    Use the SMTP development server to inspect the contents of generated email (headers, content, etc).
    No email is really sent, so it's perfect for debugging.
    */

    internal class Program
    {
        private static void Main()
        {
            var engines = new ViewEngineCollection
            {
                new ResourceRazorViewEngine(typeof (Program).Assembly, @"ResourceSample.Resources.Views")
            };

            var service = new EmailService(engines);

            dynamic email = new Email("Test");
            email.To = "test1@test.com";
            email.Message = "Hello, non-asp.net world!";

            using (var memoryStream = new MemoryStream())
            {
                var contentAsBytes = File.ReadAllBytes(@"c:\tmp\test2.log");
                    //Encoding.UTF8.GetBytes(@"c:\tmp\test2.log");
                memoryStream.Write(contentAsBytes, 0, contentAsBytes.Length);
                memoryStream.Seek(0, SeekOrigin.Begin);

                var attachment = new Attachment(memoryStream, "log.txt", MediaTypeNames.Application.Octet);
                email.Attach(attachment);

                Console.WriteLine(JsonConvert.SerializeObject(email, Formatting.Indented,
                    new MemoryStreamJsonConverter()));

                service.Send(email);
            }
        }
    }
}
