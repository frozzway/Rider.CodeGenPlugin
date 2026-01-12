using System;
using System.Collections.Generic;
using JetBrains.Application.UI.Controls.BulbMenu.Anchors;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.ContextActions;
using JetBrains.ReSharper.Feature.Services.Intentions;
using JetBrains.ReSharper.Feature.Services.Resources;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.Util;

namespace Rider.Plugins.CodeGenJetbrains.Actions.ContextActions.AddAttributeActions;

public abstract class AddAttributeAction(ICSharpContextActionDataProvider provider) : IContextAction
{
    protected abstract string AttributeName { get; }
    protected abstract Func<string, string> ParametersInitializer { get; }

    public bool IsAvailable(IUserDataHolder cache)
    {
        var propertyDeclaration = provider.GetSelectedElement<IPropertyDeclaration>();
        return propertyDeclaration != null && propertyDeclaration.IsValid();
    }

    public IEnumerable<IntentionAction> CreateBulbItems()
    {
        var property = provider.GetSelectedElement<IPropertyDeclaration>();
        if (property == null) yield break;

        // Вариант 1: Только для текущего свойства
        var singleAction = new AddAttributeBulbAction(property, AttributeName, ParametersInitializer, isRecursive: false);
        var hammerIcon = BulbThemedIcons.ContextAction.Id;

        var submenuAnchor = new SubmenuAnchor(
            IntentionsAnchors.ContextActionsAnchor,
            SubmenuBehavior.Executable
        );

        yield return new IntentionAction(singleAction, singleAction.Text, hammerIcon, submenuAnchor);

        // Вариант 2: Для всех публичных свойств в классе
        if (property.GetContainingTypeDeclaration() is IClassLikeDeclaration)
        {
            var recursiveAction = new AddAttributeBulbAction(property, AttributeName, ParametersInitializer, isRecursive: true);
            yield return new IntentionAction(recursiveAction, recursiveAction.Text, null, submenuAnchor);
        }
    }
}
