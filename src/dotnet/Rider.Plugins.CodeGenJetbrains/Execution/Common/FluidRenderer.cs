using System;
using System.IO;
using System.Reflection;
using Fluid;
using Microsoft.Extensions.FileProviders;

namespace Rider.Plugins.CodeGenJetbrains.Execution.Common;

public class FluidRenderer(object templateModel, string templateResourceName)
{
    private static readonly Assembly ResourcesAssembly = Assembly.GetAssembly(typeof(FluidRenderer))!;
    private static readonly string ResourcesPrefix = $"{ResourcesAssembly.GetName().Name}.Templates";

    private readonly string _templateText = LoadTemplate(templateResourceName);
    public string RenderContent() => RenderContent(_templateText, templateModel);

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

    private static string RenderContent(string templateText, object templateModel)
    {
        var parser = new FluidParser();
        if (!parser.TryParse(templateText, out var template, out var error))
            throw new ArgumentException($"Parse error: {error}");

        var options = new TemplateOptions
        {
            MemberAccessStrategy = new UnsafeMemberAccessStrategy { IgnoreCasing = true },
            FileProvider = new EmbeddedFileProvider(ResourcesAssembly, ResourcesPrefix)
        };

        var context = new TemplateContext(templateModel, options);
        return template.Render(context);
    }
}
