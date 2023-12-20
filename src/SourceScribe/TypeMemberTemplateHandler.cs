using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using SourceScribe.Attributes;
using SourceScribe.Scriban;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Scriban;
using Scriban.Runtime;
using Scriban.Syntax;
using Location = Microsoft.CodeAnalysis.Location;
using Type = SourceScribe.CodeModel.Roslyn.Type;

namespace SourceScribe;

internal static class TypeMemberTemplateHandler
{
    // todo: need to enable release tracking for rules:
    // https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md
    // private static readonly DiagnosticDescriptor ScirbanParseError = new("TMTH0001", // id
    //     "Error parsing template", // title
    //     "{0}", // message
    //     $"SourceScribe", // category
    //     DiagnosticSeverity.Error,
    //     true);

    internal static void Register(IncrementalGeneratorInitializationContext context)
    {
        var typesProvider = GetTypesProvider(context);
        var files = GetAdditionalFilesProvider(context);

        var combined = typesProvider.Combine(files);

        context.RegisterSourceOutput(combined, GenerateFromTypeTemplateAttributes);
    }

    /// <summary>
    /// Get all types in the compilation that have the TypeMemberTemplateAttribute applied to them.
    /// Will also return types that have an attribute that has the TypeMemberTemplateAttribute applied to it
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private static IncrementalValuesProvider<(INamedTypeSymbol, IEnumerable<string>)> GetTypesProvider(
        IncrementalGeneratorInitializationContext context)
    {
        var typesProvider = context.SyntaxProvider.CreateSyntaxProvider(MayHaveTargetMemberTemplateAttribute,
                (ctx, _) => ctx.SemanticModel.GetDeclaredSymbol(ctx.Node) as INamedTypeSymbol)
            .Where(x =>
            {
                bool hasAttribute = GetTypeMemberTemplateAttributes(x, TypeMemberTemplateAttribute.Name, TypeMemberTemplateAttribute.ClassName)
                    .Any() || x.GetAttributes()
                    .SelectMany(a => GetTypeMemberTemplateAttributes(a.AttributeClass, RegisterTypeMemberTemplateAttribute.Name, RegisterTypeMemberTemplateAttribute.ClassName))
                    .Any();

                return hasAttribute;
            })
            .Select((symbol, _) => (symbol, GetTemplatesForTypeSymbol(symbol)));

        return typesProvider;
    }


    private static IEnumerable<string> GetTemplatesForTypeSymbol(INamedTypeSymbol symbol)
    {
        return GetTypeMemberTemplateAttributes(symbol, TypeMemberTemplateAttribute.Name, TypeMemberTemplateAttribute.ClassName)
            .Union(symbol.GetAttributes()
                .SelectMany(a => GetTypeMemberTemplateAttributes(a.AttributeClass, RegisterTypeMemberTemplateAttribute.Name, RegisterTypeMemberTemplateAttribute.ClassName)))
            .Select(a => a.ConstructorArguments.ElementAt(0)
                .Value!.ToString());
    }

    /// <summary>
    /// Returns all attributes from a <see cref="INamedTypeSymbol"/>
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="name"></param>
    /// <param name="className"></param>
    /// <returns></returns>
    private static IEnumerable<AttributeData> GetTypeMemberTemplateAttributes(INamedTypeSymbol symbol, string name, string className)
    {
        if (symbol != null)
        {
            // get attributes of the current symbol
            var attributes = symbol.GetAttributes();

            foreach (var attribute in attributes)
            {
                var attributeName = attribute.AttributeClass?.Name;

                if (attributeName != null && (attributeName.EndsWith(name) ||
                                              attributeName.EndsWith(className)))
                {
                    yield return attribute;
                }
            }
        }
    }

    /// <summary>
    /// Determine if the <see cref="SyntaxNode"/> is a "type" (class, interface, struct, record) and has at least 1 attribute applied to it.
    /// <br />
    /// If it matches, return the node as a candidate for code generation.
    /// If the node is an attribute, ignore it and don't consider it a candidate. 
    /// </summary>
    /// <param name="node">The <see cref="SyntaxNode"/> to check.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>True if the node is a candidate</returns>
    private static bool MayHaveTargetMemberTemplateAttribute(SyntaxNode node, CancellationToken cancellationToken)
    {
        // class, interface, struct, record, all have a base type of TypeDeclarationSyntax
        if (node is not TypeDeclarationSyntax typeDeclarationSyntax)
        {
            return false;
        }

        // if a type has a base type that is an attribute, then it is not a candiate.
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

        // if has at least 1 attribute, than is a candidate
        return typeDeclarationSyntax.AttributeLists.SelectMany(a => a.Attributes)
            .Any();
    }

    /// <summary>
    /// Get all additional files that have a file extension of <see cref="Constants.ScribanFileExtension"/>. Returns a formatted tuple of (fileName, filePath, content).
    /// </summary>
    /// <param name="context"><see cref="IncrementalGeneratorInitializationContext"/>.</param>
    /// <returns>An incremental value provider containing an array of tuples: (fileName, filePath, content) of each matched additional file.</returns>
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

    /// <summary>
    /// Generate code from the template files for a given type (<see cref="INamedTypeSymbol"/>).
    /// </summary>
    /// <param name="sourceProductionContext"></param>
    /// <param name="valueProviders">
    ///     The value within the provider containing:
    ///     <br />
    ///     - Left: The type symbol and the templates to generate.
    ///     <br />
    ///     - Righting: the list of template files that are available to generate from.
    /// </param>
    private static void GenerateFromTypeTemplateAttributes(
        SourceProductionContext sourceProductionContext,
        ((INamedTypeSymbol symbol, IEnumerable<string> templates) Left,
            ImmutableArray<(string fileName, string filePath, string template)> Right) valueProviders)
    {
        var typeSymbol = valueProviders.Left.symbol;
        var files = valueProviders.Right;

        IType typeInfo = Type.Create(typeSymbol);

        if (typeInfo == null)
        {
            // todo: log error for null/notfound type
            return;
        }

        foreach (var templateName in valueProviders.Left.templates)
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

                sourceProductionContext.AddSource($"{typeSymbol.Name}_{name}.g.cs",
                    SourceText.From(output, Encoding.UTF8));
            }
            catch (Exception ex)
            {
                var classFriendlyFileName = new Regex("[^a-zA-Z0-9 -]")
                    .Replace(name, string.Empty);
                
                // todo: report this as a diagnostic, need to enable release tracking for rules:
                // see: // https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md
                //
                // for now, report error information as comments in the source file
                string message = "Error generating file";
                string additionalText = string.Empty;
                if (ex is ScriptRuntimeException scriptEx)
                {
                    message = scriptEx.OriginalMessage;
                    
                    var additional = new StringBuilder();
                    additional.AppendLine("//");
                    additional.AppendLine("// Script Message:");
                    additional.AppendLine($"// {scriptEx.OriginalMessage}");
                    additional.AppendLine("//");
                    additional.AppendLine($"// File: {foundFile.filePath}");
                    additional.AppendLine($"//       {scriptEx.Span.ToStringSimple()}");

                    additionalText = additional.ToString();
                }
                
                StringBuilder sb = new();
                sb.AppendLine($"public static class {typeSymbol.Name}_{classFriendlyFileName}");
                sb.AppendLine("{");
                sb.AppendLine($"#error Failed to generate file: {message}");
                sb.AppendLine($"// Exception: {ex.GetType()}");
                sb.AppendLine($"// {ex.Message}");
                sb.AppendLine("//");
                sb.AppendLine(additionalText);
                sb.AppendLine("}");
                
                sourceProductionContext.AddSource($"{typeSymbol.Name}_{name}.g.cs",
                    SourceText.From(sb.ToString(), Encoding.UTF8));
            }
        }
    }

    /// <summary>
    /// Create a <see cref="TemplateContext"/> for the given type and files.
    /// </summary>
    /// <param name="typeInfo"></param>
    /// <param name="files"></param>
    /// <returns></returns>
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