namespace Rider.Plugins.CodeGenJetbrains.FluidModels.RepositoryGeneration;

public class FRepository : FGetList
{
    public FType Type { get; set; }
    public string[] Methods { get; set; }
}
