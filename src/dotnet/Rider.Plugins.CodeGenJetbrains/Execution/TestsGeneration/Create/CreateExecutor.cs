using JetBrains.ProjectModel;
using Rider.Plugins.CodeGenJetbrains.Extensions;
using Rider.Plugins.CodeGenJetbrains.FluidModels.TestsGeneration.Abstract;
using Rider.Plugins.CodeGenJetbrains.FluidModels.TestsGeneration.Models;
using Rider.Plugins.CodeGenJetbrains.FluidModels.TestsGeneration.Models.Create;

namespace Rider.Plugins.CodeGenJetbrains.Execution.TestsGeneration.Create;

public class CreateExecutor : BaseExecutor<CreateTestDto>
{
    protected override FTest ToFModel(
        CreateTestDto dto,
        IProjectFolder targetFolder,
        IProjectFolder testCasesFolder)
    {
        var folderNamespace = targetFolder.GetExpectedNamespace();

        var fModel = new FCreateTest
        {
            Request = new FTestRequest
            {
                Type = dto.RequestType.ToFType(),
                Endpoint = dto.RequestEndpoint
            },
            TestCase = new FCreateTestCase
            {
                Name = dto.CaseName,
                Type = GetFType(testCasesFolder.GetExpectedNamespace(), dto.FilesPrefix, "_Success")
            },
            BaseTests = new FCreateBaseTests
            {
                TheoryName = dto.FilesPrefix,
                Type = GetFType(folderNamespace, dto.FilesPrefix, "Tests"),
            },
            TestModel = new FCreateTestModel { Type = GetFType(folderNamespace, dto.FilesPrefix, "TestModel") },
            TestSuite = new FCreateTestSuite { Type = GetFType(folderNamespace, dto.FilesPrefix, "TestSuite") },
        };

        if (dto.AssertRequestInfo != null)
        {
            fModel.AssertRequest = new FTestAssertRequest
            {
                Endpoint = dto.AssertRequestInfo.RequestEndpoint,
                ResponseActType = dto.AssertRequestInfo.ResponseActType.ToFType(),
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
