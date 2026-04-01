using JetBrains.IDE.UI.Extensions;
using JetBrains.Rider.Model.UIAutomation;
using Rider.Plugins.CodeGenJetbrains.Execution.Common;
using Rider.Plugins.CodeGenJetbrains.Extensions;

namespace Rider.Plugins.CodeGenJetbrains.Execution.Generation.TestsGeneration.Create;

public class CreateDialog(SolutionTypeElementsAccessor accessor)
    : BaseCreateUpdateDialog<CreateTestDto>(accessor)
{
    protected override string DefaultCaseName => "Успешное добавление сущности";
    protected override string Verb => "Create";
    protected override string ActHttpVerb => "POST";
    protected override bool EntityInputEnabled => false;

    public override CreateTestDto GetDto()
    {
        var filePrefixTextBox = (BeTextBox)MainForm.GetInputField(ComponentsIdentity.FilesPrefix)!.InputControl;
        var testCaseTextBox = (BeTextBox)MainForm.GetInputField(ComponentsIdentity.TestCaseName)!.InputControl;
        var assertEndpointTextBox = (BeTextBox)AssertForm.GetInputField(ComponentsIdentity.AssertEndpoint)!.InputControl;
        var actEndpointTextBox = (BeTextBox)MainForm.GetInputField(ComponentsIdentity.ActEndpoint)!.InputControl;

        var assertRequestDto = AssertForm.Enabled.Value
            ? new AssertRequestDto(
                ResponseActType: SearchableClassPickers[ComponentsIdentity.ResponseActType].Value.GetValue(),
                ResponseAssertType: SearchableClassPickers[ComponentsIdentity.ResponseAssertType].Value.GetValue(),
                RequestAssertEndpoint: assertEndpointTextBox.TryGetText().DefaultIfEmpty(DefaultEndpoint).RemoveLastPart()
            )
            : null;

        return new CreateTestDto(
            FilesPrefix: filePrefixTextBox.TryGetText().DefaultIfEmpty(DefaultFilePrefix),
            CaseName: testCaseTextBox.TryGetText().DefaultIfEmpty(DefaultCaseName),
            RequestEndpoint: actEndpointTextBox.TryGetText().DefaultIfEmpty(DefaultEndpoint),
            RequestType: SearchableClassPickers[ComponentsIdentity.RequestActType].Value.GetValue(),
            AssertRequestInfo: assertRequestDto);
    }
}
