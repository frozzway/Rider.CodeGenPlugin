using System.Collections.Generic;
using JetBrains.Application.Parts;
using JetBrains.Core;
using JetBrains.ProjectModel;
using JetBrains.Rd.Tasks;
using JetBrains.ReSharper.Feature.Services.Protocol;
using Rider.Plugins.CodeGenJetbrains.Model;

namespace Rider.Plugins.CodeGenJetbrains;

[SolutionComponent(Instantiation.ContainerAsyncAnyThreadUnsafe)]
public class RdDatabaseCaller(ISolution solution)
{
    private readonly RdCodeGenJetbrainsModel _model = solution.GetProtocolSolution().GetRdCodeGenJetbrainsModel();

    public IReadOnlyList<DataSourceDto> GetDataSources()
        => ((RdCall<Unit, List<DataSourceDto>>)_model.GetDataSources).Sync(Unit.Instance);

    public IReadOnlyList<DbStructureItem> GetSourceStructure(string sourceId)
        => ((RdCall<string, List<DbStructureItem>>)_model.GetStructure).Sync(sourceId);

    public IReadOnlyList<DbTableDto> GetTables(string structureId)
        => ((RdCall<string, List<DbTableDto>>)_model.GetTables).Sync(structureId);

    public IReadOnlyList<DbColumnDto> GetColumns(string tableId)
        => ((RdCall<string, DbTableColumnsDto>)_model.GetColumns).Sync(tableId).Columns;
}
