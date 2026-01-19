using JetBrains.Application.Parts;
using JetBrains.Application.UI.ActionsRevised.Menu;
using JetBrains.ProjectModel;
using Rider.Plugins.CodeGenJetbrains.Execution.TestsGeneration.Update;

namespace Rider.Plugins.CodeGenJetbrains.Actions.TestsGeneration;

[Action("UpdateTestActionId", "[Update] Test")]
[SolutionComponent(Instantiation.ContainerAsyncAnyThreadUnsafe)]
public class UpdateTestAction : SolutionFolderAction<UpdateTestDto>
{
    protected override string DialogTitle => "Create test (Update)";
}
