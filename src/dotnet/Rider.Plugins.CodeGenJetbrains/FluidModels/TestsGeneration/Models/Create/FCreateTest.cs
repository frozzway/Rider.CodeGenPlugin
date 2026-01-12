using Rider.Plugins.CodeGenJetbrains.FluidModels.TestsGeneration.Abstract;

namespace Rider.Plugins.CodeGenJetbrains.FluidModels.TestsGeneration.Models.Create;

public class FCreateTest : FTest
{
    public FTestRequest Request { get; set; }
    public FTestAssertRequest? AssertRequest { get; set; }
}
