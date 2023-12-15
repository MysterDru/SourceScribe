using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MetaFarms.Libs.Generators.Templating.NTypewriter;

[Generator]
public partial class IncrementalGenerator : IIncrementalGenerator
{
    private const string AttributeSourceCode = @"
using System;
namespace MetaFarms.Libs.Generators.Templating;
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
internal sealed class TypeMemberTemplateAttribute : Attribute
{
    public TypeMemberTemplateAttribute(string templateName)
    {
    }
}
";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ExecutePostInitialization);

        TypeMemberTemplateHandler.RegisterOutputForTypeMembers(context);
    }

    private void ExecutePostInitialization(IncrementalGeneratorPostInitializationContext obj)
    {
        obj.AddSource("Attribute.g.cs", SourceText.From(AttributeSourceCode, Encoding.UTF8));
    }
}
