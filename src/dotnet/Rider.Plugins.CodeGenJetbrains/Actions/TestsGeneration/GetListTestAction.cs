using JetBrains.Application.Parts;
using JetBrains.Application.UI.ActionsRevised.Menu;
using JetBrains.ProjectModel;
using Rider.Plugins.CodeGenJetbrains.Execution.TestsGeneration.GetList;

namespace Rider.Plugins.CodeGenJetbrains.Actions.TestsGeneration;

[Action("GetListTestActionId", "[GetList] Test")]
[SolutionComponent(Instantiation.ContainerAsyncAnyThreadUnsafe)]
public class GetListTestAction : SolutionFolderAction<GetListTestDto>
{
    protected override string DialogTitle => "Create test (GetList)";
}
