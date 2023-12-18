using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using MetaFarms.Libs.Generators.Templating.Attributes;
using MetaFarms.Libs.Generators.Templating.Scriban;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using MetaFarms.Libs.Generators.Templating.CodeModel.Roslyn;
using Scriban;
using Scriban.Runtime;
using Scriban.Syntax;
using Location = Microsoft.CodeAnalysis.Location;
using Type = MetaFarms.Libs.Generators.Templating.CodeModel.Roslyn.Type;

namespace MetaFarms.Libs.Generators.Templating;

internal static class TypeMemberTemplateHandler
{
    private static readonly DiagnosticDescriptor ScirbanParseError = new ("TMTH0001", // id
        "Error parsing template", // title
        "{0}", // message
        $"MetaFarms.Libs.Generators.Templating", // category
        DiagnosticSeverity.Error,
        true);

    internal static void Register(IncrementalGeneratorInitializationContext context)
    {
        var registerAttributesProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                RegisterTypeMemberTemplateAttribute.QualifiedName,
                (node, _) =>
                {
                    if (node is ClassDeclarationSyntax classDeclarationSyntax)
                    {
                        return classDeclarationSyntax.BaseList.Types.Any(t => t.Type.ToString() == "Attribute");
                    }

                    return false;
                },
                (node, _) =>
                {
                    var args = node.Attributes.First(x =>
                            x.AttributeClass?.MetadataName == RegisterTypeMemberTemplateAttribute.QualifiedName)
                        .ConstructorArguments;

                    return (args[0].Value, args[1].Value);
                })
            .Collect();

        // get types that have the TypeMemberTemplateAttribute specified
        
        var typesProvider = GetTypesProvider(context);
        var files = GetAdditionalFilesProvider(context);

        var combined = typesProvider.Combine(files);

        context.RegisterSourceOutput(combined, GenerateFromTypeTemplateAttributes);
    }

    private static IncrementalValuesProvider<INamedTypeSymbol> GetTypesProvider(
        IncrementalGeneratorInitializationContext context)
    {
        var typesProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            TypeMemberTemplateAttribute.QualifiedName,
            (node, _) => node is TypeDeclarationSyntax,
            (node, _) => node.SemanticModel.GetDeclaredSymbol(node.TargetNode) as INamedTypeSymbol);

        return typesProvider;
    }

    private static IncrementalValueProvider<ImmutableArray<(string fileName, string filePath, string content)>> GetAdditionalFilesProvider(IncrementalGeneratorInitializationContext context)
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
        (INamedTypeSymbol Left, ImmutableArray<(string fileName, string filePath, string template)> Right) arg2)
    {
        var typeSymbol = arg2.Left;
        var files = arg2.Right;

        var attributes = arg2.Left.GetAttributes()
            .Where(x => x.AttributeClass?.Name.StartsWith(TypeMemberTemplateAttribute.Name) == true);

        IType typeInfo = Type.Create(typeSymbol);

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

            var name = Path.GetFileName(foundFile.fileName);

            try
            {
                var template = Template.Parse(foundFile.template, foundFile.filePath);
                var context = GetTemplateContext(typeInfo, files);
                var output = template.Render(context);

                arg1.AddSource($"{arg2.Left.Name}_{name}.g.cs", SourceText.From(output, Encoding.UTF8));
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
