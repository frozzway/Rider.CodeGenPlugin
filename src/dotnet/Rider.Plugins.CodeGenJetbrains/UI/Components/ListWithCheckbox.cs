extern alias rt;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using JetBrains.Collections.Viewable;
using JetBrains.DataFlow;
using JetBrains.IDE.UI.Extensions;
using JetBrains.IDE.UI.Extensions.Properties;
using JetBrains.Lifetimes;
using JetBrains.Rider.Model.UIAutomation;
using Rider.Plugins.CodeGenJetbrains.Extensions;
using Generic = rt::System.Collections.Generic;

namespace Rider.Plugins.CodeGenJetbrains.UI.Components;

public class ListWithCheckbox<T> where T : notnull
{
    private readonly IViewableSet<T> _selectedItems = new ViewableSet<T>();

    private readonly IListEvents<T> _values;
    public ImmutableHashSet<T> SelectedItems => _selectedItems.ToImmutableHashSet();
    public ISource<SetEvent<T>> Change => _selectedItems.Change;
    public BeTreeGrid Grid { get; }

    public ListWithCheckbox(
        Lifetime lifetime,
        Generic.IReadOnlySet<T> values,
        string headerText,
        Func<T, Lifetime, BeControl> presentation,
        bool checkedByDefault = false,
        bool joinCheckBoxAndFirstColumn = false)
    {
        _values = values.ToListEvents(Guid.NewGuid().ToString());

        var config = joinCheckBoxAndFirstColumn
            ? new TreeConfiguration(columns:
            [
                (headerText, new BeUnitSize(BeSizingType.Fill)),
                (string.Empty, new BeUnitSize(BeSizingType.Fit)) // workaround to show column name
            ], hasHeader: true)
            : new TreeConfiguration(columns: [(headerText, new BeUnitSize(BeSizingType.Fill))]);

        typeof(TreeConfiguration)  // fix height of component
            .GetField("myVisibleRowCount", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(config, Math.Min(_values.Count, 7));

        Grid = _values.GetBeListWithCheckBoxes(lifetime, PresentationImpl, config, id: Guid.NewGuid().ToString(),
            joinCheckBoxAndFirstColumn);

        return;

        List<BeControl> PresentationImpl(Lifetime lt, T element, CheckBoxListNodeProperties properties)
        {
            properties.Included.Change.Advise_HasNew(lt, args =>
            {
                if (args.New is true)
                    _selectedItems.Add(element);
                else
                    _selectedItems.Remove(element);
            });

            if (checkedByDefault) properties.Included.SetValue(true);

            return [presentation(element, lt)];
        }
    }

    private void ReplaceItems(Generic.IReadOnlySet<T> newItems)
    {
        _selectedItems.Clear();
        _values.ReplaceItems(newItems);
    }
}
