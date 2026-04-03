using Humanizer;
using JetBrains.ProjectModel;
using Rider.Plugins.CodeGenJetbrains.Extensions;
using Rider.Plugins.CodeGenJetbrains.FluidModels;
using Rider.Plugins.CodeGenJetbrains.FluidModels.TestsGeneration.Abstract;
using Rider.Plugins.CodeGenJetbrains.FluidModels.TestsGeneration.Models;
using Rider.Plugins.CodeGenJetbrains.FluidModels.TestsGeneration.Models.GetMany;

namespace Rider.Plugins.CodeGenJetbrains.Execution.Generation.TestsGeneration.GetMany;

public class GetManyExecutor : BaseExecutor<GetManyTestDto>
{
    protected override string TestModelTemplate => "TestsGeneration.GetMany.TestModel.cs.liquid";
    protected override string TestCaseTemplate => "TestsGeneration.GetMany.TestCase.cs.liquid";
    protected override string BaseTestTemplate => "TestsGeneration.GetMany.BaseTests.cs.liquid";
    protected override string SubFolderName => "GetMany";

    protected override FTest ToFModel(GetManyTestDto dto, IProjectFolder targetFolder, IProjectFolder testCasesFolder)
    {
        var folderNamespace = targetFolder.GetExpectedNamespace();

        var fModel = new FGetManyTest
        {
            Request = new FTestRequest
            {
                Type = dto.RequestType.ToFType(),
                Endpoint = dto.RequestEndpoint
            },
            TestCase = new FGetManyTestCase
            {
                Name = dto.CaseName,
                Type = GetFType(testCasesFolder.GetExpectedNamespace(), dto.FilesPrefix, "_Success")
            },
            BaseTests = new FGetManyBaseTests
            {
                TheoryName = dto.FilesPrefix,
                Type = GetFType(folderNamespace, dto.FilesPrefix, "Tests"),
                RemoveMigrationEntities = dto.RemoveMigrationEntities,
                ResponseActTypeName = dto.ResponseActTypeName
            },
            TestModel = new FGetManyTestModel { Type = GetFType(folderNamespace, dto.FilesPrefix, "TestModel") },
            TestSuite = new FGetManyTestSuite { Type = GetFType(folderNamespace, dto.FilesPrefix, "TestSuite") },
            Entity = new FEntity
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
