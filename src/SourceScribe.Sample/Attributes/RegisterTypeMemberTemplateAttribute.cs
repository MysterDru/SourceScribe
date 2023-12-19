using System;

namespace SourceScribe.Sample.Attributes;

/// <summary>
/// Provides a way to register a template to a custom attribute. Allows for better re-usability of templates in the consuming project.
/// An attribute registered in this manner can be used in place of [TypeMemberTemplateAttribute("SomeTemplate.scriban")].
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
internal sealed class RegisterTypeMemberTemplateAttribute : System.Attribute
{
    public RegisterTypeMemberTemplateAttribute(string templateName)
    {
    }
}
