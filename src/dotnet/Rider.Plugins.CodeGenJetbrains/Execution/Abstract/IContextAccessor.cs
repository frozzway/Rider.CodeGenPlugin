using JetBrains.Application.DataContext;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using Rider.Plugins.CodeGenJetbrains.Execution.Common;

namespace Rider.Plugins.CodeGenJetbrains.Execution.Abstract;

public interface IContextAccessor
{
    void Initialize(IDataContext contextSnapshot);

    /// <summary>
    /// Специфичное состояние вызова
    /// </summary>
    ActionTarget Target { get; }

    IDataContext Snapshot { get; }
    ISolution Solution { get; }
    IProject Project { get; }
    IPsiServices PsiServices { get; }
}
