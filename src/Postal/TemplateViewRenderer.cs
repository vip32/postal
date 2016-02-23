using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Postal
{
    /// <summary>
    /// Renders <see cref="Template"/> view's into raw strings using the MVC ViewEngine infrastructure.
    /// </summary>
    public class TemplateViewRenderer : ITemplateViewRenderer
    {
        /// <summary>
        /// Creates a new <see cref="TemplateViewRenderer"/> that uses the given view engines.
        /// </summary>
        /// <param name="viewEngines">The view engines to use when rendering Template views.</param>
        public TemplateViewRenderer(ViewEngineCollection viewEngines)
        {
            if (viewEngines == null) throw new ArgumentNullException(nameof(viewEngines));

            _viewEngines = viewEngines;
            ViewDirectoryName = "Templates";
        }

        readonly ViewEngineCollection _viewEngines;

        /// <summary>
        /// The name of the directory in "Views" that contains the Template views.
        /// By default, this is "Templates".
        /// </summary>
        public string ViewDirectoryName { get; set; }

        /// <summary>
        /// Renders an Template view.
        /// </summary>
        /// <param name="template">The Template to render.</param>
        /// <param name="viewName">Optional Template view name override. If null then the Template's ViewName property is used instead.</param>
        /// <returns>The rendered Template view output.</returns>
        public string Render(Template template, string viewName = null)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));

            viewName = viewName ?? template.ViewName;
            var controllerContext = CreateControllerContext();
            var view = CreateView(viewName, controllerContext);
            var viewOutput = RenderView(view, template.ViewData, controllerContext, template.ImageEmbedder); // RazorEngine to writer
            return viewOutput;
        }

        ControllerContext CreateControllerContext()
        {
            // A dummy HttpContextBase that is enough to allow the view to be rendered.
            var httpContext = new HttpContextWrapper(
                new HttpContext(
                    new HttpRequest("", UrlRoot(), ""),
                    new HttpResponse(TextWriter.Null)
                )
            );
            var routeData = new RouteData();
            routeData.Values["controller"] = ViewDirectoryName;
            var requestContext = new RequestContext(httpContext, routeData);
            var stubController = new StubController();
            var controllerContext = new ControllerContext(requestContext, stubController);
            stubController.ControllerContext = controllerContext;
            return controllerContext;
        }

        string UrlRoot()
        {
            var httpContext = HttpContext.Current;
            if (httpContext == null)
            {
                return "http://localhost";
            }

            return httpContext.Request.Url.GetLeftPart(UriPartial.Authority) +
                   httpContext.Request.ApplicationPath;
        }

        IView CreateView(string viewName, ControllerContext controllerContext)
        {
            var result = _viewEngines.FindView(controllerContext, viewName, null);
            if (result.View != null)
                return result.View;

            throw new Exception(
                "Template view not found for " + viewName +
                ". Locations searched:" + Environment.NewLine +
                string.Join(Environment.NewLine, result.SearchedLocations)
            );
        }

        string RenderView(IView view, ViewDataDictionary viewData, ControllerContext controllerContext, ImageEmbedder imageEmbedder)
        {
            using (var writer = new StringWriter())
            {
                var viewContext = new ViewContext(controllerContext, view, viewData, new TempDataDictionary(), writer);
                viewData[ImageEmbedder.ViewDataKey] = imageEmbedder;
                view.Render(viewContext, writer); // RazorEngine to writer
                viewData.Remove(ImageEmbedder.ViewDataKey);
                return writer.GetStringBuilder().ToString();
            }
        }

        // StubController so we can create a ControllerContext.
        class StubController : Controller { }
    }
}
