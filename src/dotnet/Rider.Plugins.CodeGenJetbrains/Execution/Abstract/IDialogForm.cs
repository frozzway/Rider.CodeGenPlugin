using JetBrains.Annotations;
using JetBrains.Application.DataContext;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.Rider.Model.UIAutomation;

namespace Rider.Plugins.CodeGenJetbrains.Execution.Abstract;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public interface IDialogForm<T>
{
    BeDialog GetDialog(
        Lifetime lifetime,
        IDataContext context,
        string title);

    T GetDto();
}
