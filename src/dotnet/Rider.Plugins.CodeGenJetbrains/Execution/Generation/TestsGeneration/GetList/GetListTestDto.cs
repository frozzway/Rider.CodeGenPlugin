using JetBrains.ReSharper.Psi;

namespace Rider.Plugins.CodeGenJetbrains.Execution.Generation.TestsGeneration.GetList;

public record GetListTestDto(
    string FilesPrefix,
    string CaseName,
    string RequestEndpoint,
    bool RemoveMigrationEntities,
    bool ActWithQueryParams,
    string ResponseActTypeName,
    IClass? RequestType,
    IClass? Entity);
