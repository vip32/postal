using System.Net.Mail;
using System.Threading.Tasks;

namespace Postal
{
    /// <summary>
    /// Creates and send Template.
    /// </summary>
    public interface IMessagingService
    {
        /// <summary>
        /// Creates and sends a <see cref="MailMessage"/> using <see cref="SmtpClient"/>.
        /// This uses the default configuration for mail defined in web.config.
        /// </summary>
        /// <param name="template">The Template to send.</param>
        void Send(Template template);

        /// <summary>
        /// Creates and sends a <see cref="MailMessage"/> asynchronously using <see cref="SmtpClient"/>.
        /// This uses the default configuration for mail defined in web.config.
        /// </summary>
        /// <param name="template">The Template to send.</param>
        /// <returns>A <see cref="Task"/> that can be used to await completion of sending the Template.</returns>
        Task SendAsync(Template template);

        /// <summary>
        /// Creates a new <see cref="MailMessage"/> for the given Template. You can
        /// modify the message, for example adding attachments, and then send this yourself.
        /// </summary>
        /// <param name="template">The Template to generate.</param>
        /// <returns>A new <see cref="MailMessage"/>.</returns>
        //MailMessage ToMailMessage(Template template);
    }
}
