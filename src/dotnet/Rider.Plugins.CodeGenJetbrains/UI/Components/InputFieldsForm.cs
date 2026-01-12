using System;
using System.Collections.Generic;
using JetBrains.Collections.Viewable;
using JetBrains.IDE.UI.Extensions;
using JetBrains.Lifetimes;
using JetBrains.Rider.Model.UIAutomation;

namespace Rider.Plugins.CodeGenJetbrains.UI.Components;

/// <summary>
/// Форма с выровненными Input Field компонентами.
/// Использует BeSpanGrid для обеспечения единого выравнивания всех TextBox'ов.
/// </summary>
public class InputFieldsForm
{
    private readonly Dictionary<string, InputField> _inputFields = new();
    public BeSpanGrid Grid { get; }

    public IViewableProperty<bool> Enabled = new ViewableProperty<bool>(true);

    /// <param name="lifetime">Lifetime компонента</param>
    /// <param name="labelWidth">Ширина колонки с labels в пикселях (или auto, если 0)</param>
    public InputFieldsForm(Lifetime lifetime, int labelWidth = 0)
    {
        var columnSizes = labelWidth > 0 ? $"{labelWidth},*" : "auto,*";
        Grid = BeControls.GetSpanGrid(columnSizes);
        Enabled.Advise(lifetime, val =>
        {
            foreach (var field in _inputFields.Values)
                field.Enabled.Value = val;
        });
    }

    /// <summary>
    /// Добавляет новое поле ввода в форму.
    /// </summary>
    /// <param name="inputField">Элемент ввода с лейблом</param>
    /// <param name="id">Уникальный идентификатор InputField для последующего доступа</param>
    /// <returns>Ссылка на текущую форму для fluent API</returns>
    public InputFieldsForm AddInputField(InputField inputField, string? id = null)
    {
        id ??= Guid.NewGuid().ToString();
        _inputFields.Add(id, inputField);

        Grid.AddColumnElementsToNewRow(
            BeSizingType.Fit,   // Высота строки по содержимому
            false,              // Не разворачивать вложенные grid'ы
            (inputField.Label, BeSizingType.Fit),    // Label - фиксированная ширина
            (inputField.InputControl, BeSizingType.Fill)  // TextBox - заполняет оставшееся место
        );

        return this;
    }

    /// <summary>
    /// Получает InputField по его идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор InputField, указанный при добавлении</param>
    /// <returns>InputField или null, если InputField с таким ID не найден</returns>
    public InputField? GetInputField(string id)
    {
        return _inputFields.TryGetValue(id, out var textBox) ? textBox : null;
    }
}
