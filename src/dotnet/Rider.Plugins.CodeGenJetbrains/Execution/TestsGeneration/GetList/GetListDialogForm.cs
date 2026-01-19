using System;
using System.Linq;
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
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.Rider.Model.UIAutomation;
using Rider.Plugins.CodeGenJetbrains.Execution.Abstract;
using Rider.Plugins.CodeGenJetbrains.Execution.Common;
using Rider.Plugins.CodeGenJetbrains.Extensions;
using Rider.Plugins.CodeGenJetbrains.UI.Components;

namespace Rider.Plugins.CodeGenJetbrains.Execution.TestsGeneration.GetList;

public class GetListDialogForm(SolutionTypeElementsAccessor solutionTypeElementsAccessor) : IDialogForm<GetListTestDto>
{
    private string _defaultFilePrefix;
    private const string DefaultCaseName = "Успешное получение списка сущности";
    private const string DefaultEndpoint = "/api/endpoint/";
    private const string DefaultResponseType = "PagedResponse<>";

    private SearchableClassPicker _requestTypePicker;
    private SearchableClassPicker _entityTypePicker;
    private InputFieldsForm _mainForm;
    private BeCheckbox _removeMigrationEntitiesCheckbox;
    private BeCheckbox _actWithQueryParamCheckbox;

    public BeDialog GetDialog(Lifetime lifetime, IDataContext context, string title)
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
        _defaultFilePrefix = $"Get{folderName.Singularize()}List";
        _requestTypePicker = new SearchableClassPicker(lifetime, dialogHost, solutionTypeElementsAccessor.Classes);
        _entityTypePicker = new SearchableClassPicker(lifetime, dialogHost, solutionTypeElementsAccessor.Classes);
        _removeMigrationEntitiesCheckbox = BeControls.GetCheckBox(
            "Remove migration entities", Guid.NewGuid().ToString(),
            lifetime, initialValue: false);
        _actWithQueryParamCheckbox = BeControls.GetCheckBox(
            "Use query params", Guid.NewGuid().ToString(),
            lifetime, initialValue: true);

        var grid = BeControls.GetAutoGrid();

        var filePrefixInput = InputFieldFactory.CreateTextBox("File names prefix:", lifetime, placeholder: _defaultFilePrefix);
        var testCaseInput = InputFieldFactory.CreateTextBox("Test case name:", lifetime, placeholder: DefaultCaseName);
        var requestEndpointInput = InputFieldFactory.CreateTextBox("Request endpoint:", lifetime, placeholder: DefaultEndpoint,
            configure: box => box.WithEndpointsCompletion(solution, lifetime, FilterByHttpVerb("GET", "POST")));
        var entityInput = new InputField(lifetime, "Entity", _entityTypePicker.Control);
        var requestTypeInput = new InputField(lifetime, "Request:", _requestTypePicker.Control);
        var responseTypeName = InputFieldFactory.CreateTextBox("Response:", lifetime, placeholder: DefaultResponseType,
            configure: box => box.WithTypeCompletionShort(solution, lifetime, CSharpLanguage.Instance!));

        _mainForm = new InputFieldsForm(lifetime)
            .AddInputField(filePrefixInput, "files-prefix")
            .AddInputField(testCaseInput, "testcase-name")
            .AddInputField(requestEndpointInput, "request-endpoint")
            .AddInputField(entityInput, "entity-input")
            .AddInputField(requestTypeInput, "request-type")
            .AddInputField(responseTypeName, "response-type");

        grid.AddElements(_mainForm.Grid, _actWithQueryParamCheckbox, _removeMigrationEntitiesCheckbox);

        ((BeTextBox)requestEndpointInput.InputControl).Text.Change.Advise(lifetime, newUrl =>
        {
            var endpoint = solution.ResolveHttpEndpoint<AspNetHttpEndpoint>(newUrl, "GET");
            if (endpoint is null) return;
            solution.Locks.ExecuteWithReadLock(() =>
            {
                var responseType = endpoint.ActionMethod.ReturnType.GetUnwrappedType();
                if (responseType is not null)
                    responseTypeName.CurrentValue.SetValue(responseType.GetPresentableName(CSharpLanguage.Instance!));

                var requestType = endpoint.ActionMethod.Parameters.FirstOrDefault()?.Type.GetUnwrappedType().GetTypeElement();
                if (requestType is IClass typed)
                    _requestTypePicker.Value.SetValue(typed);
            });
        });

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

    private static Func<IHttpEndpoint, bool> FilterByHttpVerb(params string[] httpVerbs)
        => endpoint => httpVerbs.Any(httpVerb => endpoint.Verb.ToString() == httpVerb);

    public GetListTestDto GetDto()
    {
        var filePrefixTextBox = (BeTextBox)_mainForm.GetInputField("files-prefix")!.InputControl;
        var testCaseTextBox = (BeTextBox)_mainForm.GetInputField("testcase-name")!.InputControl;
        var requestEndpointTextBox = (BeTextBox)_mainForm.GetInputField("request-endpoint")!.InputControl;
        var responseTypeTextBox = (BeTextBox)_mainForm.GetInputField("response-type")!.InputControl;

        return new GetListTestDto(
            FilesPrefix: filePrefixTextBox.TryGetText().DefaultIfEmpty(_defaultFilePrefix),
            CaseName: testCaseTextBox.TryGetText().DefaultIfEmpty(DefaultCaseName),
            RequestEndpoint: requestEndpointTextBox.TryGetText().DefaultIfEmpty(DefaultEndpoint),
            RemoveMigrationEntities: _removeMigrationEntitiesCheckbox.Property.Value!.Value,
            ActWithQueryParams: _actWithQueryParamCheckbox.Property.Value!.Value,
            ResponseActTypeName: responseTypeTextBox.TryGetText().DefaultIfEmpty(DefaultResponseType),
            RequestType: _requestTypePicker.Value.GetValue(),
            Entity: _entityTypePicker.Value.GetValue());
    }
}
