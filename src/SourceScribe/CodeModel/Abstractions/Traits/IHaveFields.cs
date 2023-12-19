using System;
using System.Collections.Generic;
using System.Text;

namespace SourceScribe.CodeModel.Abstractions.Traits
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public interface IHaveFields
    {
        IEnumerable<IField> Fields { get; }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
