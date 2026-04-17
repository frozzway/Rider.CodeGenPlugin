using JetBrains.Annotations;
using JetBrains.Application.DataContext;

namespace Rider.Plugins.CodeGenJetbrains.Execution.Abstract;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public interface IExecutor<T>
{
    void Execute(IDataContext dataContext, T dto);
}
