using System;
using JetBrains.Application.DataContext;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using Rider.Plugins.CodeGenJetbrains.Execution.Abstract;
using Rider.Plugins.CodeGenJetbrains.Execution.Common;
using Rider.Plugins.CodeGenJetbrains.Extensions;
using Rider.Plugins.CodeGenJetbrains.FluidModels;
using Rider.Plugins.CodeGenJetbrains.FluidModels.TestsGeneration.Abstract;

namespace Rider.Plugins.CodeGenJetbrains.Execution.TestsGeneration;

public abstract class BaseExecutor<T> : IExecutor<T>
{
    private const string TestSuiteTemplate = "TestsGeneration.TestSuite.cs.liquid";
    protected abstract string TestModelTemplate { get; }
    protected abstract string TestCaseTemplate { get; }
    protected abstract string BaseTestTemplate { get; }
    protected abstract string SubFolderName { get; }

    protected abstract FTest ToFModel(
        T dto,
        IProjectFolder targetFolder,
        IProjectFolder testCasesFolder);

    public void Execute(IDataContext context, T dto)
    {
        var targetElement = context.GetData(ProjectModelDataConstants.PROJECT_MODEL_ELEMENT);

        if (targetElement is not IProjectFolder folder)
            throw new InvalidOperationException();

        GenerateImpl(folder, dto);
    }

    protected virtual void GenerateImpl(IProjectFolder selectedFolder, T dto)
    {
        const string testCaseFolderName = "TestCases";

        if (selectedFolder.Name != SubFolderName)
        {
            selectedFolder = selectedFolder.GetSubFoldersWithLock(SubFolderName).FirstOrDefault()
                             ?? selectedFolder.CreateFolder(SubFolderName);
        }

        var testCasesFolder = selectedFolder.GetSubFoldersWithLock(testCaseFolderName).FirstOrDefault()
                              ?? selectedFolder.CreateFolder(testCaseFolderName);

        var fModel = ToFModel(dto, selectedFolder, testCasesFolder);

        GenerateFileFromModel(selectedFolder, fModel.TestModel, TestModelTemplate);
        GenerateFileFromModel(selectedFolder, fModel.TestSuite, TestSuiteTemplate);
        GenerateFileFromModel(selectedFolder, fModel.BaseTests, BaseTestTemplate);
        GenerateFileFromModel(testCasesFolder, fModel.TestCase, TestCaseTemplate);
    }

    private static void GenerateFileFromModel(
        IProjectFolder folder,
        FTestContainer model,
        string templateFilePath)
    {
        var fileName = model.GetFileName();

        if (folder.GetSubFilesWithLock(fileName).Any())
            return;

        var renderer = new FluidRenderer(model, templateFilePath);

        folder.CreateFileWithContent(fileName, fileContent: renderer.RenderContent())
            .TryAddToGit()
            .FixImportsInFile(withProgress: true)
            .MarkDirtyAndRefresh();
    }

    protected static FType GetFType(string namespaceStr, string prefix, string name)
        => new()
        {
            Namespace = namespaceStr,
            Name = prefix + name
        };
}
