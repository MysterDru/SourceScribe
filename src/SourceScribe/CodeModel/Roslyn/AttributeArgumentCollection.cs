using System;
using System.Collections.Generic;
using System.Linq;
using SourceScribe.CodeModel.Abstractions;

namespace SourceScribe.CodeModel.Roslyn
{
    internal sealed class AttributeArgumentCollection : List<IAttributeArgument>
    {
        private AttributeArgumentCollection()
        {

        }


        public static AttributeArgumentCollection Create()
        {
            return new AttributeArgumentCollection();
        }


        public override string ToString()
        {
            var values = this.Select(x => x.IsFromConstructor ? x.Value : $"{x.Name} : {x.Value}");
            var result = String.Join(", ", values);                
            return result;
        }      
    }
}