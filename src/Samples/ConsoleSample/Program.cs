﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using Newtonsoft.Json;
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
            // create the email template
            var template = new EmailTemplate("Test");
            dynamic dtemplate = template;
            dtemplate.To = "test1@test.com";
            dtemplate.Message = "Hello, non-asp.net world! ";
            dtemplate.Test = "test property";
            dtemplate.SubjectSuffix = DateTime.Now.ToShortTimeString();
            dtemplate.Values = new[] { "one", "two", "three" };
            template.Attach(@"c:\tmp\test2.log");
            template.Attach(@"c:\tmp\cat.jpg", "CatImageId"); // TODO: put in progam folder

            // serialize and deserialize the email template
            var serializerSettings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    new AttachmentReadConverter(true)
                }
            };
            var serializedTemplate = JsonConvert.SerializeObject(template, Formatting.Indented, serializerSettings);
            Console.WriteLine(serializedTemplate);
            Console.WriteLine("serialized size: {0}", serializedTemplate.Length);
            var deserializedTemplate = JsonConvert.DeserializeObject<EmailTemplate>(serializedTemplate, serializerSettings);

            // send the email template
            var engines = new ViewEngineCollection
            {
                new FileSystemRazorViewEngine(Path.GetFullPath(@"..\..\Views"))
            };
            var service = new SmtpMessagingService(engines);
            service.Send(deserializedTemplate);
        }
    }

}
