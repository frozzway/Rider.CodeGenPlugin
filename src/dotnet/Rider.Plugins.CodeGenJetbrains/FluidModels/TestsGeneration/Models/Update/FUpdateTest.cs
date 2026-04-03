using Rider.Plugins.CodeGenJetbrains.FluidModels.TestsGeneration.Abstract;

namespace Rider.Plugins.CodeGenJetbrains.FluidModels.TestsGeneration.Models.Update;

public class FUpdateTest : FTest
{
    public FTestRequest Request { get; set; }
    public FEntity Entity { get; set; }
    public FTestAssertRequest? AssertRequest { get; set; }
}
