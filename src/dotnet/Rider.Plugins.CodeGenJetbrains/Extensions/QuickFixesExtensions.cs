using JetBrains.Application.Progress;
using JetBrains.Application.Threading;
using JetBrains.Application.UI.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Intentions.Scoped.Scopes;
using JetBrains.ReSharper.Intentions.CSharp.QuickFixes;
using JetBrains.ReSharper.Intentions.QuickFixes;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;

namespace Rider.Plugins.CodeGenJetbrains.Extensions;

public static class QuickFixesExtensions
{
    public static IProjectFile FixImportsInFile(this IProjectFile projectFile, bool withProgress = false)
    {
        var solution = projectFile.GetSolution();

        using var writeLock = solution.Locks.UsingWriteLock();

        // 1. Получаем PSI Source File
        var sourceFile = projectFile.ToSourceFile();
        if (sourceFile is null) return projectFile;

        // 2. Получаем PSI-дерево (например, для C#)
        var psiFile = sourceFile.GetPrimaryPsiFile();
        if (psiFile is null) return projectFile;

        // 3. Ищем "Seed" reference.
        // Нам нужен ЛЮБОЙ reference, чтобы просто создать инстанс ImportTypeFix.
        // Scoped-механизм все равно пересканирует файл заново.
        IReference seedRef = psiFile
            .Descendants<IReferenceName>()
            .FirstOrDefault()
            .GetReferences()
            .FirstOrDefault();

        if (seedRef == null) return projectFile; // В файле вообще нет ссылок, фиксить нечего

        solution.GetPsiServices().Transactions.DocumentTransactionManager.DoTransaction("Fix imports in file",
            () => withProgress
                ? ExecuteImportTypeFixWithProgress(sourceFile, seedRef, solution)
                : ExecuteImportTypeFix(sourceFile, seedRef, solution));

        return projectFile;
    }

    private static bool ExecuteImportTypeFix(
        IPsiSourceFile sourceFile,
        IReference seedRef,
        ISolution solution)
    {
        var scope = new SourceFileScope(sourceFile);

        new ImportTypeFix(seedRef).ExecuteAction(
            solution,
            scope,
            null,
            NullProgressIndicator.Create()
        );
        new ImportExtensionMemberFix(seedRef).ExecuteAction(
            solution,
            scope,
            null,
            NullProgressIndicator.Create()
        );

        return true;
    }

    private static bool ExecuteImportTypeFixWithProgress(
        IPsiSourceFile sourceFile,
        IReference seedRef,
        ISolution solution)
    {
        var scope = new SourceFileScope(sourceFile);

        solution.GetComponent<UITaskExecutor>().SingleThreaded.ExecuteTask("Importing references", TaskCancelable.No,
            parentIndicator =>
            {
                parentIndicator.Start(2);
                parentIndicator.TaskName = "Importing missing types...";
                using (var subProgress = parentIndicator.AdvanceNested(1))
                {
                    new ImportTypeFix(seedRef).ExecuteAction(
                        solution,
                        scope,
                        null,
                        subProgress
                    );
                }
                parentIndicator.TaskName = "Importing extension methods...";
                using (var subProgress = parentIndicator.AdvanceNested(1))
                {
                    new ImportExtensionMemberFix(seedRef).ExecuteAction(
                        solution,
                        scope,
                        null,
                        subProgress
                    );
                }
            });

        return true;
    }
}
