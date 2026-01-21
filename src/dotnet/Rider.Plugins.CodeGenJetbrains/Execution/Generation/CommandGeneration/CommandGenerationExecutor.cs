using System;
using JetBrains.Application.DataContext;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using JetBrains.Util;
using Rider.Plugins.CodeGenJetbrains.Execution.Abstract;
using Rider.Plugins.CodeGenJetbrains.Execution.Common;
using Rider.Plugins.CodeGenJetbrains.Extensions;
using Rider.Plugins.CodeGenJetbrains.FluidModels;
using Rider.Plugins.CodeGenJetbrains.FluidModels.CommandGeneration;

namespace Rider.Plugins.CodeGenJetbrains.Execution.Generation.CommandGeneration;

public class CommandGenerationExecutor : IExecutor<CommandGenerationDto>
{
    private const string CommandTemplate = "CommandGeneration.Command.cs.liquid";
    private const string HandlerTemplate = "CommandGeneration.Handler.cs.liquid";
    private const string ResultTemplate = "CommandGeneration.Result.cs.liquid";

    public void Execute(IDataContext context, CommandGenerationDto dto)
    {
        var targetElement = context.GetData(ProjectModelDataConstants.PROJECT_MODEL_ELEMENT);

        if (targetElement is IProjectFolder folder)
            GenerateImpl(folder, dto);

        else throw new InvalidOperationException();
    }

    private static void GenerateImpl(IProjectFolder folder, CommandGenerationDto dto)
    {
        var commandShortName = dto.Name.TrimFromEnd("Command").TrimFromEnd("Query");

        if (folder.Name != commandShortName)
        {
            folder = folder.GetSubFoldersWithLock(commandShortName).FirstOrDefault()
                     ?? folder.CreateFolder(commandShortName);
        }

        var model = ToFModel(folder, dto, commandShortName);
        if (dto.ReturnType is null)
            folder.CreateFileFromModel(model.Result, ResultTemplate);
        folder.CreateFileFromModel(model.Command, CommandTemplate);
        folder.CreateFileFromModel(model.Handler, HandlerTemplate);
    }

    private static FBase ToFModel(
        IProjectFolder folder,
        CommandGenerationDto dto,
        string commandShortName)
    {
        var expectedNamespace = folder.GetExpectedNamespace();
        var model = new FBase
        {
            Command = new FCommand
            {
                Type = new FType
                {
                    Name = dto.Name,
                    Namespace = expectedNamespace
                }
            },
            Handler = new FHandler
            {
                Type = new FType
                {
                    Name = $"{commandShortName}Handler",
                    Namespace = expectedNamespace
                }
            },
            Result = new FResult
            {
                Type = new FType
                {
                    Name = $"{commandShortName}Result",
                    Namespace = expectedNamespace
                }
            },
            ReturnType = dto.ReturnType ?? $"{commandShortName}Result"
        };
        model.Command.Base = model;
        model.Handler.Base = model;
        return model;
    }
}
