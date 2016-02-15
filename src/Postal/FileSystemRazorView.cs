﻿using System.IO;
using System.Web.Mvc;
using RazorEngine;

namespace Postal
{
    /// <summary>
    /// A view that uses the Razor engine to render a templates loaded directly from the
    /// file system. This means it will work outside of ASP.NET.
    /// </summary>
    public class FileSystemRazorView : IView
    {
        readonly string _template;
        readonly string _cacheName;
        
        /// <summary>
        /// Creates a new <see cref="FileSystemRazorView"/> using the given view filename.
        /// </summary>
        /// <param name="filename">The filename of the view.</param>
        public FileSystemRazorView(string filename)
        {
            _template = File.ReadAllText(filename);
            _cacheName = filename;
        }

        /// <summary>
        /// Renders the view into the given <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="viewContext">The <see cref="ViewContext"/> that contains the view data model.</param>
        /// <param name="writer">The <see cref="TextWriter"/> used to write the rendered output.</param>
        public void Render(ViewContext viewContext, TextWriter writer)
        {
            var content = Razor.Parse(_template, viewContext.ViewData.Model, _cacheName);

            writer.Write(content);
            writer.Flush();
        }
    }
}
