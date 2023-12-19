using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SourceScribe.Extensions;
using Scriban;
using Scriban.Runtime;
using Scriban.Syntax;
using SourceScribe.Attributes;
using SourceScribe.CodeModel.Abstractions;
using SourceScribe.CodeModel.Roslyn;
using SourceScribe.Scriban;
using Location = Microsoft.CodeAnalysis.Location;
using Type = SourceScribe.CodeModel.Roslyn.Type;

namespace SourceScribe;

internal static class TypeMemberTemplateHandler
{
    private static readonly DiagnosticDescriptor ScirbanParseError = new("TMTH0001", // id
        "Error parsing template", // title
        "{0}", // message
        $"SourceScribe", // category
        DiagnosticSeverity.Error,
        true);

    internal static void Register(IncrementalGeneratorInitializationContext context)
    {
        var typesProvider = GetTypesProvider(context);
        var files = GetAdditionalFilesProvider(context);

        var combined = typesProvider.Combine(files);

        context.RegisterSourceOutput(combined, GenerateFromTypeTemplateAttributes);
    }

    private static void HandleTypesWithFilesProvider(
        SourceProductionContext arg1,
        ((INamedTypeSymbol Left, ImmutableArray<INamedTypeSymbol> Right) Left,
            ImmutableArray<(string fileName, string filePath, string content)> Right) arg2)
    {
    }

    private static IncrementalValuesProvider<(INamedTypeSymbol, string[])> GetTypesProvider(
        IncrementalGeneratorInitializationContext context)
    {
        var typesProvider = context.SyntaxProvider.CreateSyntaxProvider(MayHaveTargetMemberTemplateAttribute,
                (ctx, _) => ctx.SemanticModel.GetDeclaredSymbol(ctx.Node) as INamedTypeSymbol)
            .Where(x =>
            {
                bool hasAttribute = HasTypeMemberTemplateAttribute(x)
                    .Any() || x.GetAttributes()
                    .SelectMany(a => HasTypeMemberTemplateAttribute(a.AttributeClass))
                    .Any();

                return hasAttribute;
            })
            .Select((symbol, _) => (symbol, GetTemplateForSymbol(symbol)));

        return typesProvider;
    }

    private static string[] GetTemplateForSymbol(INamedTypeSymbol symbol)
    {
        return HasTypeMemberTemplateAttribute(symbol)
            .Union(symbol.GetAttributes().SelectMany(a => HasTypeMemberTemplateAttribute(a.AttributeClass)))
            .Select(a => a.ConstructorArguments.ElementAt(0).Value.ToString())
            .ToArray();
    }

    private static IEnumerable<AttributeData> HasTypeMemberTemplateAttribute(INamedTypeSymbol symbol)
    {
        if (symbol != null)
        {
            var attributes = symbol.GetAttributes();

            foreach (var attribute in attributes)
            {
                var attributeName = attribute.AttributeClass?.Name;

                if (attributeName != null && (attributeName.EndsWith(TypeMemberTemplateAttribute.Name) ||
                                              attributeName.EndsWith(TypeMemberTemplateAttribute.ClassName)))
                {
                    yield return attribute;
                }
            }
        }
    }

    private static bool MayHaveTargetMemberTemplateAttribute(SyntaxNode node, CancellationToken cancellationToken)
    {
        if (node is TypeDeclarationSyntax typeDeclarationSyntax)
        {
            var hasAttributeBaseType = typeDeclarationSyntax.BaseList?.Types.Where(x => x.Type is NameSyntax)
                .Select(t => t.Type as NameSyntax)
                .Any(t =>
                {
                    if (t is IdentifierNameSyntax identifierNameSyntax)
                    {
                        return identifierNameSyntax.Identifier.Text.EndsWith("Attribute");
                    }

                    if (t is QualifiedNameSyntax qualifiedNameSyntax)
                    {
                        return qualifiedNameSyntax.Right.Identifier.Text.EndsWith("Attribute");
                    }

                    return false;
                }) ?? false;

            if (hasAttributeBaseType)
            {
                return false;
            }

            var attributes = typeDeclarationSyntax.AttributeLists.SelectMany(a => a.Attributes);

            return attributes.Any();
        }

        return false;
    }

    private static IncrementalValueProvider<ImmutableArray<(string fileName, string filePath, string content)>>
        GetAdditionalFilesProvider(IncrementalGeneratorInitializationContext context)
    {
        var files = context.AdditionalTextsProvider.Where(x => Path.GetFileName(x.Path)
                .EndsWith(Constants.ScribanFileExtension))
            .Select((f, _) => (fileName: Path.GetFileName(f.Path), filePath: f.Path, content: f.GetText()
                ?.ToString() ?? string.Empty))
            .Collect();

        return files;
    }

    private static void GenerateFromTypeTemplateAttributes(
        SourceProductionContext arg1,
        ((INamedTypeSymbol symbol, string[] templates) Left,
            ImmutableArray<(string fileName, string filePath, string template)> Right) arg2)
    {
        var typeSymbol = arg2.Left.symbol;
        var files = arg2.Right;

        IType typeInfo = Type.Create(typeSymbol);

        if (typeInfo == null)
        {
            // todo: log error for null/notfound type
            return;
        }

        foreach (var templateName in arg2.Left.templates)
        {
            var foundFile = files.FirstOrDefault(x => Path.GetFileName(x.fileName) == templateName);

            if (foundFile.template == null)
            {
                continue;
            }

            var name = Path.GetFileName(foundFile.fileName);

            try
            {
                var template = Template.Parse(foundFile.template, foundFile.filePath);
                var context = GetTemplateContext(typeInfo, files);
                var output = template.Render(context);

                arg1.AddSource($"{typeSymbol.Name}_{name}.g.cs", SourceText.From(output, Encoding.UTF8));
            }
            catch (ScriptRuntimeException ex)
            {
                var msg = ex.OriginalMessage;
                var span = ex.Span;

                var text = TextSpan.FromBounds(span.Start.Offset, span.End.Offset);
                var line = new LinePositionSpan(new LinePosition(span.Start.Line, span.Start.Column),
                    new LinePosition(span.End.Line, span.End.Column));

                var location = Location.Create(span.FileName, text, line);
                var diagnostic = Diagnostic.Create(ScirbanParseError, location, msg);

                arg1.ReportDiagnostic(diagnostic);
            }
            catch (Exception ex)
            {
                var location = Location.Create(foundFile.filePath, default, default);
                var diagnostic = Diagnostic.Create(ScirbanParseError, location, ex.Message);

                arg1.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static TemplateContext GetTemplateContext(
        IType typeInfo,
        ImmutableArray<(string fileName, string filePath, string template)> files)
    {
        // todo: this is probably the area that is the least performant. Is there a way to  cache the template context and script object for each type?
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