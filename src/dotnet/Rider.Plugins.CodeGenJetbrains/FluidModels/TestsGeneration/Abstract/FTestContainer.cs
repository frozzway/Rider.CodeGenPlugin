namespace Rider.Plugins.CodeGenJetbrains.FluidModels.TestsGeneration.Abstract;

public abstract class FTestContainer : IFTypeContainer
{
    public FTest Test { get; set; }
    public FType Type { get; set; }
}
