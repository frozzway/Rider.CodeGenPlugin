using JetBrains.Application.Parts;
using JetBrains.Application.UI.ActionsRevised.Menu;
using JetBrains.ProjectModel;
using Rider.Plugins.CodeGenJetbrains.Execution.Generation.RepositoryGeneration;

namespace Rider.Plugins.CodeGenJetbrains.Actions;

[Action("GenerateRepositoryActionId", "Repository")]
[SolutionComponent(Instantiation.ContainerAsyncAnyThreadUnsafe)]
public class GenerateRepositoryAction : SolutionFolderAction<RepositoryGenerationDto>
{
    protected override string DialogTitle => "Generate repository";
}
