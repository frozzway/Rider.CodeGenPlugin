using Humanizer;
using JetBrains.ProjectModel;
using Rider.Plugins.CodeGenJetbrains.Execution.Abstract;
using Rider.Plugins.CodeGenJetbrains.Extensions;
using Rider.Plugins.CodeGenJetbrains.FluidModels;
using Rider.Plugins.CodeGenJetbrains.FluidModels.TestsGeneration.Abstract;
using Rider.Plugins.CodeGenJetbrains.FluidModels.TestsGeneration.Models;
using Rider.Plugins.CodeGenJetbrains.FluidModels.TestsGeneration.Models.Update;

namespace Rider.Plugins.CodeGenJetbrains.Execution.Generation.TestsGeneration.Update;

public class UpdateExecutor(IContextAccessor contextAccessor) : BaseExecutor<UpdateTestDto>(contextAccessor)
{
    protected override string TestModelTemplate => "TestsGeneration.Update.TestModel.cs.liquid";
    protected override string TestCaseTemplate => "TestsGeneration.Update.TestCase.cs.liquid";
    protected override string BaseTestTemplate => "TestsGeneration.Update.BaseTests.cs.liquid";
    protected override string SubFolderName => "Update";

    protected override FTest ToFModel(
        UpdateTestDto dto,
        IProjectFolder targetFolder,
        IProjectFolder testCasesFolder)
    {
        var folderNamespace = targetFolder.GetExpectedNamespace();

        var fModel = new FUpdateTest
        {
            Request = new FTestRequest
            {
                Type = dto.RequestType.ToFType(),
                Endpoint = dto.RequestEndpoint
            },
            TestCase = new FUpdateTestCase
            {
                Name = dto.CaseName,
                Type = GetFType(testCasesFolder.GetExpectedNamespace(), dto.FilesPrefix, "_Success")
            },
            BaseTests = new FUpdateBaseTests
            {
                TheoryName = dto.FilesPrefix,
                Type = GetFType(folderNamespace, dto.FilesPrefix, "Tests"),
            },
            TestModel = new FUpdateTestModel { Type = GetFType(folderNamespace, dto.FilesPrefix, "TestModel") },
            TestSuite = new FUpdateTestSuite { Type = GetFType(folderNamespace, dto.FilesPrefix, "TestSuite") },
            Entity = new FEntity
            {
                Type = dto.Entity.ToFType(),
                PluralName = dto.Entity?.ShortName.Pluralize()
            }
        };

        if (dto.AssertRequestInfo != null)
        {
            fModel.AssertRequest = new FTestAssertRequest
            {
                Endpoint = dto.AssertRequestInfo.RequestAssertEndpoint,
                ResponseAssertType = dto.AssertRequestInfo.ResponseAssertType.ToFType()
            };
        }

        fModel.BaseTests.Test = fModel;
        fModel.TestCase.Test = fModel;
        fModel.TestModel.Test = fModel;
        fModel.TestSuite.Test = fModel;
        return fModel;
    }
}
