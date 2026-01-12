using System;
using System.Collections.Generic;
using JetBrains.DataFlow;
using JetBrains.IDE.UI;
using JetBrains.IDE.UI.Extensions;
using JetBrains.Lifetimes;
using JetBrains.Rider.Model.UIAutomation;
using JetBrains.UI.RichText;

namespace Rider.Plugins.CodeGenJetbrains.UI.Components;

/// <summary>
/// Generic Modal Input Field для выбора значений типа T через диалог.
/// Весь контрол кликабелен, отображает текстовое представление выбранного значения.
/// </summary>
/// <typeparam name="T">Тип выбираемого значения</typeparam>
public class ModalInputField<T>
{
    private readonly BeButton _clickableField;

    /// <summary>
    /// Property со значением выбранного элемента типа T.
    /// </summary>
    public IProperty<T> Value { get; }

    /// <summary>
    /// Основной контрол для использования в UI.
    /// </summary>
    public BeControl Control => _clickableField;

    private ModalInputField(BeButton clickableField, IProperty<T> value)
    {
        _clickableField = clickableField;
        Value = value;
    }

    /// <summary>
    /// Создаёт новый ModalInputField с generic типом значения.
    /// </summary>
    /// <param name="lifetime">Lifetime компонента</param>
    /// <param name="dialogHost">Хост для отображения диалогов</param>
    /// <param name="createDialog">
    /// Функция создания диалога. Принимает callback для установки значения типа T.
    /// Пример: selectValue => MyDialog.Create(selectValue)
    /// </param>
    /// <param name="presentValue">
    /// Делегат-презентация: преобразует значение типа T в строку для отображения.
    /// Пример: user => user.Name
    /// </param>
    /// <param name="initialValue">Начальное значение типа T</param>
    /// <param name="placeholder">Placeholder текст, отображаемый когда значение null или default</param>
    /// <param name="id">ID для контрола</param>
    /// <returns>Новый экземпляр ModalInputField&lt;T&gt;</returns>
    public static ModalInputField<T> Create(
        Lifetime lifetime,
        IDialogHost dialogHost,
        Func<Action<T>, BeDialog> createDialog,
        Func<T, string> presentValue,
        T initialValue = default!,
        string placeholder = "Click to select...",
        string id = "")
    {
        var valueProperty = new Property<T>("ModalInputField.Value", initialValue);

        // Создаём dynamic label с презентацией значения через делегат
        var displayTextProperty = valueProperty.Select("DisplayText", value =>
        {
            // Если значение null или default, показываем placeholder
            if (EqualityComparer<T>.Default.Equals(value, default(T)))
                return placeholder;

            // Иначе используем делегат презентации
            return presentValue(value);
        });

        var label = BeControls.GetRichText(placeholder, JetFontStyles.Regular, Colors.Grey);

        valueProperty.Change.Advise(lifetime, val =>
        {
            if (!val.HasNew || !val.HasOld || EqualityComparer<T>.Default.Equals(val.New, val.Old))
                return;

            label.Text.Value = new RichText(
                displayTextProperty.Value,
                new TextStyle(JetFontStyles.Regular)
            ).ToModelRichText();
        });

        // Создаём кликабельную кнопку
        var clickableField = BeControls.GetButton(
            label,
            lifetime,
            onClick: () => OpenDialog(dialogHost, createDialog, valueProperty, lifetime),
            BeButtonStyle.DEFAULT,
            id: id
        );

        return new ModalInputField<T>(clickableField, valueProperty);
    }

    private static void OpenDialog(
        IDialogHost dialogHost,
        Func<Action<T>, BeDialog> createDialog,
        IProperty<T> valueProperty,
        Lifetime parentLifetime)
    {
        dialogHost.Show(
            dialogLifetime => createDialog(newValue => valueProperty.Value = newValue),
            parentLifetime
        );
    }
}
