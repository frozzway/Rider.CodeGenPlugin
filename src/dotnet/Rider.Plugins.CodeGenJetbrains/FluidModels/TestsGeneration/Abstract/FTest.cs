namespace Rider.Plugins.CodeGenJetbrains.FluidModels.TestsGeneration.Abstract;

public abstract class FTest
{
    public FBaseTests BaseTests { get; set; }
    public FTestCase TestCase { get; set; }
    public FTestSuite TestSuite { get; set; }
    public FTestModel TestModel { get; set; }
}
