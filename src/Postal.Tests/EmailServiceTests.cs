﻿using System.Net.Mail;
using Moq;
using Should;
using Xunit;
using System.IO;
using System;

namespace Postal
{
    public class EmailServiceTests
    {
        //[Fact]
        //public void CreateMessage_returns_MailMessage_created_by_parser()
        //{
        //    var renderer = new Mock<ITemplateViewRenderer>();
        //    var parser = new Mock<ITemplateParser>();
        //    var service = new SmtpMessagingService(renderer.Object, parser.Object, () => null);
        //    var email = new Template("Test");
        //    var expectedMailMessage = new MailMessage();
        //    parser.Setup(p => p.Parse(It.IsAny<string>(), email)).Returns(expectedMailMessage);
            
        //    var actualMailMessage = service.ToMailMessage(email);

        //    actualMailMessage.ShouldBeSameAs(expectedMailMessage);
        //}

        //[Fact]
        //public void SendAync_returns_a_Task_and_sends_email()
        //{
        //    var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        //    Directory.CreateDirectory(dir);
        //    try
        //    {
        //        using (var smtp = new SmtpClient())
        //        {
        //            smtp.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
        //            smtp.PickupDirectoryLocation = dir;
        //            smtp.Host = "localhost"; // HACK: required by SmtpClient, but not actually used!

        //            var renderer = new Mock<ITemplateViewRenderer>();
        //            var parser = new Mock<ITemplateParser<MailMessage>>();
        //            var service = new SmtpMessagingService(renderer.Object, parser.Object, () => smtp);
        //            parser.Setup(p => p.Parse(It.IsAny<string>(), It.IsAny<EmailTemplate>()))
        //                  .Returns(new MailMessage("test@test.com", "test@test.com"));

        //            var sending = service.SendAsync(new Template("Test"));
        //            sending.Wait();

        //            Directory.GetFiles(dir).Length.ShouldEqual(1);
        //        }
        //    }
        //    finally
        //    {
        //        Directory.Delete(dir, true);
        //    }
        //}
    }
}
