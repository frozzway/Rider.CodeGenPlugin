using System.Collections.Generic;

namespace Rider.Plugins.CodeGenJetbrains.FluidModels.ControllerGeneration;

public class FController
{
    public FType? Type { get; set; }
    public required FEntity Entity { get; set; }
    public required HashSet<string> Methods { get; set; }
    public required Dictionary<string, string> PermissionCodes { get; set; }
    public string? PermissionEnum { get; set; }
}
