using Humanizer;
using JetBrains.Application.DataContext;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using Rider.Plugins.CodeGenJetbrains.Execution.Abstract;
using Rider.Plugins.CodeGenJetbrains.Execution.Common;
using Rider.Plugins.CodeGenJetbrains.Extensions;
using Rider.Plugins.CodeGenJetbrains.FluidModels;
using Rider.Plugins.CodeGenJetbrains.FluidModels.ControllerGeneration;

namespace Rider.Plugins.CodeGenJetbrains.Execution.Generation.ControllerGeneration;

public class ControllerGenerationExecutor(
    IContextAccessor contextAccessor,
    ControllerMapperGenerationService mapperService)
    : IExecutor<ControllerGenerationDto>
{
    private const string Template = "ControllerGeneration.Controller.cs.liquid";
    private const string TemplateMethodPrefix = "ControllerGeneration.Methods.";

    public void Execute(IDataContext context, ControllerGenerationDto dto)
    {
        mapperService.AddMapperMethods(dto.Entity.ShortName, dto.Methods);

        switch (contextAccessor.Target)
        {
            case ActionTarget.Folder targetFolder:
                GenerateImpl(targetFolder.ProjectFolder, dto);
                break;

            case ActionTarget.Declaration targetClass:
                GenerateImpl(contextAccessor.Snapshot, targetClass.ClassDeclaration, dto);
                break;
        }
    }

    private void GenerateImpl(
        IDataContext context,
        IClassLikeDeclaration declaration,
        ControllerGenerationDto dto)
    {
        var model = ToFModel(dto);
        contextAccessor.PsiServices.Transactions.Execute(
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

        var file = declaration.GetSourceFile()!.ToProjectFile()!;
        file.FixImportsInFile();
    }

    private static void GenerateImpl(IProjectFolder folder, ControllerGenerationDto dto)
    {
        var model = ToFModel(dto);
        SetFType(model, folder, dto);
        var fileName = dto.ControllerName + ".cs";
        var renderer = new FluidRenderer(model, Template);
        var fileContent = renderer.RenderContent();
        var file = folder.CreateFileWithContent(fileName, fileContent);
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
