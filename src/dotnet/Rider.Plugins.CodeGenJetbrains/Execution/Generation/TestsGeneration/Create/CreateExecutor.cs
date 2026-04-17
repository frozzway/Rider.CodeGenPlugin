using JetBrains.ProjectModel;
using Rider.Plugins.CodeGenJetbrains.Execution.Abstract;
using Rider.Plugins.CodeGenJetbrains.Extensions;
using Rider.Plugins.CodeGenJetbrains.FluidModels.TestsGeneration.Abstract;
using Rider.Plugins.CodeGenJetbrains.FluidModels.TestsGeneration.Models;
using Rider.Plugins.CodeGenJetbrains.FluidModels.TestsGeneration.Models.Create;

namespace Rider.Plugins.CodeGenJetbrains.Execution.Generation.TestsGeneration.Create;

public class CreateExecutor(IContextAccessor contextAccessor) : BaseExecutor<CreateTestDto>(contextAccessor)
{
    protected override string TestModelTemplate => "TestsGeneration.Create.TestModel.cs.liquid";
    protected override string TestCaseTemplate => "TestsGeneration.Create.TestCase.cs.liquid";
    protected override string BaseTestTemplate => "TestsGeneration.Create.BaseTests.cs.liquid";
    protected override string SubFolderName => "Create";

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
                Endpoint = dto.AssertRequestInfo.RequestAssertEndpoint,
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
