using System.Collections.Generic;
using JetBrains.ReSharper.Feature.Services.CSharp.Generate;
using JetBrains.ReSharper.Feature.Services.Generate;
using JetBrains.ReSharper.Psi.CSharp;

namespace Rider.Plugins.CodeGenJetbrains.Actions.GenerationContextActions.RepositoryMethods;

[GeneratorBuilder(GenerationActionsKinds.RepositoryMethods, typeof(CSharpLanguage))]
public class RepositoryMethodsBuilder : GeneratorBuilderBase<CSharpGeneratorContext>
{
    protected override bool IsAvailable(CSharpGeneratorContext context) => true;

    protected override bool HasProcessableElements(CSharpGeneratorContext context, IEnumerable<IGeneratorElement> elements) => true;
}
