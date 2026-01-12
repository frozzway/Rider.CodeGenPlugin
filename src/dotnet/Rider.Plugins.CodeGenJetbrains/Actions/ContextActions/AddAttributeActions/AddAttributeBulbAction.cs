using System;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.TextControl;

namespace Rider.Plugins.CodeGenJetbrains.Actions.ContextActions.AddAttributeActions;

internal class AddAttributeBulbAction(
    IPropertyDeclaration targetProperty,
    string attributeName,
    Func<string, string> parametersInitializer,
    bool isRecursive) : BulbActionBase
{
    protected override Action<ITextControl>? ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
    {
        var factory = CSharpElementFactory.GetInstance(targetProperty);

        if (!isRecursive)
        {
            // Добавляем атрибут только текущему свойству
            AddAttributeToProperty(factory, targetProperty);
        }
        else
        {
            // Ищем родительский класс
            if (targetProperty.GetContainingTypeDeclaration() is not IClassLikeDeclaration classDeclaration)
                return null;

            var publicProperties = classDeclaration.MemberDeclarations
                .Where(m => m is IPropertyDeclaration
                {
                    IsStatic: false,
                    DeclaredElement.AccessibilityDomain.DomainType: JetBrains.ReSharper.Psi.AccessibilityDomain
                        .AccessibilityDomainType.PUBLIC
                })
                .OfType<IPropertyDeclaration>();

            // Проходим по всем свойствам класса
            foreach (var prop in publicProperties)
                AddAttributeToProperty(factory, prop);
        }

        return null; // Возвращаем Action, если нужно что-то сделать с текстовым курсором после вставки
    }

    private void AddAttributeToProperty(CSharpElementFactory factory, IPropertyDeclaration property)
    {
        if (property.Attributes.Any(a => a.TypeReference?.GetName() == attributeName))
            return;

        // 1. Получаем имя свойства (например, "Id")
        var propertyName = property.DeclaredName;

        // 2. Создаем атрибут из строки.
        // Используем полное имя класса атрибута, чтобы ReSharper сам добавил using.
        var attributeText = $"{attributeName}({parametersInitializer(propertyName)})";

        // Достаем IAttribute из dummy-декларации, чтобы не создавать вручную IAttribute
        var dummyCode = $"[{attributeText}] int X {{ get; }}";
        var dummyDeclaration = factory.CreateTypeMemberDeclaration(dummyCode) as IPropertyDeclaration;
        var newAttribute = dummyDeclaration?.Attributes.SingleItem!;

        // 3. Добавляем атрибут к свойству (перед или после существующих, или создаем новую секцию)
        property.AddAttributeAfter(newAttribute, null);
    }

    public override string Text => isRecursive
        ? $"Add {attributeName} attribute to all public properties"
        : $"Add {attributeName} attribute";
}

