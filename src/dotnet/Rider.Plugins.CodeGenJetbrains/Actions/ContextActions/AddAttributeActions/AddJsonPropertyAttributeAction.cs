using System;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.ContextActions;
using Rider.Plugins.CodeGenJetbrains.Extensions;

namespace Rider.Plugins.CodeGenJetbrains.Actions.ContextActions.AddAttributeActions;

[ContextAction(
    GroupType = typeof(CSharpContextActions),
    Name = "Add JsonPropertyName attribute",
    Description = "Adds JsonPropertyName attribute to properties",
    Priority = 1)]
public class AddJsonPropertyAttributeAction(ICSharpContextActionDataProvider provider) : AddAttributeAction(provider)
{
    protected override string AttributeName => "JsonPropertyName";
    protected override Func<string, string> ParametersInitializer => propertyName => $"\"{propertyName.ToSnakeCaseRegex()}\"";
}
