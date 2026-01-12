using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.DataFlow;
using JetBrains.IDE.UI.Extensions;
using JetBrains.Lifetimes;
using Rider.Plugins.CodeGenJetbrains.Execution.RepositoryGeneration;
using Rider.Plugins.CodeGenJetbrains.Extensions;
using Rider.Plugins.CodeGenJetbrains.Model;

namespace Rider.Plugins.CodeGenJetbrains.UI.Components;

public class DataSourceInput
{
    public InputField<DataSourceDto> SourceInput { get; }
    public InputField<DbObject> DatabaseInput { get; }
    public InputField<DbObject> SchemaInput { get; }
    public InputField<DbObject> TableInput { get; }
    public Lifetime Lifetime { get; }

    private readonly RdDatabaseCaller _rdDatabaseCaller;

    public DataSourceInput(Lifetime lifetime, RdDatabaseCaller rdDatabaseCaller)
    {
        _rdDatabaseCaller = rdDatabaseCaller;
        Lifetime = lifetime;
        SourceInput = InputFieldFactory.CreateDropdown("Source:", rdDatabaseCaller.GetDataSources(), lifetime,
            presentation: (_, element, _) => element.Name.GetBeLabel());
        DatabaseInput = InputFieldFactory.CreateDropdown("Database:", new DbObject[] {}, lifetime);
        SchemaInput = InputFieldFactory.CreateDropdown("Schema:", new DbObject[] {}, lifetime);
        TableInput = InputFieldFactory.CreateDropdown("Table:", new DbObject[] {}, lifetime);
        SetAdvises();
    }

    private void SetAdvises()
    {
        IReadOnlyList<DbStructureItem> sourceStructure = null!;

        SourceInput.CurrentValue.Change.Advise_NewNotNull(Lifetime, args =>
        {
            sourceStructure = _rdDatabaseCaller.GetSourceStructure(args.New.Id);
            var databases = sourceStructure.Where(IsDatabase).Select(ToDbObject).ToImmutableArray();
            ReplaceInputItems(DatabaseInput, databases);
            var schemas = sourceStructure.Where(IsSchema).Select(ToDbObject).ToImmutableArray();
            ReplaceInputItems(SchemaInput, schemas);
        });

        DatabaseInput.CurrentValue.Change.Advise_NewNotNull(Lifetime, args =>
        {
            var schemas = sourceStructure.Where(IsSchema).Where(i => i.ParentId == args.New.Id)
                .Select(ToDbObject).ToImmutableArray();
            ReplaceInputItems(SchemaInput, schemas);
        });

        SchemaInput.CurrentValue.Change.Advise_HasNew(Lifetime, args =>
        {
            var newItem = args.GetNewOrNull();
            if (newItem is null)
            {
                TableInput.Values!.ReplaceItems([]);
                return;
            }
            var tables = _rdDatabaseCaller.GetTables(args.New.Id);
            TableInput.Values!.ReplaceItems(tables.Select(ToDbObject));
        });
    }

    private static void ReplaceInputItems(InputField<DbObject> input, ImmutableArray<DbObject> newItems)
    {
        input.Values!.ReplaceItems(newItems);
        var defaultItem = newItems.FirstOrDefault(i => i.IsDefault);
        if (defaultItem != null)
            input.CurrentValue.SetValue(defaultItem);
    }

    private static DbObject ToDbObject(DbStructureItem item) => new(item.Id, item.Name, item.IsDefault, false);
    private static DbObject ToDbObject(DbTableDto item) => new(item.Id, item.DisplayName, false, false);
    private static bool IsDatabase(DbStructureItem item) => item.Kind == "database";
    private static bool IsSchema(DbStructureItem item) => item.Kind == "schema";
}
