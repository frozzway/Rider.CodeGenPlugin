using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.DataFlow;
using JetBrains.IDE.UI.Extensions;
using JetBrains.IDE.UI.Extensions.Properties;
using JetBrains.Lifetimes;
using JetBrains.Rider.Model.UIAutomation;

namespace Rider.Plugins.CodeGenJetbrains.UI.Components;

public class DropdownListWithCheckbox<T> where T : notnull
{
    public BeControl Control { get; }
    public ImmutableArray<T?> SelectedItems => [.._rows.Select(r => r.IsChecked.Value ? r.Dropdown.CurrentValue.Value : default)];
    public ImmutableArray<Row> Rows => [.._rows];

    private readonly IListEvents<Row> _rows;
    private readonly bool _checkedByDefault;
    private readonly Lifetime _lifetime;
    private LifetimeDefinition _dropdownLifetime;
    private readonly PresentComboItem<T, BeControl>? _dropdownPresentation;

    public record Row(BeControl? FirstColumn, InputField<T> Dropdown, T? DropdownInitialValue)
    {
        public Property<bool> IsChecked { get; set; }
    };

    public DropdownListWithCheckbox(
        Lifetime lifetime,
        (string? FirstColumnHeader, string? DropdownHeader)? headers,
        bool checkedByDefault = false,
        bool joinCheckBoxAndFirstColumn = false,
        PresentComboItem<T, BeControl>? dropdownPresentation = null)
        : this(lifetime, headers, 0, [], [], [], checkedByDefault, joinCheckBoxAndFirstColumn, dropdownPresentation) {}

    public DropdownListWithCheckbox(
        Lifetime lifetime,
        (string? FirstColumnHeader, string? DropdownHeader)? headers,
        int rows,
        IEnumerable<BeControl>? firstColumnItems,
        IEnumerable<T> dropdownValues,
        IEnumerable<T?> dropdownInitialValues,
        bool checkedByDefault = false,
        bool joinCheckBoxAndFirstColumn = false,
        PresentComboItem<T, BeControl>? dropdownPresentation = null)
    {
        _lifetime = lifetime;
        _checkedByDefault = checkedByDefault;
        _dropdownLifetime = Lifetime.Define(lifetime);
        _dropdownPresentation = dropdownPresentation;

        var dropdownValuesArr = dropdownValues.ToImmutableArray();
        var firstColumnItemsArr = firstColumnItems?.ToImmutableList() ?? [];

        var config =
            firstColumnItems is null
                ? joinCheckBoxAndFirstColumn
                    ? new TreeConfiguration(columns:
                    [
                        (headers.GetValueOrDefault().DropdownHeader ?? "", new BeUnitSize(BeSizingType.Fill)),
                        (string.Empty, new BeUnitSize(BeSizingType.Fit)) // workaround to show column name
                    ], hasHeader: headers.HasValue)
                    : new TreeConfiguration(columns:
                        [(headers.GetValueOrDefault().DropdownHeader ?? "", new BeUnitSize(BeSizingType.Fill))])
                : new TreeConfiguration(columns:
                [
                    (headers.GetValueOrDefault().FirstColumnHeader ?? "", new BeUnitSize(BeSizingType.Fill)),
                    (headers.GetValueOrDefault().DropdownHeader ?? "", new BeUnitSize(BeSizingType.Fill))
                ], hasHeader: headers.HasValue);

        _rows = Enumerable.Range(0, rows)
            .Select(index => CreateRow(index, dropdownInitialValues, firstColumnItemsArr, dropdownValuesArr))
            .ToListEvents(Guid.NewGuid().ToString());

        Control = _rows.GetBeListWithCheckBoxes(lifetime, PresentationImpl, config, id: Guid.NewGuid().ToString(),
            joinCheckBoxAndFirstColumn).WithMinSize(GetSize(rows), lifetime);
    }

    private Row CreateRow(
        int index,
        IEnumerable<T?> dropdownInitialValues,
        ImmutableList<BeControl> firstColumnItemsArr,
        ImmutableArray<T> dropdownValuesArr)
    {
        var firstColumn = firstColumnItemsArr.ElementAtOrDefault(index);
        var initialValue = dropdownInitialValues.ElementAtOrDefault(index);
        var dropdown = InputFieldFactory.CreateDropdown(
            labelText: "", dropdownValuesArr, _dropdownLifetime.Lifetime, initialValue, _dropdownPresentation);
        return new Row(firstColumn, dropdown, initialValue);
    }

    public void SetItems(
        int rows,
        IEnumerable<BeControl>? firstColumnItems,
        IEnumerable<T> dropdownValues,
        IEnumerable<T?> dropdownInitialValues
    )
    {
        var dropdownValuesArr = dropdownValues.ToImmutableArray();
        var firstColumnItemsArr = firstColumnItems?.ToImmutableList() ?? [];

        _rows.Clear();
        _dropdownLifetime.Terminate();
        _dropdownLifetime = Lifetime.Define(_lifetime);

        var newRows = Enumerable.Range(0, rows)
            .Select(index => CreateRow(index, dropdownInitialValues, firstColumnItemsArr, dropdownValuesArr)).ToList();

        _rows.AddRange(newRows);

        ((BeStyleControl)Control).Sizes.Clear();
        ((BeStyleControl)Control).Sizes.Add(new BeSizeModifier(BeBoundsType.MIN_SIZE, GetSize(rows)));
    }

    private static BeControlSize GetSize(int rowCount)
    {
        var height = rowCount <= 7 ? BeControlSizeType.SMALL : BeControlSizeType.MEDIUM;
        return new BeControlSizePredefined(
            width: new BeSize(BeControlSizeType.FIT_TO_CONTENT),
            height: new BeSize(height));
    }

    private List<BeControl> PresentationImpl(Lifetime lt, Row row, CheckBoxListNodeProperties properties)
    {
        row.IsChecked = properties.Included;
        row.Dropdown.CurrentValue.Change.Advise_HasNew(lt, args =>
        {
            if (!args.HasOld) return;
            properties.Included.SetValue(true);
        });

        if (_checkedByDefault && row.DropdownInitialValue is not null)
        {
            properties.Included.SetValue(true);
        };

        if (row.FirstColumn is not null)
            return [row.FirstColumn, row.Dropdown.InputControl];

        return [row.Dropdown.InputControl];
    }
}
