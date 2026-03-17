namespace Rider.Plugins.CodeGenJetbrains.FluidModels.CommandGeneration;

public class FBase
{
    public bool UseLanguageExt { get; set; }
    public string ReturnType { get; set; }
    public FCommand Command { get; set; }
    public FHandler Handler { get; set; }
    public FResult Result { get; set; }
}
