using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace SourceScribe.Extensions;

public static class RoslynExtensions
{
    internal static IEnumerable<ITypeSymbol> GetBaseTypesAndThis(this ITypeSymbol type)
    {
        var current = type;
        while (current != null)
        {
            yield return current;

            current = current.BaseType;
        }
    }
}
