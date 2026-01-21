using System;
using JetBrains.Application.DataContext;
using JetBrains.Collections.Viewable;
using JetBrains.IDE.UI.Extensions;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.Rider.Model.UIAutomation;
using Rider.Plugins.CodeGenJetbrains.Execution.Abstract;
using Rider.Plugins.CodeGenJetbrains.Extensions;
using Rider.Plugins.CodeGenJetbrains.UI.Components;

namespace Rider.Plugins.CodeGenJetbrains.Execution.Generation.CommandGeneration;

public class CommandGenerationDialog : IDialogForm<CommandGenerationDto>
{
    private const string DefaultName = "CreateEntityCommand";

    private InputField<string> _commandNameInput;
    private BeCheckbox _createResultCheckbox;
    private InputField<string> _returnTypeInput;

    public BeDialog GetDialog(
        Lifetime lifetime,
        IDataContext context,
        string title)
    {
        var solution = context.GetData(ProjectModelDataConstants.SOLUTION);
        if (solution == null)
            throw new InvalidOperationException();

        return GetDialogImpl(lifetime, solution, title);
    }

    private BeDialog GetDialogImpl(
        Lifetime lifetime,
        ISolution solution,
        string title)
    {
        var grid = BeControls.GetAutoGrid();

        _commandNameInput = InputFieldFactory.CreateTextBox("Name", lifetime, DefaultName);
        _createResultCheckbox = BeControls.GetCheckBox("Create result type", Guid.NewGuid().ToString(),
            lifetime, initialValue: false);
        _returnTypeInput = InputFieldFactory.CreateTextBox("Return type", lifetime,
            configure: box => box.WithTypeCompletionShort(solution, lifetime, CSharpLanguage.Instance!, allTypes: true));

        _createResultCheckbox.Property.AdviseNotNull(lifetime, newValue => _returnTypeInput.Enabled.Value = !newValue);

        var form = new InputFieldsForm(lifetime)
            .AddInputField(_commandNameInput)
            .AddInputField(_returnTypeInput);

        grid.AddElements(form.Grid, _createResultCheckbox);

        return grid.InDialog(
            title: title,
            id: Guid.NewGuid().ToString(),
            isResizable: true);
    }

    public CommandGenerationDto GetDto()
    {
        return new CommandGenerationDto
        {
            Name = ((BeTextBox)_commandNameInput.InputControl).TryGetText().DefaultIfEmpty(DefaultName),
            ReturnType = _createResultCheckbox.Property.Value is true
                ? null
                : _returnTypeInput.CurrentValue.Value
        };
    }
}
