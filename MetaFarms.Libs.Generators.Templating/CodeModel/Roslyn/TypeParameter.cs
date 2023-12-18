using Microsoft.CodeAnalysis;

namespace MetaFarms.Libs.Generators.Templating.CodeModel.Roslyn
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