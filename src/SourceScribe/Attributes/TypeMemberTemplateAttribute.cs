using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SourceScribe.Attributes;

public static class TypeMemberTemplateAttribute
{
    public const string ClassName = "TypeMemberTemplateAttribute";

    private const string AttributeSourceCode = @"
using System;
namespace SourceScribe;
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
internal class TypeMemberTemplateAttribute : Attribute
{
    public TypeMemberTemplateAttribute(string templateName)
    {
    }
}
";

    public const string Name = "TypeMemberTemplate";
    
    public const string QualifiedName = "SourceScribe.TypeMemberTemplateAttribute";

    /// <summary>
    /// Register the TypeMemberTemplateAttribute within the consuming assembly.
    /// </summary>
    /// <param name="context"></param>
    public static void AddSource(IncrementalGeneratorPostInitializationContext context)
    {
        context.AddSource($"{ClassName}.g.cs", SourceText.From(AttributeSourceCode, Encoding.UTF8));
    }
}
