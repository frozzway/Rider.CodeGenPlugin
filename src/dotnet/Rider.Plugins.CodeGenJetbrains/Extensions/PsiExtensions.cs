using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.Threading;
using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Web.AspRouteTemplates.EndpointsProvider;
using JetBrains.ReSharper.Feature.Services.Web.AspRouteTemplates.EndpointsProvider.AspNetHttpEndpoints;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.Util;
using Rider.Plugins.CodeGenJetbrains.FluidModels;

namespace Rider.Plugins.CodeGenJetbrains.Extensions;

public static class PsiExtensions
{
    public static IEnumerable<ITypeElement> GetAllTypeElements(this ISolution solution)
    {
        var psiServices = solution.GetComponent<IPsiServices>();
        var scope = psiServices.Symbols.GetSymbolScope(LibrarySymbolScope.NONE, caseSensitive: false);
        var matches = scope.GetAllTypeElementsGroupedByName();
        return matches;
    }

    public static bool IsPredefinedType(this IType type, IClrTypeName clrName)
        => type is IDeclaredType declaredType && declaredType.GetClrName().FullName == clrName.FullName;

    public static FType ToFType(this ITypeElement? typeElement)
        => new()
        {
            Name = typeElement?.ShortName ?? string.Empty,
            Namespace = typeElement?.GetContainingNamespace().QualifiedName ?? string.Empty,
        };

    public static string? GetSummary(this IDeclaredElement? element)
    {
        var xmlDoc = element?.GetXMLDoc(inherit: false);
        var summaryNode = xmlDoc?.SelectSingleNode("summary");
        return summaryNode?.InnerText.Trim();
    }

    public static string GetExpectedNamespace(this IProjectItem projectItem)
    {
        var solution = projectItem.GetSolution();
        string? namespaceStr = null;
        solution.Locks.ExecuteWithWriteLock(() => namespaceStr = projectItem.CalculateExpectedNamespace(CSharpLanguage.Instance!));
        return namespaceStr!;
    }

    public static T? ResolveHttpEndpoint<T>(this ISolution solution, string url, string httpVerb) where T: IHttpEndpoint
    {
        var endpointsProvider = solution.GetComponent<IHttpEndpointsProvider>();
        var roots = endpointsProvider.GetEndpointsTreeRoots();
        var allRoutes = roots.FlattenRoutes().OfType<T>();
        return allRoutes.FirstOrDefault(e =>
            e.BuildRouteString() == url
            && e.Verb.ToString() == httpVerb);
    }
}
