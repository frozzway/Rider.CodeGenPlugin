using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Application.UI.Controls.JetPopupMenu;
using JetBrains.Application.UI.Controls.JetPopupMenu.Detail;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.UI.Automation;
using JetBrains.ReSharper.Feature.Services.Web.AspRouteTemplates.EndpointsProvider;
using JetBrains.ReSharper.Feature.Services.Web.AspRouteTemplates.EndpointsProvider.AspNetHttpEndpoints;
using JetBrains.Rider.Model.UIAutomation;
using JetBrains.UI.RichText;
using JetBrains.Util.Media;

namespace Rider.Plugins.CodeGenJetbrains.Extensions;

public static class EndpointCompletionExtensions
{
    public record EndpointItem(string Verb, string Route);

    /// <summary>
    /// Подключает completion endpoint-ов (только URL) к BeTextBox.
    /// </summary>
    public static BeTextBox WithEndpointsCompletionOld(
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

    public static BeTextBox WithEndpointsCompletion(
        this BeTextBox textBox,
        ISolution solution,
        Lifetime lifetime,
        Func<IHttpEndpoint, bool>? filter = null)
    {
        var allEndpoints = GetAllEndpointsWithVerbs(solution, filter);

        return textBox.WithSimpleCompletion(
            lifetime,
            getItems: filterText =>
            {
                var filtered = allEndpoints
                    .Where(x => x.Route.Contains(filterText, StringComparison.OrdinalIgnoreCase));

                // Преобразование в элементы меню
                return filtered.Select(CreateMenuItem);
            },
            translateItem: item =>
            {
                if (item.In.Key is string route)
                    item.Out = route;
            },
            solution: solution);
    }

    private static JetPopupMenuItem CreateMenuItem(EndpointItem item)
    {
        var richText = new RichText();

        var verbStyle = GetStyleForVerb(item.Verb);
        richText.Append($"{item.Verb} ", verbStyle);
        richText.Append(item.Route, TextStyle.Default);

        var descriptor = new SimpleMenuItem
        {
            Text = richText,
            Style = MenuItemStyle.Enabled,
            // Можно добавить иконку, если нужно
            // Icon = ...
        };

        // Первый аргумент (Key) — это то, что вставится в текст (item.Route).
        // Второй аргумент (Descriptor) — это то, как элемент выглядит (RichText).
        return new JetPopupMenuItem(item.Route, descriptor);
    }

    private static TextStyle GetStyleForVerb(string verb)
    {
        var color = verb.ToUpperInvariant() switch
        {
            "GET" => JetRgbaColor.FromRgb(30, 136, 229),     // Синий
            "POST" => JetRgbaColor.FromRgb(67, 160, 71),     // Зеленый
            "PUT" => JetRgbaColor.FromRgb(251, 140, 0),      // Оранжевый
            "DELETE" => JetRgbaColor.FromRgb(229, 57, 53),   // Красный
            "PATCH" => JetRgbaColor.FromRgb(142, 36, 170),   // Фиолетовый
            _ => JetRgbaColor.FromRgb(0, 0, 0)               // Дефолтный
        };

        var style = TextStyle.Default;
        style.ForegroundColor = color;
        //style.FontStyle = JetFontStyles.Bold;
        return style;
    }

    public static IEnumerable<EndpointItem> GetAllEndpointsWithVerbs(ISolution solution,
        Func<IHttpEndpoint, bool>? filter = null)
    {
        filter ??= _ => true;

        var endpointsProvider = solution.GetComponent<IHttpEndpointsProvider>();
        var roots = endpointsProvider.GetEndpointsTreeRoots();

        // Получаем плоский список IHttpEndpoint
        var httpEndpoints = FlattenRoutes(roots)
            .OfType<IHttpEndpoint>()
            .Where(filter);

        // Разворачиваем каждый endpoint в набор (Verb, Route)
        foreach (var endpoint in httpEndpoints.OrderBy(e => e.BuildRouteString()))
        {
            var routeString = endpoint.BuildRouteString();
            var method = endpoint.Verb.ToString();
            if (method is not null)
                yield return new EndpointItem(method, routeString);
        }
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

    public static string RemoveLastPart(this string url) => Regex.Replace(url, @"\{[^}]+\}$", "");
}
