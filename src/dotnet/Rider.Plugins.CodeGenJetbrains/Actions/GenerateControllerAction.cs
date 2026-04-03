using JetBrains.Application.Parts;
using JetBrains.Application.UI.ActionsRevised.Menu;
using JetBrains.ProjectModel;
using Rider.Plugins.CodeGenJetbrains.Execution.Generation.ControllerGeneration;

namespace Rider.Plugins.CodeGenJetbrains.Actions;

[Action("GenerateControllerActionId", "Controller")]
[SolutionComponent(Instantiation.ContainerAsyncAnyThreadUnsafe)]
public class GenerateControllerAction : SolutionFolderAction<ControllerGenerationDto>
{
    protected override string DialogTitle => "Generate controller";
}
