using System.Collections.Generic;
using System.Linq;
using Humanizer;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Web.AspRouteTemplates.EndpointsProvider.AspNetHttpEndpoints;
using JetBrains.ReSharper.Psi;
using JetBrains.Util;
using Rider.Plugins.CodeGenJetbrains.Extensions;
using Rider.Plugins.CodeGenJetbrains.FluidModels;

namespace Rider.Plugins.CodeGenJetbrains.Execution.Common;

public static class ExecutionUtils
{
    public static IProjectFolder CreateFileFromModel(
        this IProjectFolder folder,
        IFTypeContainer model,
        string templatePath)
    {
        var fileName = model.GetFileName();

        if (folder.GetSubFilesWithLock(fileName).Any())
            return folder;

        var renderer = new FluidRenderer(model, templatePath);

        folder.CreateFileWithContent(fileName, fileContent: renderer.RenderContent())
            .TryAddToGit()
            .FixImportsInFile(withProgress: true)
            .ApplyFormatter()
            .MarkDirtyAndRefresh();

        return folder;
    }

    public static IClass? FindEntityUsingControllerName(
        this AspNetHttpEndpoint endpoint,
        IReadOnlyCollection<IClass> classes)
    {
        var targetEntityName = endpoint.Controller.ShortName
            .TrimFromStart("Admin")
            .TrimFromEnd("Controller")
            .Singularize();

        var entity = classes
            .FirstOrDefault(i => i.GetContainingNamespace().QualifiedName.Contains("Core")
                                 && i.ShortName == targetEntityName);
        return entity;
    }
}
