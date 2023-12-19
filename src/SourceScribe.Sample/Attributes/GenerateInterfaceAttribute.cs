using System;
using SourceScribe;

namespace SourceScribe.Sample.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
[RegisterTypeMemberTemplate("InterfaceGenerator.scriban")]
//[TypeMemberTemplate("InterfaceGenerator.scriban")]
internal sealed class GenerateInterfaceAttribute : System.Attribute
{
}
