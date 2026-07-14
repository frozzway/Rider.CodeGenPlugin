using System.Collections.Generic;
using Rider.Plugins.CodeGenJetbrains.Execution.Abstract;
using Rider.Plugins.CodeGenJetbrains.Execution.Common;

namespace Rider.Plugins.CodeGenJetbrains.Execution.Generation.TestsGeneration.GetGrid;

public class GetGridMapperGenerationService(
    IContextAccessor contextAccessor,
    SolutionTypeElementsAccessor typeElementsAccessor)
    : MapperGenerationService(contextAccessor, typeElementsAccessor)
{
    protected override string MapperClassPostfix => "Mapper";
    protected override IEnumerable<string> GetMapperMethodsContent(string methodName, string entityName)
    {
        return [$"public static partial {entityName}GridResponse ToGridResponse(this {entityName} entity);"];
    }
}
