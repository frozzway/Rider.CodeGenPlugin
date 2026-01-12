using JetBrains.ReSharper.Psi;

namespace Rider.Plugins.CodeGenJetbrains.Execution.TestsGeneration.Create;

public record CreateTestDto(
    string FilesPrefix,
    string CaseName,
    string RequestEndpoint,
    IClass? RequestType,
    AssertRequestDto? AssertRequestInfo);

public record AssertRequestDto(
    IClass? ResponseActType,
    IClass? ResponseAssertType,
    string RequestEndpoint);
