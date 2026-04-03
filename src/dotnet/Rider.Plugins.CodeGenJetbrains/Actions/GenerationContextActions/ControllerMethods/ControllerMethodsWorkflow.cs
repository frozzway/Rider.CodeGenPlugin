using JetBrains.Application.DataContext;
using JetBrains.ReSharper.Feature.Services.Generate.Actions;
using JetBrains.ReSharper.Feature.Services.Generate.Workflows;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.UI.Icons;
using Rider.Plugins.CodeGenJetbrains.Extensions;

namespace Rider.Plugins.CodeGenJetbrains.Actions.GenerationContextActions.ControllerMethods;

public class ControllerMethodsWorkflow(IconId icon) : GenerateCodeWorkflowBase(
    kind: GenerationActionsKinds.ControllerMethods,
    icon: icon,
    title: Title,
    actionGroup: GenerateActionGroup.CLR_LANGUAGE,
    windowTitle: Title,
    description: Title,
    actionId: null), IGenerateActionWorkflow
{
    private const string Title = "Controller Methods";

    void IGenerateActionWorkflow.Execute(IDataContext context)
    {
        var action = new GenerateControllerAction();
        action.Execute(context, () => {});
    }

    public override double Order => 100;

    public override bool IsAvailable(IDataContext dataContext)
        => CaretContextUtil.IsCaretInsideCSharpClassButNotMethod(dataContext, out _);

    public override bool IsEnabled(ITreeNode context) => true;
}
