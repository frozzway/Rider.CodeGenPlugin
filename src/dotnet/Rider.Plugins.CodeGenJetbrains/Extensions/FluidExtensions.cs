using System;
using Fluid;
using Microsoft.Extensions.FileProviders;

namespace Rider.Plugins.CodeGenJetbrains.Extensions;

public static class FluidExtensions
{
    public static string RenderContent(
        string templateText,
        object templateModel,
        EmbeddedFileProvider? fileProvider = null)
    {
        var parser = new FluidParser();
        if (!parser.TryParse(templateText, out var template, out var error))
            throw new ArgumentException($"Parse error: {error}");

        var options = new TemplateOptions
        {
            MemberAccessStrategy = new UnsafeMemberAccessStrategy { IgnoreCasing = true },
        };

        if (fileProvider is not null)
            options.FileProvider = fileProvider;

        var context = new TemplateContext(templateModel, options);
        return template.Render(context);
    }
}
