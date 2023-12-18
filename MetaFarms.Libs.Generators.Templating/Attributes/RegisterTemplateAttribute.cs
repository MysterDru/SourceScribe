using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MetaFarms.Libs.Generators.Templating.Attributes;

public static class RegisterTypeMemberTemplateAttribute
{
    private const string ClassName = "RegisterTypeMemberTemplateAttribute";

    private const string AttributeSourceCode = @"
using System;
namespace MetaFarms.Libs.Generators.Templating;

/// <summary>
/// Provides a way to register a template to a custom attribute. Allows for better re-usability of templates in the consuming project.
/// An attribute registered in this manner can be used in place of [TypeMemberTemplateAttribute(""SomeTemplate.scriban"")].
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
internal sealed class RegisterTypeMemberTemplateAttribute : System.Attribute
{
    public RegisterTypeMemberTemplateAttribute(Type customAttributeType, string templateName)
    {
    }
}
";

    public const string Name = "RegisterTypeMemberTemplate";

    public const string QualifiedName = "MetaFarms.Libs.Generators.Templating.RegisterTypeMemberTemplateAttribute";

    /// <summary>
    /// Register the TypeMemberTemplateAttribute within the consuming assembly.
    /// </summary>
    /// <param name="context"></param>
    public static void AddSource(IncrementalGeneratorPostInitializationContext context)
    {
        // context.AddSource($"{ClassName}.g.cs", SourceText.From(AttributeSourceCode, Encoding.UTF8));
    }
}
