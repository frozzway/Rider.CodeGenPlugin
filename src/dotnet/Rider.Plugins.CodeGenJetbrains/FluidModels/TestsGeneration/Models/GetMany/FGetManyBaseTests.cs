using Rider.Plugins.CodeGenJetbrains.FluidModels.TestsGeneration.Abstract;

namespace Rider.Plugins.CodeGenJetbrains.FluidModels.TestsGeneration.Models.GetMany;

public class FGetManyBaseTests : FBaseTests
{
    public string? ResponseActTypeName { get; set; }
    public bool RemoveMigrationEntities { get; set; }
}
