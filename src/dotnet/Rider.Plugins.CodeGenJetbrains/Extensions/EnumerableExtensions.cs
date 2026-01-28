using System;
using System.Collections.Generic;
using System.Linq;

namespace Rider.Plugins.CodeGenJetbrains.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<T> MoveToFirst<T>(this IEnumerable<T> source, Predicate<T> predicate)
    {
        // 1. Создаем копию списка, чтобы не ломать исходную коллекцию
        var list = source.ToList();

        // 2. Ищем индекс первого элемента, подходящего под условие
        var index = list.FindIndex(predicate);

        // 3. Если элемент найден и он не в начале
        if (index > 0)
        {
            var item = list[index];
            list.RemoveAt(index); // Сначала удаляем со старого места
            list.Insert(0, item); // Потом вставляем в начало
        }

        return list;
    }
}
