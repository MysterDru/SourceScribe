using Microsoft.CodeAnalysis;
using SourceScribe.CodeModel.Abstractions;

namespace SourceScribe.CodeModel.Roslyn
{
    internal sealed class TypeParameter : Type, ITypeParameter
    {
        private TypeParameter(ITypeParameterSymbol symbol) : base(symbol)
        {

        }


        public static ITypeParameter Create(ITypeParameterSymbol symbol)
        {
            return new TypeParameter(symbol);
        }
    }
}