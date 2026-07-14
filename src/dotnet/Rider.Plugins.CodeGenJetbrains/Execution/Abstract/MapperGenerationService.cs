using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.ReSharper.Psi.Tree;
using Rider.Plugins.CodeGenJetbrains.Execution.Common;
using Rider.Plugins.CodeGenJetbrains.Extensions;

namespace Rider.Plugins.CodeGenJetbrains.Execution.Abstract;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract class MapperGenerationService(
    IContextAccessor contextAccessor,
    SolutionTypeElementsAccessor typeElementsAccessor)
{
    record ClassInfo(IClass Class, IReadOnlyList<DeclarationInfo> Declarations);
    record DeclarationInfo(IClassLikeDeclaration Declaration, IPsiSourceFile SourceFile);

    protected abstract string MapperClassPostfix { get; }

    public void AddMapperMethods(string entityName, IEnumerable<string> methodNames)
    {
        var mapperClsName = entityName + MapperClassPostfix;

        var mapperClasses = typeElementsAccessor.Classes
            .Where(cls => cls.ShortName.EndsWith(MapperClassPostfix)
                          && cls.Module.ContainingProjectModule as IProject == contextAccessor.Project)
            .Select(cls => new ClassInfo
            (
                Class: cls,
                Declarations: cls.GetDeclarations<IClassLikeDeclaration>()
                    .Select(decl => new DeclarationInfo
                    (
                        Declaration: decl,
                        SourceFile: decl.GetSourceFile()!
                    ))
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                    .Where(i => i.SourceFile is not null && !i.SourceFile.Properties.IsGeneratedFile)
                    .ToArray()
            ))
            .ToArray();

        var mapperDeclaration =
            GetDeclarationFromExistingClasses(mapperClasses, mapperClsName)
            ?? GetDeclarationFromNewFile(mapperClasses, mapperClsName);

        contextAccessor.PsiServices.Transactions.Execute("Add Mapper Methods",
            () => AddMapperMethods(mapperDeclaration, entityName, methodNames));

        var file = mapperDeclaration.GetSourceFile()!.ToProjectFile()!;
        file.TryAddToGit()
            .FixImportsInFile(withProgress: true)
            .ApplyFormatter()
            .MarkDirtyAndRefresh();
    }

    private static IClassLikeDeclaration? GetDeclarationFromExistingClasses(
        ClassInfo[] mapperClasses,
        string mapperClsName)
        => mapperClasses
            .FirstOrDefault(classInfo => classInfo.Class.ShortName == mapperClsName)
            ?.Declarations.FirstOrDefault()?.Declaration;

    private IClassLikeDeclaration GetDeclarationFromNewFile(
        ClassInfo[] mapperClasses,
        string mapperClsName)
    {
        var mapperFolder = mapperClasses
            .FirstOrDefault()?.Declarations.FirstOrDefault()?.SourceFile.ToProjectFile()?.ParentFolder;

        if (mapperFolder is null)
            mapperFolder = contextAccessor.Project.ProjectFile!.ParentFolder!.CreateFolder("Mapper");

        const string classTemplate=
            """
            using Riok.Mapperly.Abstractions;
            namespace {{Namespace}};

            [Mapper]
            public static partial class {{ClassName}} {}
            """;

        var mapperFile = mapperFolder.CreateFileWithContent(
            fileName: mapperClsName + ".cs",
            fileContent: FluidExtensions.RenderContent(
                templateText: classTemplate,
                templateModel: new
                {
                    Namespace = mapperFolder.GetExpectedNamespace(),
                    ClassName = mapperClsName
                }));

        var psiSourceFile = mapperFile.ToSourceFile()!;

        contextAccessor.PsiServices.Files.CommitAllDocuments();
        var csharpFile = (ICSharpFile)psiSourceFile.GetDominantPsiFile<CSharpLanguage>()!;
        return csharpFile.Descendants<IClassLikeDeclaration>().First();
    }

    private void AddMapperMethods(
        IClassLikeDeclaration declaration,
        string entityName,
        IEnumerable<string> methods)
    {
        var factory = CSharpElementFactory.GetInstance(declaration, applyCodeFormatter: false);
        var existingMethods = declaration.MethodDeclarations.ToList();
        foreach (var methodName in methods)
        {
            var contents = GetMapperMethodsContent(methodName, entityName: entityName);

            foreach (var content in contents)
            {
                var methodInstance = (IMethodDeclaration)factory.CreateTypeMemberDeclaration(content);

                if (!methodInstance.HasMatchingSignature(existingMethods))
                    declaration.AddClassMemberDeclaration(methodInstance);
            }
        }
    }

    protected abstract IEnumerable<string> GetMapperMethodsContent(string methodName, string entityName);
}
