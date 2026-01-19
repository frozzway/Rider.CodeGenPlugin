using JetBrains.ReSharper.Psi;

namespace Rider.Plugins.CodeGenJetbrains.Execution.TestsGeneration.Update;

public record UpdateTestDto(
    string FilesPrefix,
    string CaseName,
    string RequestEndpoint,
    IClass? RequestType,
    AssertRequestDto? AssertRequestInfo);

public record AssertRequestDto(
    IClass? ResponseAssertType,
    string RequestAssertEndpoint);
