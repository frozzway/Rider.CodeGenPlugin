using System.IO;
using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Rider.Plugins.CodeGenJetbrains.Extensions;

namespace Rider.Plugins.CodeGenJetbrains.Execution.Common;

public class FluidRenderer(object templateModel, string templateResourceName)
{
    private static readonly Assembly ResourcesAssembly = Assembly.GetAssembly(typeof(FluidRenderer))!;
    private static readonly string ResourcesPrefix = $"{ResourcesAssembly.GetName().Name}.Templates";
    private readonly EmbeddedFileProvider _fileProvider = new(ResourcesAssembly, ResourcesPrefix);

    private readonly string _templateText = LoadTemplate(templateResourceName);
    public string RenderContent() => FluidExtensions.RenderContent(_templateText, templateModel, _fileProvider);

    private static string LoadTemplate(string templateResourceName)
    {
        // Формат имени: {DefaultNamespace}.{FolderPath}.{FileName}
        // Например: "Plugin.Backend.Templates.ClassTemplate.cs.liquid"
        var resourceName = $"{ResourcesPrefix}.{templateResourceName}";

        using var stream = ResourcesAssembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            var availableResources = ResourcesAssembly.GetManifestResourceNames();
            throw new FileNotFoundException(
                $"Template '{resourceName}' not found. Available: {string.Join(", ", availableResources)}");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
