using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace MetaFarms.Libs.Generators.Templating.CodeModel.Roslyn
{
    internal sealed class Delegate : NamedType, IDelegate
    {
        private readonly INamedTypeSymbol symbol;
        private readonly IMethodSymbol methodSymbol;

        public IEnumerable<IParameter> Parameters => ParameterCollection.Create(methodSymbol);
        public IType ReturnType => MetaFarms.Libs.Generators.Templating.CodeModel.Roslyn.Type.Create(methodSymbol.ReturnType);
     

        private Delegate(INamedTypeSymbol symbol) : base(symbol)
        {
            this.symbol = symbol;
            this.methodSymbol = symbol.DelegateInvokeMethod;
        }


        public static new Delegate Create(INamedTypeSymbol symbol)
        {
            return new Delegate(symbol);
        }
    }
}