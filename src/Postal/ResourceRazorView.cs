using System.IO;
using System.Reflection;
using System.Web.Mvc;
using RazorEngine;

namespace Postal
{
    /// <summary>
    /// An <see cref="IView"/> that reads its content from an assembly resource.
    /// </summary>
    public class ResourceRazorView : IView
    {
        private readonly string _resourcePath;
        private readonly string _template;

        /// <summary>
        /// Creates a new <see cref="ResourceRazorView"/> for a given assembly and resource.
        /// </summary>
        /// <param name="sourceAssembly">The assembly containing the resource.</param>
        /// <param name="resourcePath">The resource path.</param>
        public ResourceRazorView(Assembly sourceAssembly, string resourcePath)
        {
            this._resourcePath = resourcePath;
            // We've already ensured that the resource exists in ResourceRazorViewEngine
            // ReSharper disable AssignNullToNotNullAttribute
            using (var stream = sourceAssembly.GetManifestResourceStream(resourcePath))
            using (var reader = new StreamReader(stream))
                _template = reader.ReadToEnd();
            // ReSharper restore AssignNullToNotNullAttribute
        }

        /// <summary>
        /// Renders the view into the given <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="viewContext">Contains the view data model.</param>
        /// <param name="writer">The <see cref="TextWriter"/> used to write the rendered output.</param>
        public void Render(ViewContext viewContext, TextWriter writer)
        {
            var content = Razor.Parse(_template, viewContext.ViewData.Model, _resourcePath);

            writer.Write(content);
            writer.Flush();
        }
    }
}