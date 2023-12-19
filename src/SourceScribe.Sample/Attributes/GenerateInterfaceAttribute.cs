using System;
using SourceScribe;

namespace SourceScribe.Sample.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
// [RegisterTypeMemberTemplate("RegisterTypeMemberTemplateAttribute")]
[TypeMemberTemplate("InterfaceGenerator.scriban")]
internal sealed class GenerateInterfaceAttribute : System.Attribute
{
}
