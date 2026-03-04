using Rider.Plugins.CodeGenJetbrains.FluidModels.TestsGeneration.Abstract;

namespace Rider.Plugins.CodeGenJetbrains.FluidModels.TestsGeneration.Models.GetGrid;

public class FGetGridBaseTests : FBaseTests
{
    public string? ResponseActTypeName { get; set; }
    public string? ResponseActTypeNameUnwrapped { get; set; }
    public bool ActWithQueryParams { get; set; }
    public bool RemoveMigrationEntities { get; set; }
}
