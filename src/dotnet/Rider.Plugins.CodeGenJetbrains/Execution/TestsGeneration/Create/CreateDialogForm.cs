using JetBrains.IDE.UI.Extensions;
using JetBrains.Rider.Model.UIAutomation;
using Rider.Plugins.CodeGenJetbrains.Execution.Common;
using Rider.Plugins.CodeGenJetbrains.Extensions;

namespace Rider.Plugins.CodeGenJetbrains.Execution.TestsGeneration.Create;

public class CreateDialogForm(SolutionTypeElementsAccessor accessor)
    : BaseCreateUpdateDialogForm<CreateTestDto>(accessor)
{
    protected override string DefaultCaseName => "Успешное добавление сущности";
    protected override string Verb => "Create";
    protected override string ActHttpVerb => "POST";

    public override CreateTestDto GetDto()
    {
        var filePrefixTextBox = (BeTextBox)_mainForm.GetInputField(ComponentsIdentity.FilesPrefix)!.InputControl;
        var testCaseTextBox = (BeTextBox)_mainForm.GetInputField(ComponentsIdentity.TestCaseName)!.InputControl;
        var assertEndpointTextBox = (BeTextBox)_assertForm.GetInputField(ComponentsIdentity.AssertEndpoint)!.InputControl;
        var actEndpointTextBox = (BeTextBox)_mainForm.GetInputField(ComponentsIdentity.ActEndpoint)!.InputControl;

        var assertRequestDto = _assertForm.Enabled.Value
            ? new AssertRequestDto(
                ResponseActType: _searchableClassPickers[ComponentsIdentity.ResponseActType].Value.GetValue(),
                ResponseAssertType: _searchableClassPickers[ComponentsIdentity.ResponseAssertType].Value.GetValue(),
                RequestAssertEndpoint: assertEndpointTextBox.TryGetText().DefaultIfEmpty(DefaultEndpoint).RemoveLastPart()
            )
            : null;

        return new CreateTestDto(
            FilesPrefix: filePrefixTextBox.TryGetText().DefaultIfEmpty(_defaultFilePrefix),
            CaseName: testCaseTextBox.TryGetText().DefaultIfEmpty(DefaultCaseName),
            RequestEndpoint: actEndpointTextBox.TryGetText().DefaultIfEmpty(DefaultEndpoint),
            RequestType: _searchableClassPickers[ComponentsIdentity.RequestActType].Value.GetValue(),
            AssertRequestInfo: assertRequestDto);
    }
}
