using JetBrains.Application.DataContext;
using JetBrains.Application.UI.Actions;
using JetBrains.Application.UI.ActionsRevised.Menu;
using JetBrains.IDE.UI;
using JetBrains.IDE.UI.Extensions;
using JetBrains.Lifetimes;
using Microsoft.Extensions.DependencyInjection;
using Rider.Plugins.CodeGenJetbrains.Execution.Abstract;

namespace Rider.Plugins.CodeGenJetbrains.Actions;

public abstract class BaseDialogAction<T> : IExecutableAction
{
    protected abstract string DialogTitle { get; }

    public virtual bool Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
    {
        return true;
    }

    public void Execute(IDataContext context, DelegateExecute nextExecute)
    {
        var dialogHost = context.GetComponent<IDialogHost>();
        var contextSnapshot = new DataContextSnapshot(context);

        var scope = DependencyInjection.ServiceProvider.CreateScope();

        var dialogForm = scope.ServiceProvider.GetRequiredService<IDialogForm<T>>();
        var executor = scope.ServiceProvider.GetRequiredService<IExecutor<T>>();

        dialogHost.Show(
            getDialog: lt =>
                dialogForm.GetDialog(lt, context, DialogTitle)
                    .WithOkButton(lt, () =>
                    {
                        executor.Execute(contextSnapshot, dialogForm.GetDto());
                        scope.Dispose();
                    })
                    .WithCancelButton(lt, () => scope.Dispose()),
            parentLifetime: Lifetime.Eternal
        );
    }
}
