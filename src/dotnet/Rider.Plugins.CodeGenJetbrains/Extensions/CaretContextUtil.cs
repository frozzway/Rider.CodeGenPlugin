using JetBrains.Application.DataContext;
using JetBrains.Application.Threading;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ReSharper.Feature.Services.Navigation.ContextNavigation;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.DataContext;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl.DataContext;

namespace Rider.Plugins.CodeGenJetbrains.Extensions;

extern alias rt;

public static class CaretContextUtil
{
    public static bool IsCaretInsideCSharpClassButNotMethod(IDataContext dataContext,
        [rt::System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] out IClassLikeDeclaration? classDecl)
    {
        classDecl = null;
        var solution = dataContext.GetData(ProjectModelDataConstants.SOLUTION);
        var textControl = dataContext.GetData(TextControlDataConstants.TEXT_CONTROL);

        if (solution == null || textControl == null)
            return false;

        using var lReadLock = solution.Locks.UsingReadLock();
        var sourceFile = dataContext.GetData(PsiDataConstants.SOURCE_FILE);
        if (sourceFile != null)
        {
            var props = sourceFile.Properties;
            if (!props.ProvidesCodeModel || props.IsNonUserFile || props.IsGeneratedFile)
                return false;
        }

        var nodeUnderCaret = dataContext.GetSelectedTreeNode<ITreeNode>();

        if (nodeUnderCaret?.GetContainingTypeDeclaration() is not IClassLikeDeclaration containingType)
            return false;

        if (containingType is IInterfaceDeclaration)
            return false;

        if (nodeUnderCaret.GetContainingNode<IMethodDeclaration>() != null)
            return false;

        classDecl = containingType;

        return true;
    }
}
