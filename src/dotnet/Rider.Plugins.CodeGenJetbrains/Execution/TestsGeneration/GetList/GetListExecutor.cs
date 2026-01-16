using Humanizer;
using JetBrains.ProjectModel;
using Rider.Plugins.CodeGenJetbrains.Extensions;
using Rider.Plugins.CodeGenJetbrains.FluidModels.TestsGeneration.Abstract;
using Rider.Plugins.CodeGenJetbrains.FluidModels.TestsGeneration.Models;
using Rider.Plugins.CodeGenJetbrains.FluidModels.TestsGeneration.Models.GetList;

namespace Rider.Plugins.CodeGenJetbrains.Execution.TestsGeneration.GetList;

public class GetListExecutor : BaseExecutor<GetListTestDto>
{
    protected override string TestModelTemplate => "TestsGeneration.GetList.TestModel.cs.liquid";
    protected override string TestCaseTemplate => "TestsGeneration.GetList.TestCase.cs.liquid";
    protected override string BaseTestTemplate => "TestsGeneration.GetList.BaseTests.cs.liquid";

    protected override FTest ToFModel(GetListTestDto dto, IProjectFolder targetFolder, IProjectFolder testCasesFolder)
    {
        var folderNamespace = targetFolder.GetExpectedNamespace();

        var fModel = new FGetListTest
        {
            Request = new FTestRequest
            {
                Type = dto.RequestType.ToFType(),
                Endpoint = dto.RequestEndpoint
            },
            TestCase = new FGetListTestCase
            {
                Name = dto.CaseName,
                Type = GetFType(testCasesFolder.GetExpectedNamespace(), dto.FilesPrefix, "_Success")
            },
            BaseTests = new FGetListBaseTests
            {
                TheoryName = dto.FilesPrefix,
                Type = GetFType(folderNamespace, dto.FilesPrefix, "Tests"),
                ActWithQueryParams = dto.ActWithQueryParams,
                RemoveMigrationEntities = dto.RemoveMigrationEntities,
                ResponseActTypeName = dto.ResponseActTypeName
            },
            TestModel = new FGetListTestModel { Type = GetFType(folderNamespace, dto.FilesPrefix, "TestModel") },
            TestSuite = new FGetListTestSuite { Type = GetFType(folderNamespace, dto.FilesPrefix, "TestSuite") },
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
