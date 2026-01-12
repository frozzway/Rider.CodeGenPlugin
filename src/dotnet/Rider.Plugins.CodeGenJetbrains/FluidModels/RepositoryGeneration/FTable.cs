namespace Rider.Plugins.CodeGenJetbrains.FluidModels.RepositoryGeneration;

public class FTable
{
    public string Schema { get; set; }
    public string Name { get; set; }
    public string FullName => $"{Schema}.{Name}";
}
