using Microsoft.CodeAnalysis;
using SourceScribe.CodeModel.Abstractions;

namespace SourceScribe.CodeModel.Roslyn
{
    internal sealed class Event : SymbolBase, IEvent
    {
        private readonly IEventSymbol symbol;

        public bool IsSealed => symbol.IsSealed;
        public IType Type => Roslyn.Type.Create(symbol.Type, this);
       

        private Event(IEventSymbol symbol) : base(symbol)
        {
            this.symbol = symbol;          
        }
                

        public static IEvent Create(IEventSymbol @event)
        {
            return new Event(@event);
        }
    }
}