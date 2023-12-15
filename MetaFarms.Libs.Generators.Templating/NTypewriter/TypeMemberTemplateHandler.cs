using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NTypewriter.CodeModel;
using NTypewriter.CodeModel.Roslyn;
using Scriban;
using Scriban.Runtime;

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
                return node is TypeDeclarationSyntax typeSyntax && typeSyntax.AttributeLists
                    .SelectMany(a => a.Attributes)
                    .Any(a => a.Name.ToString()
                        .StartsWith("TypeMemberTemplate"));
            },
            (ctx, _) => { return ctx.SemanticModel.GetDeclaredSymbol(ctx.Node) as INamedTypeSymbol; });

        var files = context.AdditionalTextsProvider.Where(x => Path.GetFileName(x.Path)
                .EndsWith(".scriban"))
            .Select((f, _) => (fileName: Path.GetFileName(f.Path), content: f.GetText()
                ?.ToString() ?? string.Empty))
            .Collect();

        var combined = typesProvider.Combine(files);

        context.RegisterSourceOutput(combined, GenerateFromTypeTemplateAttributes);
    }

    private static void GenerateFromTypeTemplateAttributes(
        SourceProductionContext arg1,
        (INamedTypeSymbol Left, ImmutableArray<(string fileName, string template)> Right) arg2)
    {
        var typeSymbol = arg2.Left;
        var files = arg2.Right;

        var attributes = arg2.Left.GetAttributes()
            .Where(x => x.AttributeClass?.Name.StartsWith("TypeMemberTemplate") == true);

        IType typeInfo = _createTypeMethod.Value?.Invoke(null, new object[] { typeSymbol, null }) as IType;

        if (typeInfo == null)
        {
            // todo: log error for null/notfound type
            return;
        }

        foreach (var attribute in attributes)
        {
            var templateName = attribute.ConstructorArguments.ElementAt(0)
                .Value?.ToString();
            var foundFile = files.FirstOrDefault(x => Path.GetFileName(x.fileName) == templateName);

            if (foundFile.template == null)
            {
                continue;
            }

            var template = Template.Parse(foundFile.template);
            var context = GetTemplateContext(typeInfo, files);
            var result = template.Render(context);

            var name = Path.GetFileName(foundFile.fileName);
            arg1.AddSource($"{arg2.Left.Name}_{name}.g.cs", SourceText.From(result, Encoding.UTF8));
        }
    }

    private static TemplateContext GetTemplateContext(
        IType typeInfo,
        ImmutableArray<(string fileName, string template)> files)
    {
        var scriptObject = new ScriptObject();
        scriptObject.Import(new
        {
            member = typeInfo,
        });

        var context = new TemplateContext
        {
            AutoIndent = false,
            TemplateLoader = new AdditionalFilesTemplateLoader(files)
        };
        context.PushGlobal(scriptObject);

        return context;
    }
}
