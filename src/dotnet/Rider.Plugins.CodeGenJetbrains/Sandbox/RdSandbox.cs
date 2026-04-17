using JetBrains.ProjectModel;

namespace Rider.Plugins.CodeGenJetbrains.Sandbox;

public static class RdSandbox
{
    public static void TestRdProtocol(ISolution solution)
    {
        // var protocolModel = solution.GetProtocolSolution().GetRdCodeGenJetbrainsModel();
        // if (protocolModel.GetDatabaseColumns is not RdCall<string, DbTableSchema> customCall)
        //     throw new InvalidCastException();
        // var result = customCall.Sync("tableName", new RpcTimeouts(TimeSpan.FromMilliseconds(200), TimeSpan.FromMinutes(5)));
        // return result;
    }
}
