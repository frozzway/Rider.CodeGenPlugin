using System;
using System.Diagnostics;
using JetBrains.Application.DataContext;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using Rider.Plugins.CodeGenJetbrains.Execution.Abstract;
using Rider.Plugins.CodeGenJetbrains.Extensions;

namespace Rider.Plugins.CodeGenJetbrains.Execution.Common;

public class ContextAccessor : IContextAccessor
{
    private IDataContext _context = null!;
    private ActionTarget? _target;
    private IProject? _project;

    public void Initialize(IDataContext contextSnapshot)
        => _context = contextSnapshot;

    public IDataContext Snapshot => _context;

    public ISolution Solution => _context.GetData(ProjectModelDataConstants.SOLUTION)
                                 ?? throw new InvalidOperationException("Solution not found");

    public IPsiServices PsiServices => Solution.GetPsiServices();

    public ActionTarget Target
    {
        get
        {
            if (_target != null)
                return _target;

            var element = _context.GetData(ProjectModelDataConstants.PROJECT_MODEL_ELEMENT);
            if (element is IProjectFolder folder)
                return _target = new ActionTarget.Folder(folder);

            if (CaretContextUtil.IsCaretInsideCSharpClassButNotMethod(_context, out var decl))
                return _target = new ActionTarget.Declaration(decl);

            throw new InvalidOperationException("Unsupported invocation context");
        }
    }

    public IProject Project
        => _project ??= Target switch
        {
            ActionTarget.Folder f => f.ProjectFolder.GetProject(),
            ActionTarget.Declaration d => d.ClassDeclaration.GetProject(),
            _ => throw new UnreachableException()
        } ?? throw new InvalidOperationException("Project not found");
}
