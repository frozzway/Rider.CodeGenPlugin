using Rider.Plugins.CodeGenJetbrains.FluidModels.TestsGeneration.Abstract;

namespace Rider.Plugins.CodeGenJetbrains.Execution.Common;

public static class FContainerExtensions
{
    public static string GetFileName(this FTestContainer container) => container.Type.Name + ".cs";
}
