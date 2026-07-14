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
using JetBrains.ReSharper.Psi.CSharp.Tree;
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
        if (element is null)
            return null;

        using var readLock = element.GetSolution().Locks.UsingReadLock();
        using var compilationContext = CompilationContextCookie.GetExplicitUniversalContextIfNotSet();
        var xmlDoc = element.GetXMLDoc(false);
        var summaryNode = xmlDoc?.SelectSingleNode("summary");
        return summaryNode?.InnerText.Trim();
    }

    public static string GetExpectedNamespace(this IProjectItem projectItem)
    {
        var solution = projectItem.GetSolution();
        using var readLock = solution.Locks.UsingReadLock();
        return projectItem.CalculateExpectedNamespace(CSharpLanguage.Instance!);
    }

    public static T? ResolveHttpEndpoint<T>(this ISolution solution, string url, string httpVerb) where T: IHttpEndpoint
    {
        var endpointsProviders = solution.GetComponents2<IHttpEndpointsProvider>();
        var roots = endpointsProviders.SelectMany(p => p.GetEndpointsTreeRoots());
        var allRoutes = roots.FlattenRoutes().OfType<T>();
        return allRoutes.FirstOrDefault(e =>
            e.BuildRouteString() == url
            && e.Verb.ToString() == httpVerb);
    }

    public static bool HasMatchingSignature(this IMethodDeclaration newMethod, IEnumerable<IMethodDeclaration> existingMethods)
    {
        return existingMethods.Any(existing =>
        {
            if (existing.DeclaredName != newMethod.DeclaredName)
                return false;

            // Совпадает ли количество generic-аргументов (например, метод<T> и метод<T, K>)
            if (existing.TypeParameterDeclarations.Count != newMethod.TypeParameterDeclarations.Count)
                return false;

            var existingParams = existing.Params.ParameterDeclarations;
            var newParams = newMethod.Params.ParameterDeclarations;

            // Совпадает ли количество параметров?
            if (existingParams.Count != newParams.Count)
                return false;

            // Попарно сравниваем типы и модификаторы параметров
            for (int i = 0; i < existingParams.Count; i++)
            {
                // Берем текстовое представление типа (например, "int", "string", "MyDto")
                // TypeUsage может быть null у некорректных деклараций, подстрахуемся
                var existingTypeStr = existingParams[i].TypeUsage?.GetText() ?? string.Empty;
                var newTypeStr = newParams[i].TypeUsage?.GetText() ?? string.Empty;

                // Сравниваем типы (убираем возможные лишние пробелы для надежности)
                if (existingTypeStr.Replace(" ", "") != newTypeStr.Replace(" ", ""))
                {
                    return false;
                }

                // Дополнительно проверяем модификаторы (ref, out, in), если они используются в вашем маппере
                if (existingParams[i].Kind != newParams[i].Kind)
                {
                    return false;
                }
            }

            // Имя, количество параметров и все их типы совпали — это дубликат сигнатуры
            return true;
        });
    }
}
