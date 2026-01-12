using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.UI.Automation;
using JetBrains.ReSharper.Feature.Services.Web.AspRouteTemplates.EndpointsProvider;
using JetBrains.ReSharper.Feature.Services.Web.AspRouteTemplates.EndpointsProvider.AspNetHttpEndpoints;
using JetBrains.Rider.Model.UIAutomation;

namespace Rider.Plugins.CodeGenJetbrains.Extensions;

public static class EndpointCompletionExtensions
{
    /// <summary>
    /// Подключает completion endpoint-ов (только URL) к BeTextBox.
    /// </summary>
    public static BeTextBox WithEndpointsCompletion(
        this BeTextBox textBox,
        ISolution solution,
        Lifetime lifetime,
        Func<IHttpEndpoint, bool>? filter = null)
    {
        var allRoutes = GetAllRoutes(solution, filter);

        return textBox.WithSimpleCompletion(
            lifetime,
            filter => allRoutes.Where(route => route.Contains(filter, StringComparison.OrdinalIgnoreCase)),
            solution: solution);
    }

    /// <summary>
    /// Возвращает список маршрутов в URL представлении (без HTTP Verbs)
    /// </summary>
    public static string[] GetAllRoutes(this ISolution solution, Func<IHttpEndpoint, bool>? filter = null)
    {
        filter ??= _ => true;

        var endpointsProvider = solution.GetComponent<IHttpEndpointsProvider>();
        var roots = endpointsProvider.GetEndpointsTreeRoots();
        var allRoutes = FlattenRoutes(roots)
            .OfType<IHttpEndpoint>()
            .Where(filter)
            .Select(BuildRouteString)
            .Order()
            .ToArray();

        return allRoutes;
    }

    /// <summary>
    /// Рекурсивно обходит дерево и собирает строки маршрутов из IEndpoint.
    /// </summary>
    public static IEnumerable<IEndpoint> FlattenRoutes(this IEnumerable<IEndpointsTreeNode> nodes)
    {
        foreach (var node in nodes)
        {
            // 1. Возвращаем endpoints текущего узла
            foreach (var endpoint in node.Endpoints)
            {
                yield return endpoint;
            }

            // 2. Рекурсивно спускаемся в дочерние узлы
            foreach (var route in FlattenRoutes(node.Children))
            {
                yield return route;
            }
        }
    }

    /// <summary>
    /// Преобразует IEndpoint в строку URL (без HTTP Verbs).
    /// </summary>
    public static string BuildRouteString(this IEndpoint endpoint)
    {
        // IEndpoint имеет свойство RouteSegments (IReadOnlyList<IRouteSegment>)
        // IRouteSegment переопределяет ToString(), возвращая текст сегмента.
        // Собираем путь через "/", добавляя ведущий слеш.
        var path = string.Join("/", endpoint.RouteSegments.Select(s => s.ToString()));

        return path.StartsWith("/") ? path : "/" + path;
    }
}
