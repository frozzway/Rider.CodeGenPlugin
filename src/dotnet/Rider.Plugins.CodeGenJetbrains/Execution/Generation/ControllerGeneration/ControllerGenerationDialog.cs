using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Application.DataContext;
using JetBrains.DataFlow;
using JetBrains.IDE.UI;
using JetBrains.IDE.UI.Extensions;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.Rider.Model.UIAutomation;
using Rider.Plugins.CodeGenJetbrains.Execution.Abstract;
using Rider.Plugins.CodeGenJetbrains.Execution.Common;
using Rider.Plugins.CodeGenJetbrains.UI.Components;
using Rider.Plugins.CodeGenJetbrains.Extensions;

namespace Rider.Plugins.CodeGenJetbrains.Execution.Generation.ControllerGeneration;

public class ControllerGenerationDialog(
    SolutionTypeElementsAccessor typeElementsAccessor) : IDialogForm<ControllerGenerationDto>
{
    private const string DefaultControllerName = "MyController";
    private const string DefaultPermissionEnum = "PermissionCode";
    private const string NotChosenPermissionValue = "-";

    private readonly HashSet<string> _methods =
    [
        ControllerMethods.AddAsync,
        ControllerMethods.UpdateAsync,
        ControllerMethods.GetAsync,
        ControllerMethods.GetGridAsync,
        ControllerMethods.GetExcelAsync,
        ControllerMethods.GetManyAsync
    ];

    private InputField<string> _nameInput = null!;
    private InputField<string> _entitySummaryInput = null!;
    private SearchableClassPicker _entityPicker = null!;
    private SearchableTypeElementPicker<IEnum> _permissionEnumPicker = null!;
    private DropdownListWithCheckbox<string> _methodList = null!;

    public BeDialog GetDialog(
        Lifetime lifetime,
        IDataContext context,
        string title)
    {
        CaretContextUtil.IsCaretInsideCSharpClassButNotMethod(context, out var declaration);
        return GetDialogImpl(lifetime, context, declaration, title);
    }

    private BeDialog GetDialogImpl(
        Lifetime lifetime,
        IDataContext context,
        IClassLikeDeclaration? classDeclaration,
        string title)
    {
        var dialogHost = context.GetComponent<IDialogHost>();
        var grid = BeControls.GetAutoGrid();

        _methodList = new DropdownListWithCheckbox<string>(
            lifetime,
            headers: ("Method", "Permission"),
            checkedByDefault: false,
            rows: _methods.Count,
            firstColumnItems: _methods.Select(m => m.GetBeLabel()),
            dropdownValues: _methods.Select(_ => NotChosenPermissionValue),
            dropdownInitialValues: _methods.Select(_ => NotChosenPermissionValue));

        _nameInput = InputFieldFactory.CreateTextBox("Controller name:", lifetime, placeholder: DefaultControllerName);
        _entityPicker = new SearchableClassPicker(lifetime, dialogHost, typeElementsAccessor.Classes);
        var entityTypeInput = new InputField(lifetime, "Entity type:", _entityPicker.Control);
        _entitySummaryInput = InputFieldFactory.CreateTextBox("Entity summary:", lifetime);

        _permissionEnumPicker = new SearchableTypeElementPicker<IEnum>(lifetime, dialogHost, typeElementsAccessor.Enums);
        var permissionEnumInput = new InputField(lifetime, "Permission enum:", _permissionEnumPicker.Control);

        _permissionEnumPicker.Value.Change.Advise_NewNotNull(lifetime, _ => RefreshPermissionList());
        _entityPicker.Value.Change.Advise_NewNotNull(lifetime,
            args =>
            {
                RefreshPermissionList();
                var summary = args.New.GetSummary();
                if (summary == null) return;
                _entitySummaryInput.CurrentValue.SetValue(summary);
            });
        _nameInput.CurrentValue.Change.Advise_NewNotNull(lifetime, args =>
            {
                var entity = args.New.FindEntityByControllerName(typeElementsAccessor.Classes);
                if (entity == null) return;
                _entityPicker.Value.SetValue(entity);
            });

        if (classDeclaration != null)
            _nameInput.CurrentValue.SetValue(classDeclaration.DeclaredName);

        var defaultPermissionEnum = typeElementsAccessor.Enums.FirstOrDefault(i => i.ShortName == DefaultPermissionEnum);
        if (defaultPermissionEnum != null)
            _permissionEnumPicker.Value.SetValue(defaultPermissionEnum);

        grid.AddElements(
            new InputFieldsForm(lifetime)
                .AddInputField(_nameInput)
                .AddInputField(entityTypeInput)
                .AddInputField(_entitySummaryInput)
                .AddInputField(permissionEnumInput)
                .Grid,
            _methodList.Control
            );

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

    private void RefreshPermissionList()
    {
        var @enum = _permissionEnumPicker.Value.Value;
        var enumValues = @enum?.EnumMembers.Select(member => member.ShortName).ToArray() ?? [];

        _methodList.SetItems(
            _methods.Count,
            firstColumnItems: _methods.Select(m => m.GetBeLabel()),
            dropdownValues: enumValues.Prepend(NotChosenPermissionValue),
            dropdownInitialValues: _methods.Select(m => NotChosenPermissionValue));
    }

    public ControllerGenerationDto GetDto()
    {
        var rows = _methodList.Rows.Where(r => r.IsChecked.Value).ToImmutableArray();

        return new ControllerGenerationDto
        {
            ControllerName = _nameInput.CurrentValue.Value.DefaultIfEmpty(DefaultControllerName),
            Methods = rows.Select(r => ((BeLabel)r.FirstColumn!).Text.Value).ToHashSet(),
            Entity = _entityPicker.Value.Value,
            EntitySummaryName = _entitySummaryInput.CurrentValue.Value,
            PermissionEnum = _permissionEnumPicker.Value.GetValue()?.ShortName,
            PermissionCodes = rows
                .Where(r => r.Dropdown.CurrentValue.Value != NotChosenPermissionValue)
                .ToDictionary(key => ((BeLabel)key.FirstColumn!).Text.Value, value => value.Dropdown.CurrentValue.Value)
        };
    }
}
