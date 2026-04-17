using System.Linq;
using JetBrains.ReSharper.Psi;

namespace Rider.Plugins.CodeGenJetbrains.Extensions;

public static class AspNetMethodTypeUnwrapper
{
    public static IType? GetUnwrappedType(this IType type)
    {
        using var compilationContext = CompilationContextCookie.GetExplicitUniversalContextIfNotSet();

        // 1. Если это Task<T>, достаем T
        if (type.IsGenericTask() || type.IsGenericValueTask())
        {
            var taskType = type as IDeclaredType;
            // Получаем подстановку для Task<TResult>
            var substitution = taskType?.GetSubstitution();
            var typeElement = taskType?.GetTypeElement();

            if (typeElement != null && substitution != null)
            {
                // Находим TResult среди type parameters
                var typeParam = typeElement.TypeParameters.FirstOrDefault(p => p.ShortName == "TResult");
                if (typeParam != null)
                {
                    // Рекурсивно разворачиваем результат Task-а
                    var innerType = substitution.Apply(typeParam);
                    return GetUnwrappedType(innerType);
                }
            }
        }

        // 2. Если это ActionResult<T>, достаем T.
        // Проверяем по имени, т.к. IsActionResult может не быть в вашем SDK
        if (IsActionResultGeneric(type))
        {
            var actionResultType = type as IDeclaredType;
            var substitution = actionResultType?.GetSubstitution();
            var typeElement = actionResultType?.GetTypeElement();

            if (typeElement != null && substitution != null)
            {
                // У ActionResult<TValue> параметр обычно называется TValue
                var typeParam = typeElement.TypeParameters.FirstOrDefault(p => p.ShortName == "TValue");
                if (typeParam != null)
                {
                    // Рекурсивно разворачиваем результат
                    var innerType = substitution.Apply(typeParam);
                    return GetUnwrappedType(innerType);
                }
            }
        }

        // 3. Если это просто Task (без T) или void -> значит payload-а нет
        if (type.IsTask() || type.IsVoid() || type.IsValueTask())
        {
            return null;
        }

        // 4. Если это ActionResult (не generic) -> payload-а нет (или он Object)
        if (IsActionResultNonGeneric(type))
        {
            return null;
        }

        // 5. В остальных случаях считаем, что это и есть наш payload (IdResponse и т.п.)
        return type;
    }

    private static bool IsActionResultGeneric(IType type)
    {
        return type is IDeclaredType dt &&
               dt.GetClrName().FullName == "Microsoft.AspNetCore.Mvc.ActionResult`1";
    }

    private static bool IsActionResultNonGeneric(IType type)
    {
        return type is IDeclaredType dt &&
               dt.GetClrName().FullName == "Microsoft.AspNetCore.Mvc.ActionResult";
    }
}
