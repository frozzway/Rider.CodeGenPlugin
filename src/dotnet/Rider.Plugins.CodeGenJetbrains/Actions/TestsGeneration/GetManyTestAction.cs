using JetBrains.Application.Parts;
using JetBrains.Application.UI.ActionsRevised.Menu;
using JetBrains.ProjectModel;
using Rider.Plugins.CodeGenJetbrains.Execution.Generation.TestsGeneration.GetMany;

namespace Rider.Plugins.CodeGenJetbrains.Actions.TestsGeneration;

[Action("GetManyTestActionId", "[GetMany] Test")]
[SolutionComponent(Instantiation.ContainerAsyncAnyThreadUnsafe)]
public class GetManyTestAction : SolutionFolderAction<GetManyTestDto>
{
    protected override string DialogTitle => "Create test (GetMany)";
}
