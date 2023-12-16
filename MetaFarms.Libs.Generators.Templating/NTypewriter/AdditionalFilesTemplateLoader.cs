using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

namespace MetaFarms.Libs.Generators.Templating.NTypewriter;

/// <summary>
/// Implementation of <see cref="ITemplateLoader"/> for scriban that will load include templates from the current AdditionalFilesProvider.
/// </summary>
internal class AdditionalFilesTemplateLoader : ITemplateLoader
{
    private readonly ImmutableArray<(string fileName, string filePath, string template)> _files;

    public AdditionalFilesTemplateLoader(ImmutableArray<(string fileName, string filePath, string template)> files)
    {
        _files = files;
    }

    public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
    {
        var path = _files.FirstOrDefault(x => x.fileName.EndsWith(templateName));

        return path.fileName;
    }

    public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
    {
        var file = _files.FirstOrDefault(x => x.fileName == templatePath);

        return file.template ?? string.Empty;
    }

    public ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath) =>
        new (Load(context, callerSpan, templatePath));
}
