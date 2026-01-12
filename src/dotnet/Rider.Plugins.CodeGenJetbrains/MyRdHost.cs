using System.Threading.Tasks;
using JetBrains.Application.Parts;
using JetBrains.Core;
using JetBrains.IDE.UI;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.Rd.Tasks;
using JetBrains.ReSharper.Feature.Services.Protocol;
using JetBrains.Rider.Model;
using Rider.Plugins.CodeGenJetbrains.Model;

namespace Rider.Plugins.CodeGenJetbrains;

[SolutionComponent(Instantiation.ContainerAsyncAnyThreadUnsafe)]
public class MyRdHost
{
    private readonly ISolution _solution;

    public MyRdHost(ISolution solution)
    {
        _solution = solution;

        var model = _solution.GetProtocolSolution().GetRdCodeGenJetbrainsModel();
        model.MyCall.SetAsync(HandleCall);
        model.MyIconCall.SetAsync(HandleIconCall);
    }

    private async Task<RdCallResponse> HandleCall(Lifetime lt, RdCallRequest request)
    {
        await Task.Delay(1000, lt);
        return lt.Execute(() => new RdCallResponse(request.MyField.Length));
    }

    private async Task<IconModel> HandleIconCall(Lifetime lt, Unit _)
    {
        await Task.Delay(1000, lt);
        return lt.Execute(() => _solution.GetComponent<IIconHost>().Transform(MyIconIds.RiderIconId));
    }
}
