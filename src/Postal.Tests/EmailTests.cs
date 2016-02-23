using System;
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
        //[Fact]
        //public void ViewName_is_set_by_constructor()
        //{
        //    var email = new Template("Test");
        //    email.ViewName.ShouldEqual("Test");
        //}

        //[Fact]
        //public void Cannot_create_Email_with_null_view_name()
        //{
        //    Assert.Throws<ArgumentNullException>(delegate
        //    {
        //        new Template(null);
        //    });
        //}

        //[Fact]
        //public void Cannot_create_Email_with_empty_view_name()
        //{
        //    Assert.Throws<ArgumentException>(delegate
        //    {
        //        new Template("");
        //    });
        //}

        //[Fact]
        //public void Dynamic_property_setting_assigns_ViewData_value()
        //{
        //    dynamic email = new Template("Test");
        //    email.Subject = "SubjectValue";

        //    var email2 = (Template)email;
        //    email2["Subject"].ShouldEqual("SubjectValue");
        //}

        //[Fact]
        //public void Getting_dynamic_property_reads_from_ViewData()
        //{
        //    var email = new Template("Test");
        //    email["Subject"] = "SubjectValue";

        //    dynamic email2 = email;
        //    Assert.Equal("SubjectValue", email2.Subject);

        //    Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(email));
        //}

        //[Fact]
        //public void Send_creates_EmailService_and_calls_Send()
        //{
        //    var emailService = new Mock<IMessagingService>();
        //    Template.CreateEmailService = () => emailService.Object;
        //    var Template = new Template("Test");

        //    Template.Send();

        //    emailService.Verify(s => s.Send(Template));
        //}

        [Fact]
        public void Derived_Email_sets_ViewData_Model()
        {
            var email = new TestTemplate();
            email.ViewData.Model.ShouldBeSameAs(email);
        }

        [Fact]
        public void Derived_Email_sets_ViewName_from_class_name()
        {
            var email = new TestTemplate();
            email.ViewName.ShouldEqual("Test");
        }

        class TestTemplate : Template
        {
            public override string ViewName { get; set; }
        }

        [Fact]
        public void Derived_Email_can_manually_set_ViewName()
        {
            var email = new NonDefaultViewNameTemplate();
            email.ViewName.ShouldEqual("Test");
        }

        class NonDefaultViewNameTemplate : Template
        {
            public NonDefaultViewNameTemplate() : base("Test")
            {

            }

            public override string ViewName { get; set; }
        }

        [Fact]
        public void Attach_adds_attachment()
        {
            dynamic email = new EmailTemplate("Test");
            var fileStream = new FileStream(@"c:\tmp\test2.log", FileMode.Open);
            fileStream.Position = 0;
            var memoryStream = new MemoryStream();
            fileStream.CopyTo(memoryStream);
            //var attachment = new Attachment(new MemoryStream(), "name");
            var attachment = new Attachment(memoryStream, "name");
            email.Attach(attachment);
            ((EmailTemplate)email).Attachments.ShouldContain(attachment);

            Console.WriteLine(JsonConvert.SerializeObject(email, Formatting.Indented,
                new MemoryStreamJsonConverter()));
        }
    }
}
