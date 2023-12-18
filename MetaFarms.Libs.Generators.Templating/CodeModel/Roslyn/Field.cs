﻿using Microsoft.CodeAnalysis;

namespace MetaFarms.Libs.Generators.Templating.CodeModel.Roslyn
{
    internal sealed class Field : SymbolBase, IField
    {
        private readonly IFieldSymbol symbol;      

        public IType Type => MetaFarms.Libs.Generators.Templating.CodeModel.Roslyn.Type.Create(symbol.Type, this);
        public bool IsConst => symbol.IsConst;
        public bool IsReadOnly => symbol.IsReadOnly;
        public bool HasConstantValue => symbol.HasConstantValue;
        public object ConstantValue => symbol.Type.GetDefaultConstantValueAsString(symbol.ConstantValue);
      

        private Field(IFieldSymbol symbol) : base(symbol)
        {
            this.symbol = symbol;           
        }
       

        public static IField Create(IFieldSymbol symbol)
        {
            return new Field(symbol);
        }
    }
}