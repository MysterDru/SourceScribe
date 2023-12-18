using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using MetaFarms.Libs.Generators.Templating.Attributes;
using MetaFarms.Libs.Generators.Templating.Scriban;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NTypewriter.CodeModel;
using NTypewriter.CodeModel.Roslyn;
using Scriban;
using Scriban.Runtime;
using Scriban.Syntax;
using Location = Microsoft.CodeAnalysis.Location;
using Type = NTypewriter.CodeModel.Roslyn.Type;

namespace MetaFarms.Libs.Generators.Templating;

internal static class TypeMemberTemplateHandler
{
    private static readonly DiagnosticDescriptor ScirbanParseError
        = new("TEMPL001",                               // id
            "Error parsing template",           // title
            "{0}", // message
            $"MetaFarms.Libs.Generators.Templating.NTypewriter",                          // category
            DiagnosticSeverity.Error,
            true);
    
    internal static void Register(IncrementalGeneratorInitializationContext context)
    {
        var typesProvider = context.SyntaxProvider.CreateSyntaxProvider((node, _) =>
            {
                return node is TypeDeclarationSyntax typeSyntax && typeSyntax.AttributeLists
                    .SelectMany(a => a.Attributes)
                    .Any(a => a.Name.ToString()
                        .StartsWith(TypeMemberTemplateAttribute.Name));
            },
            (ctx, _) => { return ctx.SemanticModel.GetDeclaredSymbol(ctx.Node) as INamedTypeSymbol; });

        var files = context.AdditionalTextsProvider.Where(x => Path.GetFileName(x.Path)
                .EndsWith(".scriban"))
            .Select((f, _) => (fileName: Path.GetFileName(f.Path), filePath: f.Path, content: f.GetText()
                ?.ToString() ?? string.Empty))
            .Collect();

        var combined = typesProvider.Combine(files);

        context.RegisterSourceOutput(combined, GenerateFromTypeTemplateAttributes);
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

        if (typeInfo?.BareName == "GenerateInterfaceExample")
        {
            var prop = (typeInfo as Class).Properties.First(x => x.Name == "this[]");
        }
        
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
