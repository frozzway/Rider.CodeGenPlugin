using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Rider.Plugins.CodeGenJetbrains.Extensions;

public static class StringExtensions
{
    public static string DefaultIfEmpty(this string? targetString, string defaultValue)
        => string.IsNullOrEmpty(targetString) ? defaultValue : targetString;

    public static IEnumerable<string> SplitByCapitals(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return [];

        return Regex.Split(input, @"(?<!^)(?=[A-Z])").Where(s => s.Length > 0);
    }

    public static string ToSnakeCaseRegex(this string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // Находим места, где строчная буква или цифра граничит с заглавной,
        // вставляем нижнее подчеркивание и приводим всё к нижнему регистру.
        return Regex.Replace(text, "([a-z0-9])([A-Z])", "$1_$2").ToLower();
    }

    public static string ToPascalCase(this string snakeCaseString)
    {
        if (string.IsNullOrEmpty(snakeCaseString))
            return snakeCaseString;

        // Разделяем строку по нижнему подчеркиванию
        string[] words = snakeCaseString.Split('_', StringSplitOptions.RemoveEmptyEntries);

        // Делаем каждое слово с заглавной буквы и соединяем
        TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
        return string.Concat(words.Select(word => textInfo.ToTitleCase(word.ToLower())));
    }
}
