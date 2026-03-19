using JetBrains.ReSharper.Psi;

namespace Rider.Plugins.CodeGenJetbrains.Execution.Generation.TestsGeneration.GetMany;

public record GetManyTestDto(
    string FilesPrefix,
    string CaseName,
    string RequestEndpoint,
    bool RemoveMigrationEntities,
    string ResponseActTypeName,
    IClass? RequestType,
    IClass? Entity);
