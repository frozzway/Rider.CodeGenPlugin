using System;
using System.Linq;
using JetBrains.Application.DataContext;
using JetBrains.Application.Threading;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl;
using JetBrains.TextControl.DataContext;
using Rider.Plugins.CodeGenJetbrains.Execution.Abstract;
using Rider.Plugins.CodeGenJetbrains.Execution.Common;
using Rider.Plugins.CodeGenJetbrains.Extensions;
using Rider.Plugins.CodeGenJetbrains.FluidModels;
using Rider.Plugins.CodeGenJetbrains.FluidModels.RepositoryGeneration;

namespace Rider.Plugins.CodeGenJetbrains.Execution.RepositoryGeneration;

public class RepositoryGenerationExecutor : IExecutor<RepositoryGenerationDto>
{
    private const string Template = "RepositoryGeneration.Repository.cs.liquid";
    private const string TemplateMethodPrefix = "RepositoryGeneration.Methods.";

    public void Execute(IDataContext context, RepositoryGenerationDto dto)
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
        RepositoryGenerationDto dto)
    {
        var model = ToFModel(dto);
        declaration.GetSolution().Locks.ExecuteWithReadLock(() =>
        {
            declaration.GetPsiServices().Transactions.Execute(
                "Generate Repository Method",
                () =>
                {
                    foreach (var method in dto.Methods)
                    {
                        var template = TemplateMethodPrefix + method + ".cs.liquid";
                        var renderer = new FluidRenderer(model, template);
                        AddMethodAtCaret(context, declaration, renderer.RenderContent());
                    }
                });
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

    private static void AddMethodAtCaret(IDataContext context, IClassLikeDeclaration declaration, string content)
    {
        var textControl = context.GetData(TextControlDataConstants.TEXT_CONTROL)!;

        var caretOffset = textControl.Caret.Offset();

        // Берём только прямых детей класса (MemberDeclarations именно для этого)
        var members = declaration.MemberDeclarations.OfType<IClassMemberDeclaration>().ToList();

        // Находим "следующий" member после каретки
        var nextMember = members
            .Select(m => new { Member = m, Range = m.GetDocumentRange() })
            .Where(x => x.Range.IsValid())
            .OrderBy(x => x.Range.TextRange.StartOffset)
            .FirstOrDefault(x => x.Range.TextRange.StartOffset > caretOffset)
            ?.Member;

        var factory = CSharpElementFactory.GetInstance(declaration, applyCodeFormatter: false);
        var method = (IMethodDeclaration)factory.CreateTypeMemberDeclaration(content);

        if (nextMember != null)
            declaration.AddClassMemberDeclarationBefore(method, nextMember);
        else
            declaration.AddClassMemberDeclaration(method);
    }
}
