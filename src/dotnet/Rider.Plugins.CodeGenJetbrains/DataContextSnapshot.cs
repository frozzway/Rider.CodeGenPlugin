using System.Collections.Generic;
using JetBrains.Application.DataContext;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ReSharper.Psi.DataContext;
using JetBrains.TextControl.DataContext;

namespace Rider.Plugins.CodeGenJetbrains;

public class DataContextSnapshot : IDataContext
{
    private readonly Dictionary<string, object> _snapshot = new();

    public DataContextSnapshot(IDataContext context)
    {
        // Извлекаем данные сразу в конструкторе, пока оригинальный контекст жив
        SaveData(context, ProjectModelDataConstants.PROJECT_MODEL_ELEMENT);
        SaveData(context, ProjectModelDataConstants.SOLUTION);
        SaveData(context, ProjectModelDataConstants.PROJECT_MODEL_ELEMENTS);
        SaveData(context, TextControlDataConstants.TEXT_CONTROL);
        SaveData(context, PsiDataConstants.SOURCE_FILE);
    }

    private void SaveData<T>(IDataContext context, DataConstant<T> constant) where T : class
    {
        var value = context.GetData(constant);
        if (value != null)
        {
            _snapshot[constant.Id] = value;
        }
    }

    /// <summary>
    /// Реализация метода интерфейса IDataContext для получения данных из снимка
    /// </summary>
    public T? GetData<T>(DataConstant<T> dataConstant) where T : class
    {
        if (_snapshot.TryGetValue(dataConstant.Id, out var value))
        {
            return (T)value;
        }

        return null;
    }

    public IDataContext Prolongate(Lifetime lifetime)
    {
        throw new System.NotImplementedException();
    }

    public DataContextState? State { get; }
    public bool IsEmpty => _snapshot.Count == 0;
}
