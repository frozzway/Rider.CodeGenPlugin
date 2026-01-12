using System.Linq;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Resources.Shell;
using Rider.Plugins.CodeGenJetbrains.Extensions;

namespace Rider.Plugins.CodeGenJetbrains.Execution.Common;

public class SolutionTypeElementsAccessor
{
    public IClass[] Classes { get; private set; }

    public SolutionTypeElementsAccessor()
    {
        var solution = Shell.Instance.GetComponent<SolutionsManager>().Solution;
        var items = solution!.GetAllTypeElements();
        Classes = items.OfType<IClass>().OrderBy(i => i.ShortName).ToArray();
    }
}
