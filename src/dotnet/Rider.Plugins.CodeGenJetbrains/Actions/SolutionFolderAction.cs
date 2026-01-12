using JetBrains.Application.DataContext;
using JetBrains.Application.UI.Actions;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;

namespace Rider.Plugins.CodeGenJetbrains.Actions;

public abstract class SolutionFolderAction<T> : BaseDialogAction<T>
{
    private static bool HasFolder(IDataContext context)
    {
        var project = context.GetData(ProjectModelDataConstants.PROJECT);
        if (project is null) return false;

        var item = context.GetData(ProjectModelDataConstants.PROJECT_MODEL_ELEMENT);
        if (item is not IProjectFolder) return false;

        return true;
    }

    public override bool Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
        => HasFolder(context);
}
