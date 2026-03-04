using Humanizer;
using JetBrains.ProjectModel;
using Rider.Plugins.CodeGenJetbrains.Execution.Generation.TestsGeneration.GetGrid;
using Rider.Plugins.CodeGenJetbrains.Extensions;
using Rider.Plugins.CodeGenJetbrains.FluidModels.TestsGeneration.Abstract;
using Rider.Plugins.CodeGenJetbrains.FluidModels.TestsGeneration.Models;
using Rider.Plugins.CodeGenJetbrains.FluidModels.TestsGeneration.Models.GetGrid;

namespace Rider.Plugins.CodeGenJetbrains.Execution.Generation.TestsGeneration.GetGrid;

public class GetGridExecutor : BaseExecutor<GetGridTestDto>
{
    protected override string TestModelTemplate => "TestsGeneration.GetGrid.TestModel.cs.liquid";
    protected override string TestCaseTemplate => "TestsGeneration.GetGrid.TestCase.cs.liquid";
    protected override string BaseTestTemplate => "TestsGeneration.GetGrid.BaseTests.cs.liquid";
    protected override string SubFolderName => "GetGrid";

    protected override FTest ToFModel(GetGridTestDto dto, IProjectFolder targetFolder, IProjectFolder testCasesFolder)
    {
        var folderNamespace = targetFolder.GetExpectedNamespace();

        var fModel = new FGetGridTest
        {
            Request = new FTestRequest
            {
                Type = dto.RequestType.ToFType(),
                Endpoint = dto.RequestEndpoint
            },
            TestCase = new FGetGridTestCase
            {
                Name = dto.CaseName,
                Type = GetFType(testCasesFolder.GetExpectedNamespace(), dto.FilesPrefix, "_Success")
            },
            BaseTests = new FGetGridBaseTests
            {
                TheoryName = dto.FilesPrefix,
                Type = GetFType(folderNamespace, dto.FilesPrefix, "Tests"),
                ActWithQueryParams = dto.ActWithQueryParams,
                RemoveMigrationEntities = dto.RemoveMigrationEntities,
                ResponseActTypeName = dto.ResponseActTypeName,
                ResponseActTypeNameUnwrapped = dto.ResponseActTypeNameUnwrapped
            },
            TestModel = new FGetGridTestModel { Type = GetFType(folderNamespace, dto.FilesPrefix, "TestModel") },
            TestSuite = new FGetGridTestSuite { Type = GetFType(folderNamespace, dto.FilesPrefix, "TestSuite") },
            Entity = new FTestEntity
            {
                Type = dto.Entity.ToFType(),
                PluralName = dto.Entity?.ShortName.Pluralize()
            }
        };

        fModel.BaseTests.Test = fModel;
        fModel.TestCase.Test = fModel;
        fModel.TestModel.Test = fModel;
        fModel.TestSuite.Test = fModel;
        return fModel;
    }
}
