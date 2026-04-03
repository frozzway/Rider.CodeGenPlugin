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

public class SearchableTypeElementPicker<T> where T: ITypeElement
{
    private readonly SearchableModalInputField<T> _searchableModalInputField;

    public SearchableTypeElementPicker(Lifetime lifetime, IDialogHost dialogHost, IEnumerable<T> classes)
    {
        var dialogSize = new BeControlSizePredefined(
            width: new BeSize(BeControlSizeType.HUGE),
            height: new BeSize(BeControlSizeType.LARGE));
        _searchableModalInputField = SearchableModalInputField<T>.Create(
            lifetime, dialogHost, classes,
            presentValue: item => item.ShortName,
            dialogHintPresentValue: item => item.GetContainingNamespace().QualifiedName,
            size: dialogSize);
    }

    public SearchableTypeElementPicker(Lifetime lifetime, IDialogHost dialogHost)
        : this(lifetime, dialogHost, GetAllTypeElementsInSolution())
    {
    }

    /// <summary>
    /// Основной контрол для использования в UI.
    /// </summary>
    public BeControl Control => _searchableModalInputField.Control;

    /// <summary>
    /// Property со значением выбранного класса
    /// </summary>
    public IProperty<T> Value => _searchableModalInputField.Value;

    private static IEnumerable<T> GetAllTypeElementsInSolution()
    {
        var solution = Shell.Instance.GetComponent<SolutionsManager>().Solution;
        var items = PsiExtensions.GetAllTypeElements(solution);
        return items.OfType<T>();
    }
}
