using JetBrains.ReSharper.Psi;

namespace Rider.Plugins.CodeGenJetbrains.Execution.Generation.TestsGeneration.Update;

public record UpdateTestDto(
    string FilesPrefix,
    string CaseName,
    string RequestEndpoint,
    IClass? RequestType,
    AssertRequestDto? AssertRequestInfo,
    IClass? Entity);

public record AssertRequestDto(
    IClass? ResponseAssertType,
    string RequestAssertEndpoint);
