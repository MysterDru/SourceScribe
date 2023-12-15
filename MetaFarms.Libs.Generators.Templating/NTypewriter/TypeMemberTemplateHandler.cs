using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NTypewriter.CodeModel;
using NTypewriter.CodeModel.Roslyn;

namespace MetaFarms.Libs.Generators.Templating.NTypewriter;

public static class TypeMemberTemplateHandler
{
    private static Lazy<MethodInfo> _createTypeMethod = new (() =>
    {
        var t = typeof(CodeModel).Assembly.GetType("NTypewriter.CodeModel.Roslyn.Type");
        return t?.GetMethod("Create", new Type[] { typeof(ITypeSymbol), typeof(ISymbolBase) });
    });

    internal static void RegisterOutputForTypeMembers(IncrementalGeneratorInitializationContext context)
    {   
        var typesProvider = context.SyntaxProvider.CreateSyntaxProvider((node, _) =>
            {
                if(node is TypeDeclarationSyntax typeSyntax)
                {
                    var hasTypeMemberTemplateAttribute = typeSyntax.AttributeLists.SelectMany(a => a.Attributes)
                        .Any(a => a.Name.ToString()
                            .StartsWith("TypeMemberTemplate"));

                    return hasTypeMemberTemplateAttribute;
                }

                return false;
            },
            (ctx, _) => ctx.SemanticModel.GetDeclaredSymbol(ctx.Node) as ITypeSymbol);
        
        context.RegisterSourceOutput(typesProvider, GenerateFromTypeTemplateAttributes);
    }

    private static void GenerateFromTypeTemplateAttributes(SourceProductionContext ctx, ITypeSymbol typeSymbol)
    {
        IType typeInfo = _createTypeMethod.Value?.Invoke(null, new object[] { typeSymbol, null }) as IType;
    }

    // private static void GenerateFromTypeTemplateAttributes(
    //     SourceProductionContext ctx,
    //     (ITypeSymbol type, Compilation compilation) provider)
    // {
    //     // var model = provider.compilation.GetSemanticModel(provider.klass.SyntaxTree);
    //     // var typeSymbol = model.GetDeclaredSymbol(provider.klass);
    //     var typeSymbol = provider.type;
    //
    //     
    // }
}
