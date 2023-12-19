using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using SourceScribe.CodeModel.Abstractions;

namespace SourceScribe.CodeModel.Roslyn
{
    internal class NamedType : Type, INamedType
    {
        private readonly INamedTypeSymbol symbol;

        public IEnumerable<ITypeParameter> TypeParameters => TypeParameterCollection.Create(symbol);
        public bool IsNested => symbol.ContainingType != null;


        private protected NamedType(INamedTypeSymbol symbol) : base(symbol)
        {
            this.symbol = symbol;
        }


        public static NamedType Create(INamedTypeSymbol symbol)
        {
            if (symbol == null)
            {
                return null;
            }
            return new NamedType(symbol);
        }
    }
}