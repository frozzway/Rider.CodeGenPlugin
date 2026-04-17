using System.Linq;
using JetBrains.Application.DataContext;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using Rider.Plugins.CodeGenJetbrains.Execution.Abstract;
using Rider.Plugins.CodeGenJetbrains.Execution.Common;
using Rider.Plugins.CodeGenJetbrains.Extensions;
using Rider.Plugins.CodeGenJetbrains.FluidModels;
using Rider.Plugins.CodeGenJetbrains.FluidModels.RepositoryGeneration;

namespace Rider.Plugins.CodeGenJetbrains.Execution.Generation.RepositoryGeneration;

public class RepositoryGenerationExecutor(IContextAccessor contextAccessor) : IExecutor<RepositoryGenerationDto>
{
    private const string Template = "RepositoryGeneration.Repository.cs.liquid";
    private const string TemplateMethodPrefix = "RepositoryGeneration.Methods.";

    public void Execute(IDataContext context, RepositoryGenerationDto dto)
    {
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
        RepositoryGenerationDto dto)
    {
        var model = ToFModel(dto);
        contextAccessor.PsiServices.Transactions.Execute(
            "Generate Repository Method",
            () =>
            {
                foreach (var method in dto.Methods)
                {
                    var template = TemplateMethodPrefix + method + ".cs.liquid";
                    var renderer = new FluidRenderer(model, template);
                    declaration.AddMethodAtCaret(context, renderer.RenderContent());
                }
            });

        declaration.GetSourceFile()!.ToProjectFile()!.FixImportsInFile();
    }

    private static void GenerateImpl(IProjectFolder folder, RepositoryGenerationDto dto)
    {
        var model = ToFModel(dto);
        SetFType(model, folder, dto);
        var renderer = new FluidRenderer(model, Template);
        var fileName = dto.EntityType.ShortName + "Repository.cs";
        var file = folder.CreateFileWithContent(fileName, fileContent: renderer.RenderContent());
        file.TryAddToGit()
            .FixImportsInFile(withProgress: true)
            .ApplyFormatter()
            .MarkDirtyAndRefresh();
    }

    private static void SetFType(FRepository model, IProjectFolder folder, RepositoryGenerationDto dto)
    {
        var expectedNamespace = folder.GetExpectedNamespace();
        var repositoryName = dto.EntityType.ShortName + "Repository";
        model.Type = new FType
        {
            Namespace = expectedNamespace,
            Name = repositoryName
        };
    }

    private static FRepository ToFModel(RepositoryGenerationDto dto)
    {
        var model = new FRepository
        {
            Entity = dto.EntityType.ToFType(),
            EntityList = dto.EntityListType.ToFType(),
            Table = new FTable
            {
                Name = dto.TableName,
                Schema = dto.SchemaName
            },
            Properties = [..dto.Properties.Select(ToFEntityProperty)],
            Methods = [..dto.Methods]
        };
        return model;
    }

    private static FEntityProperty ToFEntityProperty(DbProperty property)
        => new()
        {
            Name = property.Name,
            IsPrimaryKey = property.IsPrimaryKey,
            Column = property.Column
        };
}
