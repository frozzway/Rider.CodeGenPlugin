using System;
using System.Linq;
using JetBrains.IDE.UI.Extensions;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.Feature.Services.Web.AspRouteTemplates.EndpointsProvider.AspNetHttpEndpoints;
using JetBrains.ReSharper.Psi;
using JetBrains.Rider.Model.UIAutomation;
using Rider.Plugins.CodeGenJetbrains.Execution.Common;
using Rider.Plugins.CodeGenJetbrains.Extensions;
using Rider.Plugins.CodeGenJetbrains.UI.Components;
using PredefinedType = JetBrains.ReSharper.Psi.PredefinedType;

namespace Rider.Plugins.CodeGenJetbrains.Execution.Generation.TestsGeneration.Update;

public class UpdateDialog(SolutionTypeElementsAccessor accessor)
    : BaseCreateUpdateDialog<UpdateTestDto>(accessor)
{
    protected override string DefaultCaseName => "Успешное редактирование сущности";
    protected override string Verb => "Update";
    protected override string ActHttpVerb => "PUT";

    protected override InputFieldsForm InitializeAssertForm(
        Lifetime lifetime,
        InputField<string> assertEndpointInput,
        InputField _,
        InputField responseAssertTypeInput)
    {
        return new InputFieldsForm(lifetime)
            .AddInputField(assertEndpointInput, ComponentsIdentity.AssertEndpoint)
            .AddInputField(responseAssertTypeInput, ComponentsIdentity.ResponseAssertType);
    }

    protected override Func<AspNetHttpEndpoint?, IType?> ResolverFromParameter
        => endpoint =>
        {
            var parameters = endpoint?.ActionMethod.Parameters;
            var param = parameters?.ElementAtOrDefault(1);
            if (param == null || param.Type.IsPredefinedType(PredefinedType.CANCELLATION_TOKEN_FQN))
                return parameters?.FirstOrDefault()?.Type;
            return param.Type;
        };

    public override UpdateTestDto GetDto()
    {
        var filePrefixTextBox = (BeTextBox)MainForm.GetInputField(ComponentsIdentity.FilesPrefix)!.InputControl;
        var testCaseTextBox = (BeTextBox)MainForm.GetInputField(ComponentsIdentity.TestCaseName)!.InputControl;
        var assertEndpointTextBox = (BeTextBox)AssertForm.GetInputField(ComponentsIdentity.AssertEndpoint)!.InputControl;
        var actEndpointTextBox = (BeTextBox)MainForm.GetInputField(ComponentsIdentity.ActEndpoint)!.InputControl;

        var assertRequestDto = AssertForm.Enabled.Value
            ? new AssertRequestDto(
                ResponseAssertType: SearchableClassPickers[ComponentsIdentity.ResponseAssertType].Value.GetValue(),
                RequestAssertEndpoint: assertEndpointTextBox.TryGetText().DefaultIfEmpty(DefaultEndpoint).RemoveLastPart()
            )
            : null;

        return new UpdateTestDto(
            FilesPrefix: filePrefixTextBox.TryGetText().DefaultIfEmpty(DefaultFilePrefix),
            CaseName: testCaseTextBox.TryGetText().DefaultIfEmpty(DefaultCaseName),
            RequestEndpoint: actEndpointTextBox.TryGetText().DefaultIfEmpty(DefaultEndpoint).RemoveLastPart(),
            RequestType: SearchableClassPickers[ComponentsIdentity.RequestActType].Value.GetValue(),
            AssertRequestInfo: assertRequestDto);
    }
}
