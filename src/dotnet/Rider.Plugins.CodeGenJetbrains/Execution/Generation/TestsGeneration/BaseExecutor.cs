using System;
using JetBrains.Application.DataContext;
using JetBrains.ProjectModel;
using Rider.Plugins.CodeGenJetbrains.Execution.Abstract;
using Rider.Plugins.CodeGenJetbrains.Execution.Common;
using Rider.Plugins.CodeGenJetbrains.Extensions;
using Rider.Plugins.CodeGenJetbrains.FluidModels;
using Rider.Plugins.CodeGenJetbrains.FluidModels.TestsGeneration.Abstract;

namespace Rider.Plugins.CodeGenJetbrains.Execution.Generation.TestsGeneration;

public abstract class BaseExecutor<T>(IContextAccessor contextAccessor) : IExecutor<T>
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
        if (contextAccessor.Target is not ActionTarget.Folder targetFolder)
            throw new InvalidOperationException();

        GenerateImpl(targetFolder.ProjectFolder, dto);
    }

    protected virtual void PreGenerationImpl(IProjectFolder targetFolder, T dto) {}
    protected virtual void PostGenerationImpl(IProjectFolder targetFolder, T dto) {}

    protected virtual void GenerateImpl(IProjectFolder selectedFolder, T dto)
    {
        PreGenerationImpl(selectedFolder, dto);
        const string testCaseFolderName = "TestCases";

        if (selectedFolder.Name != SubFolderName)
        {
            selectedFolder = selectedFolder.GetSubFoldersWithLock(SubFolderName).FirstOrDefault()
                             ?? selectedFolder.CreateFolder(SubFolderName);
        }

        var testCasesFolder = selectedFolder.GetSubFoldersWithLock(testCaseFolderName).FirstOrDefault()
                              ?? selectedFolder.CreateFolder(testCaseFolderName);

        var fModel = ToFModel(dto, selectedFolder, testCasesFolder);

        selectedFolder.CreateFileFromModel(fModel.TestModel, TestModelTemplate);
        selectedFolder.CreateFileFromModel(fModel.TestSuite, TestSuiteTemplate);
        selectedFolder.CreateFileFromModel(fModel.BaseTests, BaseTestTemplate);
        testCasesFolder.CreateFileFromModel(fModel.TestCase, TestCaseTemplate);
        PostGenerationImpl(selectedFolder, dto);
    }

    protected static FType GetFType(string namespaceStr, string prefix, string name)
        => new()
        {
            Namespace = namespaceStr,
            Name = prefix + name
        };
}
