﻿using System;
using Moq;
using Should;
using Xunit;
using System.Net.Mail;
using System.IO;
using Newtonsoft.Json;

namespace Postal
{
    public class EmailTests
    {
        [Fact]
        public void ViewName_is_set_by_constructor()
        {
            var email = new Email("Test");
            email.ViewName.ShouldEqual("Test");
        }

        [Fact]
        public void Cannot_create_Email_with_null_view_name()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                new Email(null);
            });
        }

        [Fact]
        public void Cannot_create_Email_with_empty_view_name()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                new Email("");
            });
        }

        [Fact]
        public void Dynamic_property_setting_assigns_ViewData_value()
        {
            dynamic email = new Email("Test");
            email.Subject = "SubjectValue";

            var email2 = (Email)email;
            email2["Subject"].ShouldEqual("SubjectValue");
        }

        [Fact]
        public void Getting_dynamic_property_reads_from_ViewData()
        {
            var email = new Email("Test");
            email["Subject"] = "SubjectValue";

            dynamic email2 = email;
            Assert.Equal("SubjectValue", email2.Subject);

            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(email));
        }

        //[Fact]
        //public void Send_creates_EmailService_and_calls_Send()
        //{
        //    var emailService = new Mock<IEmailService>();
        //    Email.CreateEmailService = () => emailService.Object;
        //    var email = new Email("Test");

        //    email.Send();

        //    emailService.Verify(s => s.Send(email));
        //}

        [Fact]
        public void Derived_Email_sets_ViewData_Model()
        {
            var email = new TestEmail();
            email.ViewData.Model.ShouldBeSameAs(email);
        }

        [Fact]
        public void Derived_Email_sets_ViewName_from_class_name()
        {
            var email = new TestEmail();
            email.ViewName.ShouldEqual("Test");
        }

        class TestEmail : Email
        {
        }

        [Fact]
        public void Derived_Email_can_manually_set_ViewName()
        {
            var email = new NonDefaultViewNameEmail();
            email.ViewName.ShouldEqual("Test");
        }

        class NonDefaultViewNameEmail : Email
        {
            public NonDefaultViewNameEmail() : base("Test")
            {

            }
        }

        [Fact]
        public void Attach_adds_attachment()
        {
            dynamic email = new Email("Test");
            var fileStream = new FileStream(@"c:\tmp\test2.log", FileMode.Open);
            fileStream.Position = 0;
            var memoryStream = new MemoryStream();
            fileStream.CopyTo(memoryStream);
            //var attachment = new Attachment(new MemoryStream(), "name");
            var attachment = new Attachment(memoryStream, "name");
            email.Attach(attachment);
            ((Email)email).Attachments.ShouldContain(attachment);

            Console.WriteLine(JsonConvert.SerializeObject(email, Formatting.Indented,
                new MemoryStreamJsonConverter()));
        }
    }
}
