using System.Collections.Generic;
using JetBrains.Collections.Viewable;
using JetBrains.DataFlow;

namespace Rider.Plugins.CodeGenJetbrains.Extensions;

public static class DataFlowExtensions
{
    public static IViewableList<T> ReplaceItems<T>(this IViewableList<T> list, IEnumerable<T> values)
    {
        list.Clear();
        foreach (var value in values)
        {
            list.Add(value);
        }
        return list;
    }

    public static IListEvents<T> ReplaceItems<T>(this IListEvents<T> list, IEnumerable<T> values)
    {
        list.Clear();
        foreach (var value in values)
        {
            list.Add(value);
        }
        return list;
    }
}
