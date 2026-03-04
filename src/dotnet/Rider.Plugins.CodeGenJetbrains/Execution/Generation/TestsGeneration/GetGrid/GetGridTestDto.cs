using JetBrains.ReSharper.Psi;

namespace Rider.Plugins.CodeGenJetbrains.Execution.Generation.TestsGeneration.GetGrid;

public record GetGridTestDto(
    string FilesPrefix,
    string CaseName,
    string RequestEndpoint,
    bool RemoveMigrationEntities,
    bool ActWithQueryParams,
    string ResponseActTypeName,
    string ResponseActTypeNameUnwrapped,
    IClass? RequestType,
    IClass? Entity);
