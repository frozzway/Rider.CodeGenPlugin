using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.DataContext;
using JetBrains.Application.Threading;
using JetBrains.Collections.Viewable;
using JetBrains.DataFlow;
using JetBrains.IDE.UI;
using JetBrains.IDE.UI.Extensions;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ReSharper.Psi;
using JetBrains.Rider.Model.UIAutomation;
using Rider.Plugins.CodeGenJetbrains.Execution.Abstract;
using Rider.Plugins.CodeGenJetbrains.Execution.Common;
using Rider.Plugins.CodeGenJetbrains.Extensions;
using Rider.Plugins.CodeGenJetbrains.Model;
using Rider.Plugins.CodeGenJetbrains.UI.Components;
using Rider.Plugins.CodeGenJetbrains.UI;

namespace Rider.Plugins.CodeGenJetbrains.Execution.RepositoryGeneration;

public class RepositoryGenerationDialogForm(SolutionTypeElementsAccessor classesAccessor) : IDialogForm<RepositoryGenerationDto>
{
    private readonly HashSet<string> _methods =
    [
        RepositoryMethods.AddOrUpdateAsync,
        RepositoryMethods.AddOrUpdateRangeAsync,
        RepositoryMethods.GetListAsync,
        RepositoryMethods.GetManyAsync
    ];

    private DataSourceInput _dataSourceInput;
    private ISolution _solution;
    private SearchableClassPicker _entityPicker;
    private SearchableClassPicker _entityListPicker;
    private DropdownListWithCheckbox<DbColumnDto> _columnList;
    private ListWithCheckbox<string> _methodList;
    private string[] _entityProperties;

    public BeDialog GetDialog(
        Lifetime lifetime,
        IDataContext context,
        string title)
    {
        var solution = context.GetData(ProjectModelDataConstants.SOLUTION);
        if (solution == null)
            throw new InvalidOperationException();

        return GetDialogImpl(lifetime, context, solution, title);
    }

    private BeDialog GetDialogImpl(
        Lifetime lifetime,
        IDataContext context,
        ISolution solution,
        string title)
    {
        _solution = solution;
        var rdDatabaseCaller = _solution.GetComponent<RdDatabaseCaller>();
        var dialogHost = context.GetComponent<IDialogHost>();

        _dataSourceInput = new DataSourceInput(lifetime, rdDatabaseCaller);

        var form = new InputFieldsForm(lifetime)
            .AddInputField(_dataSourceInput.SourceInput, "sourceInput")
            .AddInputField(_dataSourceInput.DatabaseInput, "databaseInput")
            .AddInputField(_dataSourceInput.SchemaInput, "schemaInput")
            .AddInputField(_dataSourceInput.TableInput, "tableInput");

        var grid = BeControls.GetAutoGrid();

        grid.AddElement(form.Grid.WithTitledBorder("Data Source", bottomMargin: 10));

        _methodList = new ListWithCheckbox<string>(
            lifetime: lifetime,
            values: _methods,
            headerText: "Methods",
            presentation: (value, _) => value.GetBeLabel(),
            checkedByDefault: true,
            joinCheckBoxAndFirstColumn: true);

        grid.AddElement(_methodList.Grid);

        _entityPicker = new SearchableClassPicker(lifetime, dialogHost, classesAccessor.Classes);
        _entityListPicker = new SearchableClassPicker(lifetime, dialogHost, classesAccessor.Classes);
        var entityTypeInput = new InputField(lifetime, "Entity type:", _entityPicker.Control);
        var listTypeInput = new InputField(lifetime, "List type:", _entityListPicker.Control);

        _methodList.Change.Advise(lifetime, set =>
        {
            if (set is { Value: RepositoryMethods.GetListAsync, Kind: AddRemove.Add })
                listTypeInput.Enabled.Value = true;
            if (set is { Value: RepositoryMethods.GetListAsync, Kind: AddRemove.Remove })
                listTypeInput.Enabled.Value = false;
        });

        grid.AddElement(
            new InputFieldsForm(lifetime)
                .AddInputField(entityTypeInput)
                .AddInputField(listTypeInput)
                .Grid
        );

        _columnList = new DropdownListWithCheckbox<DbColumnDto>(lifetime,
            headers: ("Property", "Column"),
            checkedByDefault: true,
            dropdownPresentation: (_, element, _) => element.Name.GetBeLabel());

        _entityPicker.Value.Change.Advise_NewNotNull(lifetime, _ =>
        {
            if (_dataSourceInput.TableInput.CurrentValue.Value != null)
                RefreshColumnList(rdDatabaseCaller);
        });

        _dataSourceInput.TableInput.CurrentValue.Change.Advise_HasNew(lifetime, value =>
        {
            if (value.New is null)
            {
                _columnList.SetItems(0, [], [], []);
                return;
            }

            if (_entityPicker.Value.Value != null)
                RefreshColumnList(rdDatabaseCaller);
        });

        grid.AddElement(_columnList.Control);

        var wrapper = BeControls.GetScrollablePanel(
            content: grid,
            size: new BeControlSizePredefined(
                width: new BeSize(BeControlSizeType.SMALL, 2),
                height: new BeSize(BeControlSizeType.FIT_TO_CONTENT)));

        return wrapper.InDialog(
            title: title,
            id: Guid.NewGuid().ToString(),
            isResizable: true);
    }

    private void RefreshColumnList(RdDatabaseCaller caller)
    {
        _solution.Locks.ExecuteWithReadLock(() =>
        {
            var entity = _entityPicker.Value.Value;
            _entityProperties = entity.GetSuperTypes()
                .Select(type => type.GetTypeElement())
                .OfType<IClass>()
                .SelectMany(elem => elem.Properties).Concat(entity.Properties)
                .Where(prop => prop.GetAccessRights() == AccessRights.PUBLIC)
                .Where(prop => !prop.IsStatic)
                .Select(i => i.ShortName)
                .ToArray();
        });

        var columns = caller.GetColumns(_dataSourceInput.TableInput.CurrentValue.Value.Id).ToArray();
        _columnList.SetItems(
            rows: _entityProperties.Length,
            firstColumnItems: _entityProperties.Select(prop => prop.GetBeLabel()),
            dropdownValues: columns,
            dropdownInitialValues: _entityProperties.Select(prop =>
                columns.FirstOrDefault(col => col.Name == prop.ToSnakeCaseRegex()
                                              || string.Equals(col.Name, prop, StringComparison.OrdinalIgnoreCase)))
        );
    }

    public RepositoryGenerationDto GetDto()
    {
        var selectedItems = _columnList.SelectedItems;

        return new()
        {
            Methods = _methodList.SelectedItems,
            EntityType = _entityPicker.Value.Value,
            EntityListType = _entityListPicker.Value.Value,
            SchemaName = _dataSourceInput.SchemaInput.CurrentValue.Value.Name,
            TableName = _dataSourceInput.TableInput.CurrentValue.Value.Name,
            Properties = [
                ..Enumerable.Range(0, _entityProperties.Length)
                    .Where(i => selectedItems[i] is not null)
                    .Select(i =>
                    {
                        var name = _entityProperties[i];
                        var column = selectedItems[i]!;
                        return new DbProperty(name, column.Name, column.IsPrimaryKey);
                    })
            ]
        };
    }
}
