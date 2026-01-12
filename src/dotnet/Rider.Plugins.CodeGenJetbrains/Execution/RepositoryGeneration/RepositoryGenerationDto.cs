using System.Collections.Immutable;
using JetBrains.ReSharper.Psi;

namespace Rider.Plugins.CodeGenJetbrains.Execution.RepositoryGeneration;

public class RepositoryGenerationDto
{
    public ImmutableHashSet<string> Methods { get; set; } = [];
    public required string TableName { get; set; }
    public required string SchemaName { get; set; }
    public required IClass EntityType { get; set; }
    public IClass? EntityListType { get; set; }
    public ImmutableArray<DbProperty> Properties { get; set; } = [];
}

public record DbProperty(string Name, string Column, bool IsPrimaryKey);
