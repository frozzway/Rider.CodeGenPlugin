using JetBrains.Application.Parts;
using JetBrains.Application.UI.ActionsRevised.Menu;
using JetBrains.ProjectModel;
using Rider.Plugins.CodeGenJetbrains.Execution.Generation.CommandGeneration;

namespace Rider.Plugins.CodeGenJetbrains.Actions;

[Action("GenerateCommandActionId", "Command/Query")]
[SolutionComponent(Instantiation.ContainerAsyncAnyThreadUnsafe)]
public class GenerateCommandAction : SolutionFolderAction<CommandGenerationDto>
{
    protected override string DialogTitle => "Generate command";
}
