using System;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.ContextActions;
using Rider.Plugins.CodeGenJetbrains.Extensions;

namespace Rider.Plugins.CodeGenJetbrains.Actions.ContextActions.AddAttributeActions;

[ContextAction(
    GroupType = typeof(CSharpContextActions),
    Name = "Add FromQuery attribute",
    Description = "Adds FromQuery attribute to properties",
    Priority = 1)]
public class AddFromQueryAttributeAction(ICSharpContextActionDataProvider provider) : AddAttributeAction(provider)
{
    protected override string AttributeName => "FromQuery";
    protected override Func<string, string> ParametersInitializer => propertyName => $"Name = \"{propertyName.ToSnakeCaseRegex()}\"";
}
