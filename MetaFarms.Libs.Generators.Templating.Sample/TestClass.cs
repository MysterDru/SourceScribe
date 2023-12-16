using MetaFarms.Libs.Generators.Templating;

namespace MetaFarms.Libs.Templating.Sample;

[TypeMemberTemplate("Test.scriban")]
[TypeMemberTemplate("InterfaceProperties.scriban")]
public partial class PropertyTest //: Interfaces.IProperties
{
    public string? PropertyOne { get; set; }

    // public string PropertyTwo { get; set; }
}
