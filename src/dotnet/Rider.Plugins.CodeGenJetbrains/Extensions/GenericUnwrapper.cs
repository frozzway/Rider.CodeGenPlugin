using System.Text.RegularExpressions;

namespace Rider.Plugins.CodeGenJetbrains.Extensions;

public static class GenericUnwrapper
{
    public static string GetInnerType(this string typeDeclaration)
    {
        // Паттерн для поиска содержимого самых глубоких скобок
        var pattern = @"<([^<>]+)>";
        var match = Regex.Match(typeDeclaration, pattern);

        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        // Если угловых скобок нет вообще, значит тип не дженерик.
        // В этом случае сам переданный тип и есть TargetType.
        return typeDeclaration.Trim();
    }
}
