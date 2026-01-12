using JetBrains.Application.Parts;
using JetBrains.Application.UI.ActionsRevised.Menu;
using JetBrains.ProjectModel;
using Rider.Plugins.CodeGenJetbrains.Execution.RepositoryGeneration;

namespace Rider.Plugins.CodeGenJetbrains.Actions;

[Action("GenerateRepositoryActionId", "Generate Repository")]
[SolutionComponent(Instantiation.ContainerAsyncAnyThreadUnsafe)]
public class GenerateRepositoryAction : SolutionFolderAction<RepositoryGenerationDto>
{
    protected override string DialogTitle => "Generate repository";
}
