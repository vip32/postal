using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web.Mvc;

namespace Postal
{
    /// <summary>
    ///     Renders a preview of an Template to display in the browser.
    /// </summary>
    public class TemplateViewResult : ViewResult
    {
        private const string TextContentType = "text/plain";
        private const string HtmlContentType = "text/html";

        /// <summary>
        ///     Creates a new <see cref="TemplateViewResult" />.
        /// </summary>
        public TemplateViewResult(Template template, ITemplateViewRenderer renderer, ITemplateParser<MailMessage> parser)
        {
            Template = template;
            Renderer = renderer ?? new TemplateViewRenderer(ViewEngineCollection);
            TemplateParser = parser ?? new MailMessageTemplateParser(Renderer);
        }

        /// <summary>
        ///     Creates a new <see cref="TemplateViewResult" />.
        /// </summary>
        public TemplateViewResult(Template template)
            : this(template, null, null)
        {
        }

        private ITemplateViewRenderer Renderer { get; }
        private ITemplateParser<MailMessage> TemplateParser { get; }
        private Template Template { get; }

        /// <summary>
        ///     When called by the action invoker, renders the view to the response.
        /// </summary>
        public override void ExecuteResult(ControllerContext context)
        {
            var httpContext = context.RequestContext.HttpContext;
            var query = httpContext.Request.QueryString;
            var format = query["format"];
            var contentType = ExecuteResult(context.HttpContext.Response.Output, format);
            httpContext.Response.ContentType = contentType;
        }

        /// <summary>
        ///     Writes the Template preview in the given format.
        /// </summary>
        /// <returns>The content type for the HTTP response.</returns>
        public string ExecuteResult(TextWriter writer, string format = null)
        {
            var result = Renderer.Render(Template);
            var message = TemplateParser.Parse(result, Template as EmailTemplate);

            // no special requests; render what's in the template
            if (string.IsNullOrEmpty(format))
            {
                if (!message.IsBodyHtml)
                {
                    writer.Write(result);
                    return TextContentType;
                }

                var template = Extract(result);
                template.Write(writer);
                return HtmlContentType;
            }

            // Check if alternative
            var alternativeContentType = CheckAlternativeViews(writer, message, format);

            if (!string.IsNullOrEmpty(alternativeContentType))
                return alternativeContentType;

            if (format == "text")
            {
                if (message.IsBodyHtml)
                    throw new NotSupportedException("No text view available for this Template");

                writer.Write(result);
                return TextContentType;
            }

            if (format == "html")
            {
                if (!message.IsBodyHtml)
                    throw new NotSupportedException("No html view available for this Template");

                var template = Extract(result);
                template.Write(writer);
                return HtmlContentType;
            }

            throw new NotSupportedException(string.Format("Unsupported format {0}", format));
        }

        private static string CheckAlternativeViews(TextWriter writer, MailMessage mailMessage, string format)
        {
            var contentType = format == "html"
                ? HtmlContentType
                : TextContentType;

            // check for alternative view
            var view = mailMessage.AlternateViews.FirstOrDefault(v => v.ContentType.MediaType == contentType);

            if (view == null)
                return null;

            string content;
            using (var reader = new StreamReader(view.ContentStream))
                content = reader.ReadToEnd();

            content = ReplaceLinkedImagesWithEmbeddedImages(view, content);

            writer.Write(content);
            return contentType;
        }

        private static TemplateParts Extract(string template)
        {
            var headerBuilder = new StringBuilder();

            using (var reader = new StringReader(template))
            {
                // try to read until we passed headers
                var line = reader.ReadLine();

                while (line != null)
                {
                    if (string.IsNullOrEmpty(line))
                    {
                        return new TemplateParts(headerBuilder.ToString(), reader.ReadToEnd());
                    }

                    headerBuilder.AppendLine(line);
                    line = reader.ReadLine();
                }
            }

            return null;
        }

        internal static string ReplaceLinkedImagesWithEmbeddedImages(AlternateView view, string content)
        {
            var resources = view.LinkedResources;

            if (!resources.Any())
                return content;

            foreach (var resource in resources)
            {
                var find = "src=\"cid:" + resource.ContentId + "\"";
                var imageData = ComposeImageData(resource);
                content = content.Replace(find, "src=\"" + imageData + "\"");
            }

            return content;
        }

        private static string ComposeImageData(LinkedResource resource)
        {
            var contentType = resource.ContentType.MediaType;
            var bytes = ReadFully(resource.ContentStream);
            return string.Format("data:{0};base64,{1}",
                contentType,
                Convert.ToBase64String(bytes));
        }

        private static byte[] ReadFully(Stream input)
        {
            using (var ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private class TemplateParts
        {
            private readonly string _body;
            private readonly string _header;

            public TemplateParts(string header, string body)
            {
                _header = header;
                _body = body;
            }

            public void Write(TextWriter writer)
            {
                writer.WriteLine("<!--");
                writer.WriteLine(_header);
                writer.WriteLine("-->");
                writer.WriteLine(_body);
            }
        }
    }
}