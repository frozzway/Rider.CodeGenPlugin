using System.Linq;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Resources.Shell;
using Rider.Plugins.CodeGenJetbrains.Extensions;

namespace Rider.Plugins.CodeGenJetbrains.Execution.Common;

public class SolutionTypeElementsAccessor
{
    public ITypeElement[] TypeElements { get; }

    public IClass[] Classes => TypeElements.OfType<IClass>().ToArray();
    public IEnum[] Enums => TypeElements.OfType<IEnum>().ToArray();

    public SolutionTypeElementsAccessor()
    {
        var solution = Shell.Instance.GetComponent<SolutionsManager>().Solution;
        var items = solution!.GetAllTypeElements().OrderBy(i => i.ShortName).ToArray();
        TypeElements = items;
    }
}
