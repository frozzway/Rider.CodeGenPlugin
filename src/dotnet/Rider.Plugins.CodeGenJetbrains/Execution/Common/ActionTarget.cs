using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace Rider.Plugins.CodeGenJetbrains.Execution.Common;

public abstract record ActionTarget
{
    /// <summary>
    /// Вызов из Solution Explorer по клику на директорию
    /// </summary>
    /// <param name="ProjectFolder">Выбранная директория</param>
    public sealed record Folder(IProjectFolder ProjectFolder) : ActionTarget;

    /// <summary>
    /// Вызов из редактора кода (Alt+Insert) внутри декларации класса
    /// </summary>
    /// <param name="ClassDeclaration">Декларация класса, внутри которого стоит каретка</param>
    public sealed record Declaration(IClassLikeDeclaration ClassDeclaration) : ActionTarget;
}
