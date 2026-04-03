using System;
using Humanizer;
using JetBrains.Application.DataContext;
using JetBrains.Application.Threading;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using Rider.Plugins.CodeGenJetbrains.Execution.Abstract;
using Rider.Plugins.CodeGenJetbrains.Execution.Common;
using Rider.Plugins.CodeGenJetbrains.Extensions;
using Rider.Plugins.CodeGenJetbrains.FluidModels;
using Rider.Plugins.CodeGenJetbrains.FluidModels.ControllerGeneration;

namespace Rider.Plugins.CodeGenJetbrains.Execution.Generation.ControllerGeneration;

public class ControllerGenerationExecutor : IExecutor<ControllerGenerationDto>
{
    private const string Template = "ControllerGeneration.Controller.cs.liquid";
    private const string TemplateMethodPrefix = "ControllerGeneration.Methods.";

    public void Execute(IDataContext context, ControllerGenerationDto dto)
    {
        var targetElement = context.GetData(ProjectModelDataConstants.PROJECT_MODEL_ELEMENT);

        if (targetElement is IProjectFolder folder)
            GenerateImpl(folder, dto);

        else if (CaretContextUtil.IsCaretInsideCSharpClassButNotMethod(context, out var declaration))
            GenerateImpl(context, declaration, dto);

        else throw new InvalidOperationException();
    }

    private static void GenerateImpl(
        IDataContext context,
        IClassLikeDeclaration declaration,
        ControllerGenerationDto dto)
    {
        var model = ToFModel(dto);
        declaration.GetSolution().Locks.ExecuteWithReadLock(() =>
        {
            declaration.GetPsiServices().Transactions.Execute(
                "Generate Controller Method",
                () =>
                {
                    foreach (var method in dto.Methods)
                    {
                        var template = TemplateMethodPrefix + method + ".cs.liquid";
                        var renderer = new FluidRenderer(model, template);
                        declaration.AddMethodAtCaret(context, renderer.RenderContent());
                    }
                });
        });

        declaration.GetSourceFile()!.ToProjectFile()!.FixImportsInFile();
    }

    private static void GenerateImpl(IProjectFolder folder, ControllerGenerationDto dto)
    {
        var model = ToFModel(dto);
        SetFType(model, folder, dto);
        var renderer = new FluidRenderer(model, Template);
        var fileName = dto.ControllerName + ".cs";
        var file = folder.CreateFileWithContent(fileName, fileContent: renderer.RenderContent());
        file.TryAddToGit()
            .FixImportsInFile(withProgress: true)
            .ApplyFormatter()
            .MarkDirtyAndRefresh();
    }

    private static void SetFType(FController model, IProjectFolder folder, ControllerGenerationDto dto)
    {
        var expectedNamespace = folder.GetExpectedNamespace();
        var controllerName = dto.ControllerName;
        model.Type = new FType
        {
            Namespace = expectedNamespace,
            Name = controllerName
        };
    }

    private static FController ToFModel(ControllerGenerationDto dto)
    {
        var model = new FController
        {
            Methods = dto.Methods,
            Entity = new FEntity
            {
                Type = dto.Entity.ToFType(),
                PluralName = dto.Entity.ShortName.Pluralize(),
                SummaryName = dto.EntitySummaryName
            },
            PermissionEnum = dto.PermissionEnum,
            PermissionCodes = dto.PermissionCodes
        };
        return model;
    }
}
