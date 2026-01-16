using Rider.Plugins.CodeGenJetbrains.FluidModels.TestsGeneration.Abstract;

namespace Rider.Plugins.CodeGenJetbrains.FluidModels.TestsGeneration.Models.GetList;

public class FGetListBaseTests : FBaseTests
{
    public string? ResponseActTypeName { get; set; }
    public bool ActWithQueryParams { get; set; }
    public bool RemoveMigrationEntities { get; set; }
}
