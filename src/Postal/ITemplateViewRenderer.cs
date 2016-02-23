namespace Postal
{
    /// <summary>
    /// Renders an Template view.
    /// </summary>
    public interface ITemplateViewRenderer
    {
        /// <summary>
        /// Renders an Template view based on the provided view name.
        /// </summary>
        /// <param name="template">The Template data to pass to the view.</param>
        /// <param name="viewName">Optional, the name of the view. If null, the ViewName of the Template will be used.</param>
        /// <returns>The string result of rendering the Template.</returns>
        string Render(Template template, string viewName = null);
    }
}
