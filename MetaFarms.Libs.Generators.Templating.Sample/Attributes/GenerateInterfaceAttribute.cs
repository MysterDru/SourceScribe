using System;
using MetaFarms.Libs.Generators.Templating;

namespace MetaFarms.Libs.Templating.Sample.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
// [RegisterTypeMemberTemplate("RegisterTypeMemberTemplateAttribute")]
[TypeMemberTemplate("InterfaceGenerator.scriban")]
internal sealed class GenerateInterfaceAttribute : System.Attribute
{
}
