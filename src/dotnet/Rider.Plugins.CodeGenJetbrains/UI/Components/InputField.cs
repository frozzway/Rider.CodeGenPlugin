using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Collections.Viewable;
using JetBrains.DataFlow;
using JetBrains.IDE.UI.Extensions;
using JetBrains.Lifetimes;
using JetBrains.Platform.RdFramework.Util;
using JetBrains.Rider.Model.UIAutomation;

namespace Rider.Plugins.CodeGenJetbrains.UI.Components;

/// <summary>
/// Тип, объединяющий <see cref="BeControl"/> с <see cref="BeLabel"/>
/// </summary>
public class InputField
{
    public BeSpanGrid SpanGrid { get; }
    public BeControl InputControl { get; }
    public BeLabel Label { get; }
    public BeControl Control => SpanGrid;
    public IViewableProperty<bool> Enabled = new ViewableProperty<bool>(true);

    public InputField(Lifetime lifetime, string labelText, BeControl inputControl)
    {
        SpanGrid = BeControls.GetSpanGrid("auto,*");
        Label = labelText.GetBeLabel();
        InputControl = inputControl;

        SpanGrid.AddColumnElementsToNewRow(BeSizingType.Fit, false,
            (Label, BeSizingType.Fit),
            (InputControl, BeSizingType.Fill));

        Enabled.Advise(lifetime, val =>
        {
            InputControl.Enabled.Value = val;
            Label.Enabled.Value = val;
        });
    }
}

/// <summary>
/// Тип, объединяющий <see cref="BeControl"/> с <see cref="BeLabel"/>,
/// предоставляющий актуальное выбранное значение через свойство <see cref="CurrentValue"/>
/// </summary>
public class InputField<T>(
    Lifetime lifetime,
    string labelText,
    BeControl inputControl,
    IProperty<T> currentValue,
    IListEvents<T>? values = null)
    : InputField(lifetime, labelText, inputControl) where T : notnull
{
    public IProperty<T> CurrentValue { get;} = currentValue;
    public IListEvents<T>? Values { get; } = values; // for drop-downs
}

public static class InputFieldFactory
{
    ///  <summary>
    /// Создает <see cref="InputField{T}"/> с InputControl типа <see cref="BeTextBox"/>
    /// </summary>
    /// <param name="labelText">Текст label'а</param>
    /// <param name="lifetime">Lifetime для управления жизненным циклом компонента</param>
    /// <param name="placeholder">Placeholder текст для TextBox (опционально)</param>
    /// <param name="configure">Конфигурация созданного TextBox (опционально)</param>
    public static InputField<string> CreateTextBox(
        string labelText,
        Lifetime lifetime,
        string? placeholder = null,
        Action<BeTextBox>? configure = null)
    {
        var textBox = BeControls.GetTextBox(
            lifetime,
            id: Guid.NewGuid().ToString(),
            placeholder: placeholder ?? string.Empty);

        configure?.Invoke(textBox);

        var mirrorProp = new Property<string>(Guid.NewGuid().ToString(), textBox.Text.Value);
        textBox.Text.FlowInto(lifetime, mirrorProp);
        mirrorProp.FlowInto(lifetime, textBox.Text);

        var inputField = new InputField<string>(lifetime, labelText, textBox, currentValue: mirrorProp);

        return inputField;
    }

    public static InputField<T> CreateDropdown<T>(
        string labelText,
        IEnumerable<T> items,
        Lifetime lifetime,
        T? initialValue = default,
        PresentComboItem<T, BeControl>? presentation = null) where T : notnull
    {
        var eventList = items.ToListEvents(Guid.NewGuid().ToString());
        return CreateDropdown(labelText, eventList, lifetime, initialValue, presentation);
    }

    public static InputField<T> CreateDropdown<T>(
        string labelText,
        IListEvents<T> items,
        Lifetime lifetime,
        T? initialValue = default,
        PresentComboItem<T, BeControl>? presentation = null) where T : notnull
    {
        var property = new Property<T>(Guid.NewGuid().ToString(), initialValue ?? items.FirstOrDefault());
        var cb = property.GetBeComboBox(lifetime, items, presentation!);
        var inputField = new InputField<T>(lifetime, labelText, cb, property, items);
        return inputField;
    }
}
