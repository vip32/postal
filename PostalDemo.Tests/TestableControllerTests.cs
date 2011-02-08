using Moq;
using NUnit.Framework;
using Postal;
using PostalDemo.Controllers;

namespace PostalDemo.Tests
{
    public class TestableControllerTests
    {
        [Test]
        public void action_sends_email()
        {
            // Mock the IEmailService so we can verify the interaction.
            var emailService = new Mock<IEmailService>();
            // Create the controller to test
            var controller = new TestableController(emailService.Object);

            // Grab the Email object, when it's sent.
            dynamic sentEmail = null;
            emailService
                .Setup(s => s.Send(It.IsAny<Email>()))
                .Callback<Email>(e => sentEmail = e);

            controller.Send(new HelpRequest 
            { 
                EmailAddress = "john@smith.com",
                Message = "Help me!",
                Name = "John Smith"
            });

            // Check that the email was sent (alternatively, could call "Verify" on the mock).
            Assert.NotNull(sentEmail, "Email message was not sent.");

            // Check the email contains the correct data.
            Assert.AreEqual("john@smith.com", sentEmail.UserEmailAddress);
            Assert.AreEqual("Help me!", sentEmail.Message);
            Assert.AreEqual("John Smith", sentEmail.Name);
            Assert.NotNull(sentEmail.TicketId);
        }
    }
}
