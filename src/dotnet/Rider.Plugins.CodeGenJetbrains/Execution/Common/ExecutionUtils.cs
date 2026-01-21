using JetBrains.ProjectModel;
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
}
