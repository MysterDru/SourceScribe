using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SourceScribe.Attributes;

public static class RegisterTypeMemberTemplateAttribute
{
    private const string ClassName = "RegisterTypeMemberTemplateAttribute";

    private const string AttributeSourceCode = @"
using System;
namespace SourceScribe;

/// <summary>
/// Provides a way to register a template to a custom attribute. Allows for better re-usability of templates in the consuming project.
/// An attribute registered in this manner can be used in place of [TypeMemberTemplateAttribute(""SomeTemplate.scriban"")].
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
internal sealed class RegisterTypeMemberTemplateAttribute : System.Attribute
{
    public RegisterTypeMemberTemplateAttribute(string templateName)
    {
    }
}
";

    public const string Name = "RegisterTypeMemberTemplate";

    public const string QualifiedName = "SourceScribe.RegisterTypeMemberTemplateAttribute";

    /// <summary>
    /// Register the TypeMemberTemplateAttribute within the consuming assembly.
    /// </summary>
    /// <param name="context"></param>
    public static void AddSource(IncrementalGeneratorPostInitializationContext context)
    {
        context.AddSource($"{ClassName}.g.cs", SourceText.From(AttributeSourceCode, Encoding.UTF8));
    }
}
