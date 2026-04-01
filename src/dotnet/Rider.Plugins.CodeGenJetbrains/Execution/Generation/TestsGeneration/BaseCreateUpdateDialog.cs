using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Humanizer;
using JetBrains.Application.DataContext;
using JetBrains.Application.Threading;
using JetBrains.Collections.Viewable;
using JetBrains.IDE.UI;
using JetBrains.IDE.UI.Extensions;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ReSharper.Feature.Services.Web.AspRouteTemplates.EndpointsProvider.AspNetHttpEndpoints;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.Rider.Model.UIAutomation;
using Rider.Plugins.CodeGenJetbrains.Execution.Abstract;
using Rider.Plugins.CodeGenJetbrains.Execution.Common;
using Rider.Plugins.CodeGenJetbrains.Extensions;
using Rider.Plugins.CodeGenJetbrains.UI;
using Rider.Plugins.CodeGenJetbrains.UI.Components;

namespace Rider.Plugins.CodeGenJetbrains.Execution.Generation.TestsGeneration;

public abstract class BaseCreateUpdateDialog<T>(SolutionTypeElementsAccessor solutionTypeElementsAccessor) : IDialogForm<T>
{
    protected abstract string DefaultCaseName { get; }
    protected abstract string Verb { get; }
    protected abstract string ActHttpVerb { get; }
    protected abstract bool EntityInputEnabled { get; }

    protected string DefaultFilePrefix = null!;
    protected const string DefaultEndpoint = "/api/endpoint/";

    protected InputFieldsForm MainForm = null!;
    protected InputFieldsForm AssertForm = null!;
    protected readonly Dictionary<string, SearchableClassPicker> SearchableClassPickers = new();

    public BeDialog GetDialog(
        Lifetime lifetime,
        IDataContext context,
        string title)
    {
        var element = context.GetData(ProjectModelDataConstants.PROJECT_MODEL_ELEMENT);

        if (element is not IProjectFolder folder)
            throw new InvalidOperationException();

        return GetDialogImpl(lifetime, context, folder, title);
    }

    public abstract T GetDto();

    private BeDialog GetDialogImpl(
        Lifetime lifetime,
        IDataContext context,
        IProjectFolder folder,
        string title)
    {
        var dialogHost = context.GetComponent<IDialogHost>();
        var folderName = folder.Name;
        var solution = folder.GetSolution();
        DefaultFilePrefix = $"{Verb}{folderName.Singularize()}";

        var requestActTypePicker = new SearchableClassPicker(lifetime, dialogHost, solutionTypeElementsAccessor.Classes);
        var responseActTypePicker = new SearchableClassPicker(lifetime, dialogHost, solutionTypeElementsAccessor.Classes);
        var responseAssertTypePicker = new SearchableClassPicker(lifetime, dialogHost, solutionTypeElementsAccessor.Classes);
        SearchableClassPickers.Add(ComponentsIdentity.RequestActType, requestActTypePicker);
        SearchableClassPickers.Add(ComponentsIdentity.ResponseActType, responseActTypePicker);
        SearchableClassPickers.Add(ComponentsIdentity.ResponseAssertType, responseAssertTypePicker);

        var grid = BeControls.GetAutoGrid();

        var filePrefixInput = InputFieldFactory.CreateTextBox("File names prefix:", lifetime, placeholder: DefaultFilePrefix);
        var testCaseInput = InputFieldFactory.CreateTextBox("Test case name:", lifetime, placeholder: DefaultCaseName);
        var actEndpointInput = InputFieldFactory.CreateTextBox("Act endpoint:", lifetime, placeholder: DefaultEndpoint,
            configure: box => box.WithEndpointsCompletion(solution, lifetime, FilterByHttpVerb(ActHttpVerb)));

        var requestActTypeInput = new InputField(lifetime, "Request type:", requestActTypePicker.Control);

        MainForm = new InputFieldsForm(lifetime)
            .AddInputField(filePrefixInput, ComponentsIdentity.FilesPrefix)
            .AddInputField(testCaseInput, ComponentsIdentity.TestCaseName)
            .AddInputField(actEndpointInput, ComponentsIdentity.ActEndpoint)
            .AddInputField(requestActTypeInput, ComponentsIdentity.RequestActType);

        if (EntityInputEnabled)
        {
            var entityTypePicker = new SearchableClassPicker(lifetime, dialogHost, solutionTypeElementsAccessor.Classes);
            SearchableClassPickers.Add(ComponentsIdentity.EntityInput, entityTypePicker);
            var entityInput = new InputField(lifetime, "Entity", entityTypePicker.Control);
            MainForm.AddInputField(entityInput, ComponentsIdentity.EntityInput);
        }

        grid.AddElement(MainForm.Grid.WithTitledBorder("Parameters"));

        var assertEndpointInput = InputFieldFactory.CreateTextBox("Assert endpoint:", lifetime, placeholder: DefaultEndpoint,
            configure: box => box.WithEndpointsCompletion(solution, lifetime, FilterByHttpVerb("GET")));

        var responseActTypeInput = new InputField(lifetime, "Response act type:", responseActTypePicker.Control);
        var responseAssertTypeInput = new InputField(lifetime, "Response assert type:", responseAssertTypePicker.Control);

        AssertForm = InitializeAssertForm(lifetime, assertEndpointInput, responseActTypeInput, responseAssertTypeInput);
        AssertForm.Enabled.Value = false;

        var actEndpointTextBox = (BeTextBox)actEndpointInput.InputControl;
        var assertEndpointTextBox = (BeTextBox)assertEndpointInput.InputControl;
        AdviseToSetTypeFromEndpoint(actEndpointTextBox, solution, responseActTypePicker, ActHttpVerb, ResolverFromReturnValue, lifetime);
        AdviseToSetTypeFromEndpoint(actEndpointTextBox, solution, requestActTypePicker, ActHttpVerb, ResolverFromParameter, lifetime);
        AdviseToSetTypeFromEndpoint(assertEndpointTextBox, solution, responseAssertTypePicker, "GET", ResolverFromReturnValue, lifetime);
        AdviseToChangeAssertRoute(actEndpointTextBox, assertEndpointTextBox, solution, lifetime);

        if (EntityInputEnabled)
            AdviseToSetEntity(actEndpointTextBox, solution, lifetime);

        // Assert stage group
        var assertCheckbox = BeControls.GetCheckBox("Enable", Guid.NewGuid().ToString(), lifetime)!;
        assertCheckbox.Property.AdviseNotNull(lifetime, newValue => AssertForm.Enabled.Value = newValue);

        var assertGrid = BeControls.GetAutoGrid();
        assertGrid.AddElements(assertCheckbox.WithMargin(PluginBeControls.GetMargin(top: 5)), AssertForm.Grid);

        grid.AddElement(assertGrid.WithTitledBorder("Assert stage", bottomMargin: 10));

        return grid.InDialog(
            title: title,
            id: Guid.NewGuid().ToString(),
            isResizable: true,
            size: new BeControlSizeFixed(
                width: BeControlSizeType.HUGE,
                height: BeControlSizeType.FIT_TO_CONTENT));
    }

    protected virtual InputFieldsForm InitializeAssertForm(
        Lifetime lifetime,
        InputField<string> assertEndpointInput,
        InputField responseActTypeInput,
        InputField responseAssertTypeInput)
    {
        return new InputFieldsForm(lifetime)
            .AddInputField(assertEndpointInput, ComponentsIdentity.AssertEndpoint)
            .AddInputField(responseActTypeInput, ComponentsIdentity.ResponseActType)
            .AddInputField(responseAssertTypeInput, ComponentsIdentity.ResponseAssertType);
    }

    private static void AdviseToChangeAssertRoute(
        BeTextBox requestEndpointTextBox,
        BeTextBox assertEndpointTextBox,
        ISolution solution,
        Lifetime lifetime)
    {
        var getEndpoints = solution.GetAllRoutes(i => i.Verb.ToString() == "GET");
        requestEndpointTextBox.Text.Change.Advise(lifetime, newUrl =>
        {
            var baseUrl = newUrl.RemoveLastPart().TrimEnd('/');
            var pattern = "^" + Regex.Escape(baseUrl) + @"/\{[^}]+\}$";
            var targetEndpoint = getEndpoints.FirstOrDefault(i => Regex.IsMatch(i, pattern));
            if (targetEndpoint is null) return;
            assertEndpointTextBox.Text.Value = targetEndpoint;
        });
    }

    private static void AdviseToSetTypeFromEndpoint(
        BeTextBox textBox,
        ISolution solution,
        SearchableClassPicker classPicker,
        string httpVerb,
        Func<AspNetHttpEndpoint?, IType?> typeResolver,
        Lifetime lifetime)
    {
        textBox.Text.Change.Advise(lifetime, newUrl =>
        {
            var endpoint = solution.ResolveHttpEndpoint<AspNetHttpEndpoint>(newUrl, httpVerb);
            solution.Locks.ExecuteWithReadLock(() =>
            {
                var responseType = typeResolver(endpoint)?.GetUnwrappedType().GetTypeElement();
                if (responseType is not IClass typed) return;
                classPicker.Value.SetValue(typed);
            });
        });
    }

    private void AdviseToSetEntity(BeTextBox actEndpointTextBox, ISolution solution, Lifetime lifetime)
    {
        actEndpointTextBox.Text.Change.Advise(lifetime, newUrl =>
            {
                var endpoint = solution.ResolveHttpEndpoint<AspNetHttpEndpoint>(newUrl, ActHttpVerb);
                if (endpoint is null) return;
                solution.Locks.ExecuteWithReadLock(() =>
                {
                    var entity = endpoint.FindEntityUsingControllerName(solutionTypeElementsAccessor.Classes);

                    if (entity is not null)
                        SearchableClassPickers[ComponentsIdentity.EntityInput].Value.SetValue(entity);
                });
            });
    }

    private static Func<AspNetHttpEndpoint?, IType?> ResolverFromReturnValue
        => endpoint => endpoint?.ActionMethod.ReturnType;

    protected virtual Func<AspNetHttpEndpoint?, IType?> ResolverFromParameter
        => endpoint => endpoint?.ActionMethod.Parameters.FirstOrDefault()?.Type;

    private static Func<IHttpEndpoint, bool> FilterByHttpVerb(string httpVerb)
        => endpoint => endpoint.Verb.ToString() == httpVerb;
}
