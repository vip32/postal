using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;

namespace Postal
{
    /// <summary>
    ///     Converts the raw string output of a view into a <see cref="MailMessage" />.
    /// </summary>
    public class MailMessageTemplateParser : ITemplateParser<MailMessage>
    {
        private readonly ITemplateViewRenderer _alternativeViewRenderer;

        /// <summary>
        ///     Creates a new <see cref="MailMessageTemplateParser" />.
        /// </summary>
        public MailMessageTemplateParser(ITemplateViewRenderer alternativeViewRenderer)
        {
            _alternativeViewRenderer = alternativeViewRenderer;
        }

        /// <summary>
        ///     Parses the Template view output into a <see cref="MailMessage" />.
        /// </summary>
        /// <param name="viewOutput">The Template view output.</param>
        /// <param name="template">The <see cref="Template" /> used to generate the output.</param>
        /// <returns>A <see cref="MailMessage" /> containing the Template headers and content.</returns>
        public MailMessage Parse(string viewOutput, Template template)
        {
            var message = new MailMessage();
            InitializeMailMessage(message, viewOutput, template);
            return message;
        }

        private void InitializeMailMessage(MailMessage message, string emailViewOutput, Template template)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (template == null) throw new ArgumentNullException(nameof(template));

            using (var reader = new StringReader(emailViewOutput))
            {
                ParserUtils.ParseHeaders(reader, (key, value) => ProcessHeader(key, value, message, template));
                AssignCommonHeaders(message, template);
                if (message.AlternateViews.Count == 0)
                {
                    var messageBody = reader.ReadToEnd().Trim();
                    if (template.ImageEmbedder.HasImages)
                    {
                        var view = AlternateView.CreateAlternateViewFromString(messageBody, new ContentType("text/html"));
                        template.ImageEmbedder.AddImagesToView(view);
                        message.AlternateViews.Add(view);
                        message.Body = "Plain text not available.";
                        message.IsBodyHtml = false;
                    }
                    else
                    {
                        message.Body = messageBody;
                        if (message.Body.StartsWith("<")) message.IsBodyHtml = true;
                    }
                }

                AddAttachments(message, template as EmailTemplate);
            }
        }

        private void AssignCommonHeaders(MailMessage message, Template template)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (template == null) throw new ArgumentNullException(nameof(template));

            if (message.To.Count == 0)
            {
                AssignCommonHeader<string>(template, "to", to => message.To.Add(to));
                AssignCommonHeader<MailAddress>(template, "to", to => message.To.Add(to));
            }
            if (message.From == null)
            {
                AssignCommonHeader<string>(template, "from", from => message.From = new MailAddress(from));
                AssignCommonHeader<MailAddress>(template, "from", from => message.From = from);
            }
            if (message.CC.Count == 0)
            {
                AssignCommonHeader<string>(template, "cc", cc => message.CC.Add(cc));
                AssignCommonHeader<MailAddress>(template, "cc", cc => message.CC.Add(cc));
            }
            if (message.Bcc.Count == 0)
            {
                AssignCommonHeader<string>(template, "bcc", bcc => message.Bcc.Add(bcc));
                AssignCommonHeader<MailAddress>(template, "bcc", bcc => message.Bcc.Add(bcc));
            }
            if (message.ReplyToList.Count == 0)
            {
                AssignCommonHeader<string>(template, "replyto", replyTo => message.ReplyToList.Add(replyTo));
                AssignCommonHeader<MailAddress>(template, "replyto", replyTo => message.ReplyToList.Add(replyTo));
            }
            if (message.Sender == null)
            {
                AssignCommonHeader<string>(template, "sender", sender => message.Sender = new MailAddress(sender));
                AssignCommonHeader<MailAddress>(template, "sender", sender => message.Sender = sender);
            }
            if (string.IsNullOrEmpty(message.Subject))
            {
                AssignCommonHeader<string>(template, "subject", subject => message.Subject = subject);
            }
        }

        private void AssignCommonHeader<T>(Template template, string header, Action<T> assign)
            where T : class
        {
            if (template == null) throw new ArgumentNullException(nameof(template));

            object value;
            if (template.ViewData.TryGetValue(header, out value))
            {
                var typedValue = value as T;
                if (typedValue != null) assign(typedValue);
            }
        }

        private void ProcessHeader(string key, string value, MailMessage message, Template template)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            if (IsAlternativeViewsHeader(key))
            {
                foreach (var view in CreateAlternativeViews(value, template))
                {
                    message.AlternateViews.Add(view);
                }
            }
            else
            {
                AssignEmailHeaderToMailMessage(key, value, message);
            }
        }

        private IEnumerable<AlternateView> CreateAlternativeViews(string deliminatedViewNames, Template template)
        {
            var viewNames = deliminatedViewNames.Split(new[] {',', ' ', ';'}, StringSplitOptions.RemoveEmptyEntries);
            return from viewName in viewNames
                select CreateAlternativeView(template, viewName);
        }

        private AlternateView CreateAlternativeView(Template template, string alternativeViewName)
        {
            var fullViewName = GetAlternativeViewName(template, alternativeViewName);
            var output = _alternativeViewRenderer.Render(template, fullViewName);

            string contentType;
            string body;
            using (var reader = new StringReader(output))
            {
                contentType = ParseHeadersForContentType(reader);
                body = reader.ReadToEnd();
            }

            if (string.IsNullOrWhiteSpace(contentType))
            {
                if (alternativeViewName.Equals("text", StringComparison.OrdinalIgnoreCase))
                {
                    contentType = "text/plain";
                }
                else if (alternativeViewName.Equals("html", StringComparison.OrdinalIgnoreCase))
                {
                    contentType = "text/html";
                }
                else
                {
                    throw new Exception("The 'Content-Type' header is missing from the alternative view '" +
                                        fullViewName + "'.");
                }
            }

            var stream = CreateStreamOfBody(body);
            var alternativeView = new AlternateView(stream, contentType);
            if (alternativeView.ContentType.CharSet == null)
            {
                // Must set a charset otherwise mail readers seem to guess the wrong one!
                // Strings are unicode by default in .net.
                alternativeView.ContentType.CharSet = Encoding.Unicode.WebName;
                // A different charset can be specified in the Content-Type header.
                // e.g. Content-Type: text/html; charset=utf-8
            }
            template.ImageEmbedder.AddImagesToView(alternativeView);
            return alternativeView;
        }

        private static string GetAlternativeViewName(Template template, string alternativeViewName)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));

            if (template.ViewName.StartsWith("~"))
            {
                var index = template.ViewName.LastIndexOf('.');
                return template.ViewName.Insert(index + 1, alternativeViewName + ".");
            }
            return template.ViewName + "." + alternativeViewName;
        }

        private MemoryStream CreateStreamOfBody(string body)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(body);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private string ParseHeadersForContentType(StringReader reader)
        {
            string contentType = null;
            ParserUtils.ParseHeaders(reader, (key, value) =>
            {
                if (key.Equals("content-type", StringComparison.OrdinalIgnoreCase))
                {
                    contentType = value;
                }
            });
            return contentType;
        }

        private bool IsAlternativeViewsHeader(string headerName)
        {
            if (string.IsNullOrEmpty(headerName)) throw new ArgumentNullException(nameof(headerName));

            return headerName.Equals("views", StringComparison.OrdinalIgnoreCase);
        }

        private void AssignEmailHeaderToMailMessage(string key, string value, MailMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            switch (key)
            {
                case "to":
                    message.To.Add(value);
                    break;
                case "from":
                    message.From = new MailAddress(value);
                    break;
                case "subject":
                    message.Subject = value;
                    break;
                case "cc":
                    message.CC.Add(value);
                    break;
                case "bcc":
                    message.Bcc.Add(value);
                    break;
                case "reply-to":
                    message.ReplyToList.Add(value);
                    break;
                case "sender":
                    message.Sender = new MailAddress(value);
                    break;
                case "priority":
                    MailPriority priority;
                    if (Enum.TryParse(value, true, out priority))
                    {
                        message.Priority = priority;
                    }
                    else
                    {
                        throw new ArgumentException(
                            string.Format("Invalid Template priority: {0}. It must be High, Medium or Low.", value));
                    }
                    break;
                case "content-type":
                    var charsetMatch = Regex.Match(value, @"\bcharset\s*=\s*(.*)$");
                    if (charsetMatch.Success)
                    {
                        message.BodyEncoding = Encoding.GetEncoding(charsetMatch.Groups[1].Value);
                    }
                    break;
                default:
                    message.Headers[key] = value;
                    break;
            }
        }

        private void AddAttachments(MailMessage message, EmailTemplate template)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (template == null) return;

            foreach (var attachment in template.Attachments)
            {
                message.Attachments.Add(attachment);
            }
        }
    }
}