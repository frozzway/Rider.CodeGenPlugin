using System;
using System.Collections.Generic;
using JetBrains.DataFlow;
using JetBrains.IDE.UI.Extensions;
using JetBrains.IDE.UI.Extensions.Properties;
using JetBrains.Lifetimes;
using JetBrains.Rider.Model.UIAutomation;
using JetBrains.Util.Media;

namespace Rider.Plugins.CodeGenJetbrains.UI;

/// <summary>
/// Набор полезных методов для работы с UI-компонентной базой
/// </summary>
public static class PluginBeControls
{
    public static IListEvents<T> ToListEvents<T>(this IEnumerable<T> items, string listEventsId)
    {
        var listEvents = new ListEvents<T>(listEventsId, false);
        listEvents.AddRange(items);
        return listEvents;
    }

    public static BeControl WithTitledBorder(this BeControl control, string title, int topMargin = 10, int bottomMargin = 0)
        => control.WithTitledBorder(title, 15, GetGroupMargin(topMargin, bottomMargin));

    public static BeMargin GetGroupMargin(int top = 10, int bottom = 0)
        => BeMargins.Create(BeMarginType.OnePx, 0, top, 0, bottom);

    public static BeMargin GetMargin(int left = 0, int top = 0, int right = 0, int bottom = 0)
        => BeMargins.Create(BeMarginType.OnePx, left, top, right, bottom);

    public static BeTreeGrid GetTypeList<T>(
        Lifetime lifetime,
        IListEvents<T> items,
        Func<T, string> presentValue,
        Func<T, string>? hintPresentValue = null,
        Action<T>? onChosen = null)
    {
        var config = new TreeConfiguration(
            selection: BeTreeSelection.Single,
            childOffsetSize: ChildOffsetSize.NONE,   // чтобы убрать “лесенку”
            autoScrollToEnd: false,
            showBorders: BeShowBorders.All,
            autoStartEditing: false,
            hasHeader: false,
            backendContextMenu: null);

        PresentListLine<T> presentLine = (lt, chosenValue, listProps) =>
        {
            listProps.Selected.Change.Advise(lt, e =>
            {
                if (e.Property.Value)
                    onChosen?.Invoke(chosenValue);
            });

            var label = BeControls.GetLabel();
            label.Text.Value = presentValue(chosenValue);

            if (hintPresentValue is null)
                return [label];

            var hintLabel = BeControls.GetRichText(
                hintPresentValue(chosenValue),
                fgColor: new JetRgbaColor(128, 128, 128, 255)
            );

            var horizontalGrid = BeControls.GetEmptyGrid(GridOrientation.Horizontal);
            horizontalGrid
                .AddElement(label)
                .AddElement(hintLabel);

            return [horizontalGrid];
        };

        var grid = items.GetBeList(
            lifetime: lifetime,
            presentLine: presentLine,
            configuration: config);

        return grid;
    }
}
