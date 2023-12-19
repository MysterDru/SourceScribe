using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using SourceScribe.CodeModel.Abstractions;

namespace SourceScribe.CodeModel.Roslyn
{
    internal sealed class EnumValueCollection : List<IEnumValue>
    {
        private EnumValueCollection(IEnumerable<IEnumValue> items) : base(items)
        {

        }


        public static IEnumerable<IEnumValue> Create(INamedTypeSymbol symbol)
        {
            return new EnumValueCollection(symbol.GetMembers()
                .OfType<IFieldSymbol>()
                .Select(x => EnumValue.Create(x))
                .Where(x => x != null));
        }
    }
}