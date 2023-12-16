using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MetaFarms.Libs.Generators.Templating.NTypewriter.Attributes;

public static class TypeMemberTemplateAttribute
{
    private const string ClassName = "TypeMemberTemplateAttribute";

    private const string AttributeSourceCode = @"
using System;
namespace MetaFarms.Libs.Generators.Templating;
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
internal sealed class TypeMemberTemplateAttribute : System.Attribute
{
    public TypeMemberTemplateAttribute(string templateName)
    {
    }
}
";

    public const string Name = "TypeMemberTemplate";

    public static void Register(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput((ctx) =>
        {
            ctx.AddSource($"{ClassName}.g.cs", SourceText.From(AttributeSourceCode, Encoding.UTF8));
        });
    }
}
