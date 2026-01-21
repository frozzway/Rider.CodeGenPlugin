namespace Rider.Plugins.CodeGenJetbrains.FluidModels;

public interface IFTypeContainer
{
    public FType Type { get; set; }

    public string GetFileName() => Type.Name + ".cs";
}
