using JetBrains.Application.DataContext;
using JetBrains.Application.Parts;
using JetBrains.Application.UI.Actions;
using JetBrains.Application.UI.ActionsRevised.Menu;
using JetBrains.IDE.UI;
using JetBrains.IDE.UI.Extensions;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using JetBrains.Rider.Model.UIAutomation;
using JetBrains.Util;

namespace Rider.Plugins.CodeGenJetbrains.Sandbox;

[Action("ActionId", "SampleAction")]
[SolutionComponent(Instantiation.ContainerAsyncAnyThreadUnsafe)]
public class SampleAction : IExecutableAction
{
    public bool Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
    {
        var project = context.GetData(ProjectModelDataConstants.PROJECT);
        if (project is null) return false;

        var item = context.GetData(ProjectModelDataConstants.PROJECT_MODEL_ELEMENT);
        if (item is not IProjectFolder folder) return false;

        return true;
    }

    public void Execute(IDataContext context, DelegateExecute nextExecute)
    {
        var project = context.GetData(ProjectModelDataConstants.PROJECT);
        if (project is null) return;

        var item = context.GetData(ProjectModelDataConstants.PROJECT_MODEL_ELEMENT);
        if (item is not IProjectFolder folder) return;

        var dialogHost = context.GetComponent<IDialogHost>();

        BeGrid grid;

        dialogHost.Show(
            getDialog: lt => BeControls.GetDialog(
                    dialogContent: grid = SandboxElements.GetGrid(lt, context, folder),
                    title: "Title123",
                    id: nameof(SampleAction))
                .WithOkButton(lt, () => MessageBox.ShowInfo("have-a-nice-day:)"))
                .WithCancelButton(lt),
            parentLifetime: Lifetime.Eternal);
    }
}
