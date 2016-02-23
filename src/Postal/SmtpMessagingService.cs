using System;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Postal
{
    /// <summary>
    ///     Sends Template using the default <see cref="SmtpClient" />.
    /// </summary>
    public class SmtpMessagingService : IMessagingService
    {
        private readonly ITemplateParser<MailMessage> _templateParser;
        private readonly ITemplateViewRenderer _renderer;
        private readonly Func<SmtpClient> _smtpClientFunc;

        /// <summary>
        ///     Creates a new cref="SmtpMessagingService"/, using the default view engines.
        /// </summary>
        public SmtpMessagingService() : this(ViewEngines.Engines)
        {
        }

        /// <summary>Creates a new <see cref="SmtpMessagingService" />, using the given view engines.</summary>
        /// <param name="viewEngines">The view engines to use when creating Template views.</param>
        /// <param name="createSmtpClient">
        ///     A function that creates a <see cref="SmtpClient" />. If null, a default creation
        ///     function is used.
        /// </param>
        public SmtpMessagingService(ViewEngineCollection viewEngines, Func<SmtpClient> createSmtpClient = null)
        {
            if (viewEngines == null) throw new ArgumentNullException(nameof(viewEngines));

            _renderer = new TemplateViewRenderer(viewEngines);
            _templateParser = new MailMessageTemplateParser(_renderer);
            _smtpClientFunc = createSmtpClient ?? (() => new SmtpClient());
        }

        /// <summary>
        ///     Creates a new <see cref="SmtpMessagingService" />.
        /// </summary>
        public SmtpMessagingService(ITemplateViewRenderer renderer, ITemplateParser<MailMessage> templateParser, Func<SmtpClient> smtpClientFunc)
        {
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));
            if (templateParser == null) throw new ArgumentNullException(nameof(templateParser));
            if (smtpClientFunc == null) throw new ArgumentNullException(nameof(smtpClientFunc));

            _renderer = renderer;
            _templateParser = templateParser;
            _smtpClientFunc = smtpClientFunc;
        }

        /// <summary>
        ///     Sends an Template using an <see cref="SmtpClient" />.
        /// </summary>
        /// <param name="template">The Template to send.</param>
        public void Send(Template template)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));

            using (var mailMessage = ToMailMessage(template as EmailTemplate))
            using (var smtp = _smtpClientFunc())
            {
                smtp.Send(mailMessage);
            }
        }

        /// <summary>
        ///     Send an Template asynchronously, using an <see cref="SmtpClient" />.
        /// </summary>
        /// <param name="template">The Template to send.</param>
        /// <returns>A <see cref="Task" /> that completes once the Template has been sent.</returns>
        public Task SendAsync(Template template)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));

            // Wrap the SmtpClient's awkward async API in the much nicer Task pattern.
            // However, we must be careful to dispose of the resources we create correctly.
            var message = ToMailMessage(template as EmailTemplate);
            try
            {
                var smtp = _smtpClientFunc();
                try
                {
                    var taskCompletionSource = new TaskCompletionSource<object>();

                    smtp.SendCompleted += (o, e) =>
                    {
                        smtp.Dispose();
                        message.Dispose();

                        if (e.Error != null)
                        {
                            taskCompletionSource.TrySetException(e.Error);
                        }
                        else if (e.Cancelled)
                        {
                            taskCompletionSource.TrySetCanceled();
                        }
                        else // Success
                        {
                            taskCompletionSource.TrySetResult(null);
                        }
                    };

                    smtp.SendAsync(message, null);
                    return taskCompletionSource.Task;
                }
                catch
                {
                    smtp.Dispose();
                    throw;
                }
            }
            catch
            {
                message.Dispose();
                throw;
            }
        }

        /// <summary>
        ///     Renders the Template view and builds a <see cref="MailMessage" />. Does not send the Template.
        /// </summary>
        /// <param name="template">The Template to render.</param>
        /// <returns>A <see cref="MailMessage" /> containing the rendered Template.</returns>
        private MailMessage ToMailMessage(EmailTemplate template)
        {
            var content = _renderer.Render(template);
            return _templateParser.Parse(content, template);  // template to mailmessage (+add attachments)
        }
    }
}