namespace Rider.Plugins.CodeGenJetbrains.Execution.RepositoryGeneration;

public record DbObject(string Id, string Name, bool IsDefault, bool IsPrimary)
{
    public override string ToString() => Name;
};
