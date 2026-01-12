using System.Collections.Generic;
using System.Linq;
using JetBrains.DataFlow;
using JetBrains.IDE.UI;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.Rider.Model.UIAutomation;
using PsiExtensions = Rider.Plugins.CodeGenJetbrains.Extensions.PsiExtensions;

namespace Rider.Plugins.CodeGenJetbrains.UI.Components;

public class SearchableClassPicker
{
    private readonly SearchableModalInputField<IClass> _searchableModalInputField;

    public SearchableClassPicker(Lifetime lifetime, IDialogHost dialogHost, IEnumerable<IClass> classes)
    {
        var dialogSize = new BeControlSizePredefined(
            width: new BeSize(BeControlSizeType.HUGE),
            height: new BeSize(BeControlSizeType.LARGE));
        _searchableModalInputField = SearchableModalInputField<IClass>.Create(
            lifetime, dialogHost, classes,
            presentValue: item => item.ShortName,
            dialogHintPresentValue: item => item.GetContainingNamespace().QualifiedName,
            size: dialogSize);
    }

    public SearchableClassPicker(Lifetime lifetime, IDialogHost dialogHost)
        : this(lifetime, dialogHost, GetAllClassesInSolution())
    {
    }

    /// <summary>
    /// Основной контрол для использования в UI.
    /// </summary>
    public BeControl Control => _searchableModalInputField.Control;

    /// <summary>
    /// Property со значением выбранного класса
    /// </summary>
    public IProperty<IClass> Value => _searchableModalInputField.Value;

    private static IEnumerable<IClass> GetAllClassesInSolution()
    {
        var solution = Shell.Instance.GetComponent<SolutionsManager>().Solution;
        var items = PsiExtensions.GetAllTypeElements(solution);
        return items.OfType<IClass>();
    }
}
