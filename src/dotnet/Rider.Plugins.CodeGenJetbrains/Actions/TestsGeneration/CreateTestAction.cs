using JetBrains.Application.Parts;
using JetBrains.Application.UI.ActionsRevised.Menu;
using JetBrains.ProjectModel;
using Rider.Plugins.CodeGenJetbrains.Execution.Generation.TestsGeneration.Create;

namespace Rider.Plugins.CodeGenJetbrains.Actions.TestsGeneration;

[Action("CreateTestActionId", "[Create] Test")]
[SolutionComponent(Instantiation.ContainerAsyncAnyThreadUnsafe)]
public class CreateTestAction : SolutionFolderAction<CreateTestDto>
{
    protected override string DialogTitle => "Create test (Create)";
}
