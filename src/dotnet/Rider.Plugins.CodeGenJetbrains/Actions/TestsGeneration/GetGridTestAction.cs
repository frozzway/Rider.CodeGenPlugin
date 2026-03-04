using JetBrains.Application.Parts;
using JetBrains.Application.UI.ActionsRevised.Menu;
using JetBrains.ProjectModel;
using Rider.Plugins.CodeGenJetbrains.Execution.Generation.TestsGeneration.GetGrid;

namespace Rider.Plugins.CodeGenJetbrains.Actions.TestsGeneration;

[Action("GetGridTestActionId", "[GetGrid] Test")]
[SolutionComponent(Instantiation.ContainerAsyncAnyThreadUnsafe)]
public class GetGridTestAction : SolutionFolderAction<GetGridTestDto>
{
    protected override string DialogTitle => "Create test (GetGrid)";
}
