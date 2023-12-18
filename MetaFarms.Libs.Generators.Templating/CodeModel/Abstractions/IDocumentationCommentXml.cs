using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MetaFarms.Libs.Generators.Templating.CodeModel.Abstractions
{
    /// <summary>
    /// Represents a documentation XML comment.
    /// </summary>
    public interface IDocumentationCommentXml
    {
        /// <summary>
        /// Collection of the param tag.
        /// </summary>
        IEnumerable<IDocumentationCommentXmlParam> Params { get; }       

        /// <summary>
        /// The content of the return tag.
        /// </summary>
        string Returns { get; }

        /// <summary>
        /// The content of the summary tag.
        /// </summary>
        string Summary { get; }
    }
}