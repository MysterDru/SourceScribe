using Microsoft.CodeAnalysis;

namespace MetaFarms.Libs.Generators.Templating;

/// <summary>
/// Incremental source generator that will read text template files in the project and generate code from them.
/// </summary>
[Generator]
public class TemplatingIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        Attributes.TypeMemberTemplateAttribute.Register(context);

        TypeMemberTemplateHandler.Register(context);
    }
}
