using System;
using System.IO;
using JetBrains.Application.Threading;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.Properties;
using JetBrains.ReSharper.Feature.Services.Protocol;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CodeStyle;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.Rider.Model;
using JetBrains.Util;
using JetBrains.Util.dataStructures;

namespace Rider.Plugins.CodeGenJetbrains.Extensions;

public static class ProjectModelExtensions
{
    public static IProjectItem MarkDirtyAndRefresh(this IProjectItem item)
    {
        var solution = item.GetSolution();
        var protocolSolution = solution.GetProtocolSolution();
        var fileSystemModel = protocolSolution.GetFileSystemModel();
        var path = item.Location.FullPath;
        fileSystemModel.RefreshPaths.Sync(new RdFsRefreshRequest([path], async: false));
        return item;
    }

    public static ReadOnlyFrugalLocalList<IProjectFolder> GetSubFoldersWithLock(this IProjectFolder folder, string name)
    {
        var solution = folder.GetSolution();
        ReadOnlyFrugalLocalList<IProjectFolder>? result = null;
        solution.Locks.ExecuteWithWriteLock(() => result = folder.GetSubFolders(name));
        return result!.Value;
    }

    public static ReadOnlyFrugalLocalList<IProjectFile> GetSubFilesWithLock(this IProjectFolder folder, string name)
    {
        var solution = folder.GetSolution();
        ReadOnlyFrugalLocalList<IProjectFile>? result = null;
        solution.Locks.ExecuteWithWriteLock(() => result = folder.GetSubFiles(name));
        return result!.Value;
    }

    public static IProjectFile CreateFileWithContent(
        this IProjectFolder parentFolder,
        string fileName,
        string fileContent)
    {
        var solution = parentFolder.GetSolution();
        IProjectFile? newFile = null;

        solution.Locks.ExecuteWithWriteLock(() =>
        {
            // 1. Создаём физический файл на диске
            var filePath = parentFolder.Location.Combine(fileName);
            File.WriteAllText(filePath.FullPath, fileContent);

            // 2. Создаём файл в проектной модели
            var project = parentFolder.GetProject();
            var propertiesFactory = solution.GetComponent<ProjectFilePropertiesFactory>();
            var fileProperties = propertiesFactory.CreateProjectFileProperties(project.ProjectProperties);
            var folderImpl = (ProjectFolderImpl)parentFolder;
            newFile = folderImpl.DoCreateFile(fileName, filePath, fileProperties);
            if (fileName.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                newFile.Properties.SetBuildAction(BuildAction.COMPILE, project.GetRandomTargetFrameworkId());
        });

        return newFile!;
    }

    public static IProjectFolder CreateFolder(
        this IProjectFolder parentFolder,
        string folderName)
    {
        var solution = parentFolder.GetSolution();
        IProjectFolder? newFolder = null;

        solution.Locks.ExecuteWithWriteLock(() =>
        {
            // 1. Создаём физическую директорию на диске
            var folderPath = parentFolder.Location.Combine(folderName);
            Directory.CreateDirectory(folderPath.FullPath);

            // 2. Создаём папку в проектной модели
            var folderImpl = (ProjectFolderImpl)parentFolder;
            newFolder = folderImpl.DoCreateFolder(
                new ProjectFolderPath(folderName, VirtualFileSystemPath.GetEmptyPathFor(InteractionContext.SolutionContext))
            );
        });

        return newFolder!;
    }

    public static IProjectFile ApplyFormatter(this IProjectFile projectFile)
    {
        // 1. Получаем решение и PSI сервисы
        var solution = projectFile.GetSolution();
        using var readLock = solution.Locks.UsingReadLock();
        var psiServices = solution.GetPsiServices();

        // 2. Конвертируем IProjectFile в IPsiSourceFile
        // Это мостик между файловой системой и анализом кода
        var sourceFile = projectFile.ToSourceFile();
        if (sourceFile == null) return projectFile;

        // 3. Все изменения должны быть внутри транзакции (Write Lock)
        // Используем 'Execute' для создания транзакции PSI
        psiServices.Transactions.Execute("Reformat Generated Code", () =>
        {
            // Важно: убедимся, что PSI дерево синхронизировано с текстом документа
            // Если вы только что записали текст в файл, PSI может быть еще старым
            psiServices.Files.CommitAllDocuments();

            // 4. Получаем PSI-файл для C#
            // GetDominantPsiFile<CSharpLanguage>() вернет корень синтаксического дерева
            var psiFile = sourceFile.GetDominantPsiFile<CSharpLanguage>();
            if (psiFile == null) return;

            // 5. Получаем сервис форматтера для языка C#
            var languageService = CSharpLanguage.Instance.LanguageService();
            if (languageService == null) return;

            var codeFormatter = languageService.CodeFormatter!;

            // 6. Вызываем форматирование
            // CodeFormatProfile.DEFAULT означает "жесткое" форматирование по настройкам пользователя
            codeFormatter.Format(psiFile, CodeFormatProfile.DEFAULT);
        });

        return projectFile;
    }
}
