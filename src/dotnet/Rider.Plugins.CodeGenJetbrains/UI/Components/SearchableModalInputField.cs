using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.DataFlow;
using JetBrains.IDE.UI;
using JetBrains.IDE.UI.Extensions;
using JetBrains.Lifetimes;
using JetBrains.Rider.Model.UIAutomation;
using JetBrains.Util;
using Rider.Plugins.CodeGenJetbrains.Extensions;

namespace Rider.Plugins.CodeGenJetbrains.UI.Components;

public class SearchableModalInputField<T>
{
    private ModalInputField<T> InputField { get; set; }

    /// <summary>
    /// Property со значением выбранного элемента типа T.
    /// </summary>
    public IProperty<T> Value => InputField.Value;

    /// <summary>
    /// Основной контрол для использования в UI.
    /// </summary>
    public BeControl Control => InputField.Control;

    private SearchableModalInputField(ModalInputField<T> inputField)
    {
        InputField = inputField;
    }

    /// <summary>
    /// Создаёт новый SearchableModalInputField с generic типом значения.
    /// </summary>
    /// <param name="lifetime">Lifetime компонента</param>
    /// <param name="dialogHost">Хост для отображения диалогов</param>
    /// <param name="items">Элементы в коллекции</param>
    /// <param name="presentValue">Делегат-презентация выбранного значения для компонента</param>
    /// <param name="dialogPresentValue">Делегат-презентация для строки в модальном окне выбора</param>
    /// <param name="dialogHintPresentValue">Делегат-презентация для строки (hint) в модальном окне выбора</param>
    /// <param name="size">Размер диалога</param>
    /// <param name="initialValue">Начальное значение типа T</param>
    /// <param name="placeholder">Placeholder текст, отображаемый когда значение null или default</param>
    /// <param name="id">ID для контрола</param>
    /// <returns>Новый экземпляр ModalInputField&lt;T&gt;</returns>
    public static SearchableModalInputField<T> Create(
        Lifetime lifetime,
        IDialogHost dialogHost,
        IEnumerable<T> items,
        Func<T, string> presentValue,
        Func<T, string>? dialogPresentValue = null,
        Func<T, string>? dialogHintPresentValue = null,
        BeControlSize? size = null,
        T initialValue = default!,
        string placeholder = "",
        string id = "")
    {
        // Создаём Modal Input Field
        var modalField = ModalInputField<T>.Create(
            lifetime,
            dialogHost,
            createDialog: selectValue
                => CreateUserSelectionDialog(lifetime, items, selectValue, dialogPresentValue ?? presentValue,
                    dialogHintPresentValue, size),
            presentValue: presentValue,
            initialValue: initialValue,
            placeholder: placeholder,
            id: id
        );

        return new SearchableModalInputField<T>(modalField);
    }

    private static BeDialog CreateUserSelectionDialog(
        Lifetime lifetime,
        IEnumerable<T> items,
        Action<T> selectValue,
        Func<T, string> presentValue,
        Func<T, string>? hintPresentValue = null,
        BeControlSize? dialogSize = null)
    {
        const int displayLimit = 100;
        var grid = BeControls.GetEmptyGrid();
        T? selectedItem = default;

        var allItems = items.ToArray();
        var eventItems = allItems.Take(displayLimit).ToListEvents("GetBeListWrapper");

        var searchBox = new BeSearchBox();
        searchBox.Text.Change.Advise(lifetime, filterString =>
        {
            eventItems.Clear();
            var newItems = allItems
                .Where(i => SearchFilter(presentValue(i), filterString))
                .OrderBy(i => presentValue(i).Length)
                .Take(displayLimit);
            eventItems.AddRange(newItems);
        });

        var list = PluginBeControls.GetTypeList(lifetime, eventItems, presentValue, hintPresentValue,
            onChosen: newValue => selectedItem = newValue);

        BeControl selectableList = dialogSize is null
            ? list
            : BeControls.GetScrollablePanel(
                content: list,
                size: dialogSize,
                scrollbarPolicy: BeScrollbarPolicy.BOTH
            );

        grid.AddElements(searchBox, selectableList);

        return grid.InDialog(
            title: "Select value",
            id: Guid.NewGuid().ToString(),
            size: dialogSize,
            modality: DialogModality.MODAL)
            .WithOkButton(lifetime, () =>
            {
                if (selectedItem != null)
                    selectValue(selectedItem);
            })
            .WithCancelButton(lifetime);
    }

    private static bool SearchFilter(string presentation, string filterString)
    {
        var filterStringParts = filterString.SplitByCapitals();
        var presentationParts = presentation.SplitByCapitals().ToList();

        return filterStringParts.All(filterPart =>
        {
            var containingPart = presentationParts.FirstOrDefault(p => p.Contains(filterPart) || p.Contains(filterPart.Capitalize()));
            if (containingPart is not null)
                presentationParts.Remove(containingPart);
            return containingPart is not null;
        });
    }
}
