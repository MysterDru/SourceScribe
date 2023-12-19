using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using SourceScribe.CodeModel.Abstractions;

namespace SourceScribe.CodeModel.Roslyn
{
    internal sealed class Property : SymbolBase, IProperty
    {
        private readonly IPropertySymbol symbol;

        public IType Type => Roslyn.Type.Create(symbol.Type, this);

        public IMethod GetMethod => symbol.GetMethod != null ? Method.Create(symbol.GetMethod) : null;

        public IMethod SetMethod => symbol.SetMethod != null ? Method.Create(symbol.SetMethod) : null;

        public List<IParameter> Parameters => new List<IParameter>(symbol.Parameters.Select(p => Parameter.Create(p)));

        public bool IsIndexer => symbol.IsIndexer;

        public bool IsWriteOnly => symbol.IsWriteOnly;

        public bool IsReadOnly => symbol.IsReadOnly;

        public bool IsSealed => symbol.IsSealed;

        private Property(IPropertySymbol symbol)
            : base(symbol)
        {
            this.symbol = symbol;
        }


        public static IProperty Create(IPropertySymbol symbol)
        {
            return new Property(symbol);
        }
    }
}
