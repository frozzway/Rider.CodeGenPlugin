using System.Collections.Generic;
using JetBrains.Application.DataContext;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ReSharper.Feature.Services.Generate.Actions;
using JetBrains.ReSharper.Psi;

namespace Rider.Plugins.CodeGenJetbrains.Actions.GenerationActions.RepositoryMethods;

[GenerateProvider]
public class RepositoryMethodsProvider : IGenerateWorkflowProvider
{
    public IEnumerable<IGenerateActionWorkflow> CreateWorkflow(IDataContext dataContext)
    {
        var solution = dataContext.GetData(ProjectModelDataConstants.SOLUTION);
        if (solution == null) yield break;
        var iconManager = solution.GetComponent<PsiIconManager>();
        var icon = iconManager.GetImage(CLRDeclaredElementType.METHOD)!;
        yield return new RepositoryMethodsWorkflow(icon);
    }
}
