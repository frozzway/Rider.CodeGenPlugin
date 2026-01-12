using System;
using System.Linq;
using System.Text.RegularExpressions;
using Humanizer;
using JetBrains.Application.DataContext;
using JetBrains.Application.Threading;
using JetBrains.IDE.UI;
using JetBrains.IDE.UI.Extensions;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ReSharper.Feature.Services.Web.AspRouteTemplates.EndpointsProvider.AspNetHttpEndpoints;
using JetBrains.ReSharper.Psi;
using JetBrains.Rider.Model.UIAutomation;
using Rider.Plugins.CodeGenJetbrains.Execution.Abstract;
using Rider.Plugins.CodeGenJetbrains.Execution.Common;
using Rider.Plugins.CodeGenJetbrains.Extensions;
using Rider.Plugins.CodeGenJetbrains.UI;
using Rider.Plugins.CodeGenJetbrains.UI.Components;

namespace Rider.Plugins.CodeGenJetbrains.Execution.TestsGeneration.Create;

public class CreateDialogForm(SolutionTypeElementsAccessor solutionTypeElementsAccessor) : IDialogForm<CreateTestDto>
{
    private string _defaultFilePrefix;
    private const string DefaultCaseName = "Успешное добавление сущности";
    private const string DefaultEndpoint = "/api/endpoint/";

    private SearchableClassPicker _requestTypePicker;
    private SearchableClassPicker _assertActTypePicker;
    private SearchableClassPicker _assertAssertTypePicker;

    private InputFieldsForm _mainForm;
    private InputFieldsForm _assertForm;

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

    private BeDialog GetDialogImpl(
        Lifetime lifetime,
        IDataContext context,
        IProjectFolder folder,
        string title)
    {
        var dialogHost = context.GetComponent<IDialogHost>();

        var folderName = folder.Name;
        var solution = folder.GetSolution();
        _defaultFilePrefix = $"Create{folderName.Singularize()}";
        _requestTypePicker = new SearchableClassPicker(lifetime, dialogHost, solutionTypeElementsAccessor.Classes);
        _assertActTypePicker = new SearchableClassPicker(lifetime, dialogHost, solutionTypeElementsAccessor.Classes);
        _assertAssertTypePicker = new SearchableClassPicker(lifetime, dialogHost, solutionTypeElementsAccessor.Classes);

        var grid = BeControls.GetAutoGrid();

        var filePrefixInput = InputFieldFactory.CreateTextBox("File names prefix:", lifetime, placeholder: _defaultFilePrefix);
        var testCaseInput = InputFieldFactory.CreateTextBox("Test case name:", lifetime, placeholder: DefaultCaseName);
        var requestEndpointInput = InputFieldFactory.CreateTextBox("Request endpoint:", lifetime, placeholder: DefaultEndpoint,
            configure: box => box.WithEndpointsCompletion(solution, lifetime, FilterByHttpVerb("POST")));
        var requestTypeInput = new InputField(lifetime, "Request type:", _requestTypePicker.Control);

        _mainForm = new InputFieldsForm(lifetime)
            .AddInputField(filePrefixInput, "files-prefix")
            .AddInputField(testCaseInput, "testcase-name")
            .AddInputField(requestEndpointInput, "request-endpoint")
            .AddInputField(requestTypeInput, "request-type");

        var assertCheckbox = BeControls.GetCheckBox("Enable", Guid.NewGuid().ToString(), lifetime)!;
        assertCheckbox.Property.Advise(lifetime, newValue => _assertForm.Enabled.Value = newValue ?? false);

        var assertEndpointInput = InputFieldFactory.CreateTextBox("Assert endpoint:", lifetime, placeholder: DefaultEndpoint,
            configure: box => box.WithEndpointsCompletion(solution, lifetime, FilterByHttpVerb("GET")));
        var responseActTypeInput = new InputField(lifetime, "Response act type:", _assertActTypePicker.Control);
        var responseAssertTypeInput = new InputField(lifetime, "Response assert type:", _assertAssertTypePicker.Control);

        _assertForm = new InputFieldsForm(lifetime)
            .AddInputField(assertEndpointInput, "assert-endpoint")
            .AddInputField(responseActTypeInput, "response-act-type")
            .AddInputField(responseAssertTypeInput, "response-assert type");

        _assertForm.Enabled.Value = false;

        var assertGrid = BeControls.GetAutoGrid();
        assertGrid.AddElements(assertCheckbox.WithMargin(PluginBeControls.GetMargin(top: 5)), _assertForm.Grid);

        grid
            .AddElement(_mainForm.Grid.WithTitledBorder("Parameters"))
            .AddElement(assertGrid.WithTitledBorder("Assert stage", bottomMargin: 10));

        var requestTextBox = (BeTextBox)requestEndpointInput.InputControl;
        var assertTextBox = (BeTextBox)assertEndpointInput.InputControl;
        AdviseToSetTypeFromEndpoint(requestTextBox, solution, _assertActTypePicker, "POST", ResolverFromReturnValue, lifetime);
        AdviseToSetTypeFromEndpoint(requestTextBox, solution, _requestTypePicker, "POST", ResolverFromFirstParameter, lifetime);
        AdviseToSetTypeFromEndpoint(assertTextBox, solution, _assertAssertTypePicker, "GET", ResolverFromReturnValue, lifetime);
        AdviseToChangeAssertRoute(requestTextBox, assertTextBox, solution, lifetime);

        return grid.InDialog(
            title: title,
            id: Guid.NewGuid().ToString(),
            isResizable: false,
            size: new BeControlSizeFixed(
                width: BeControlSizeType.HUGE,
                height: BeControlSizeType.FIT_TO_CONTENT));
    }

    private void AdviseToChangeAssertRoute(
        BeTextBox requestEndpointTextBox,
        BeTextBox assertEndpointTextBox,
        ISolution solution,
        Lifetime lifetime)
    {
        var getEndpoints = solution.GetAllRoutes(i => i.Verb.ToString() == "GET");
        requestEndpointTextBox.Text.Change.Advise(lifetime, newUrl =>
        {
            var baseUrl = newUrl.TrimEnd('/');
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
                var responseType = typeResolver(endpoint)?.GetUnwrappedTypeElement();
                if (responseType is not IClass typed) return;
                classPicker.Value.SetValue(typed);
            });
        });
    }

    private static Func<AspNetHttpEndpoint?, IType?> ResolverFromReturnValue
        => endpoint => endpoint?.ActionMethod.ReturnType;

    private static Func<AspNetHttpEndpoint?, IType?> ResolverFromFirstParameter
        => endpoint => endpoint?.ActionMethod.Parameters.FirstOrDefault()?.Type;

    private static Func<IHttpEndpoint, bool> FilterByHttpVerb(string httpVerb)
        => endpoint => endpoint.Verb.ToString() == httpVerb;

    public CreateTestDto GetDto()
    {
        var assertTextBox = (BeTextBox)_assertForm.GetInputField("assert-endpoint")!.InputControl;
        var filePrefixTextBox = (BeTextBox)_mainForm.GetInputField("files-prefix")!.InputControl;
        var requestEndpointTextBox = (BeTextBox)_mainForm.GetInputField("request-endpoint")!.InputControl;
        var testCaseTextBox = (BeTextBox)_mainForm.GetInputField("testcase-name")!.InputControl;

        var assertRequestDto = _assertForm.Enabled.Value
            ? new AssertRequestDto(
                ResponseActType: _assertActTypePicker.Value.GetValue(),
                ResponseAssertType: _assertAssertTypePicker.Value.GetValue(),
                RequestEndpoint: RemoveLastPart(assertTextBox.TryGetText().DefaultIfEmpty(DefaultEndpoint)))
            : null;

        return new CreateTestDto(
            FilesPrefix: filePrefixTextBox.TryGetText().DefaultIfEmpty(_defaultFilePrefix),
            CaseName: testCaseTextBox.TryGetText().DefaultIfEmpty(DefaultCaseName),
            RequestEndpoint: requestEndpointTextBox.TryGetText().DefaultIfEmpty(DefaultEndpoint),
            RequestType: _requestTypePicker.Value.GetValue(),
            AssertRequestInfo: assertRequestDto);
    }

    private static string RemoveLastPart(string url) => Regex.Replace(url, @"\{[^}]+\}$", "");
}
