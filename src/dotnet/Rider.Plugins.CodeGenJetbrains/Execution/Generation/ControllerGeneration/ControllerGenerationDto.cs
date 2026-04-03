using System.Collections.Generic;
using JetBrains.ReSharper.Psi;

namespace Rider.Plugins.CodeGenJetbrains.Execution.Generation.ControllerGeneration;

public class ControllerGenerationDto
{
    public required string ControllerName { get; init; }
    public required IClass Entity { get; init; }
    public string? EntitySummaryName { get; init; }
    public required HashSet<string> Methods { get; init; }
    public required Dictionary<string, string> PermissionCodes { get; init; }
    public string? PermissionEnum { get; init; }
}
