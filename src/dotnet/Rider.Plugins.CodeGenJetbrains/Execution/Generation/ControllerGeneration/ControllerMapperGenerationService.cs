using System;
using System.Collections.Generic;
using Rider.Plugins.CodeGenJetbrains.Execution.Abstract;
using Rider.Plugins.CodeGenJetbrains.Execution.Common;

namespace Rider.Plugins.CodeGenJetbrains.Execution.Generation.ControllerGeneration;

public class ControllerMapperGenerationService(
    IContextAccessor contextAccessor,
    SolutionTypeElementsAccessor typeElementsAccessor)
    : MapperGenerationService(contextAccessor, typeElementsAccessor)
{
    protected override string MapperClassPostfix => "MapperApi";

    protected override IEnumerable<string> GetMapperMethodsContent(string methodName, string entityName)
        => methodName switch
        {
            ControllerMethods.AddAsync
                => [$"public static partial Add{entityName}Command ToCreateCommand(this Add{entityName}Request request);"],
            ControllerMethods.UpdateAsync
                => [$"public static partial Update{entityName}Command ToUpdateCommand(this Update{entityName}Request request, Guid id);"],
            ControllerMethods.GetGridAsync
                =>
                [
                    $"public static partial {entityName}GridResponse ToGridResponse(this {entityName}Grid item);",
                    $"public static partial Get{entityName}GridQuery ToGridQuery(this Get{entityName}GridRequest request);"
                ],
            ControllerMethods.GetExcelAsync
                => [$"public static partial Get{entityName}GridExcelQuery ToExcelQuery(this Get{entityName}GridExcelRequest request);"],
            ControllerMethods.GetAsync
                => [$"public static partial {entityName}Response ToResponse(this {entityName} entity);"],
            ControllerMethods.GetManyAsync
                => [$"public static partial {entityName}Response ToResponse(this {entityName} entity);"],
            _ => throw new ArgumentOutOfRangeException(nameof(methodName), methodName, null)
        };
}
