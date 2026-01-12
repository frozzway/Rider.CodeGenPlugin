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
    private const string TestModelTemplate = "TestsGeneration.TestModel.cs.liquid";
    private const string TestSuiteTemplate = "TestsGeneration.TestSuite.cs.liquid";
    private const string TestCaseTemplate = "TestsGeneration.TestCase.cs.liquid";
    private const string BaseTestTemplate = "TestsGeneration.BaseTests.cs.liquid";

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

    protected virtual void GenerateImpl(IProjectFolder targetFolder, T dto)
    {
        const string testCaseFolderName = "TestCases";

        var testCasesFolder = targetFolder.GetSubFoldersWithLock(testCaseFolderName).FirstOrDefault()
                              ?? targetFolder.CreateFolder(testCaseFolderName);

        var fModel = ToFModel(dto, targetFolder, testCasesFolder);

        GenerateFileFromModel(targetFolder, fModel.TestModel, TestModelTemplate);
        GenerateFileFromModel(targetFolder, fModel.TestSuite, TestSuiteTemplate);
        GenerateFileFromModel(targetFolder, fModel.BaseTests, BaseTestTemplate);
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
