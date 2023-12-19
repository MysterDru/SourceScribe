using System;
using System.Collections.Generic;
using System.Text;

namespace SourceScribe.CodeModel.Abstractions
{
    /// <summary>
    /// Represents an enum.
    /// </summary>
    public interface IEnum : INamedType
    {
        /// <summary>
        /// All values defined in the enum.
        /// </summary>
        IEnumerable<IEnumValue> Values { get; }

        /// <summary>
        /// Gets the underlying type
        /// </summary>
        IType UnderlyingType { get; }
     }
}