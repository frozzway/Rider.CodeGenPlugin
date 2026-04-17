package model.rider

import com.jetbrains.rd.generator.nova.*
import com.jetbrains.rd.generator.nova.PredefinedType.*
import com.jetbrains.rd.generator.nova.csharp.CSharp50Generator
import com.jetbrains.rd.generator.nova.kotlin.Kotlin11Generator
import com.jetbrains.rider.model.nova.ide.ShellModel
import com.jetbrains.rider.model.nova.ide.SolutionModel

@Suppress("unused")
object RdCodeGenJetbrainsModel : Ext(SolutionModel.Solution) {
    private val RdCallRequest = structdef {
        field("myField", string)
    }

    private val RdCallResponse = structdef {
        field("myResult", int)
    }

    // --- Структуры данных (DTO) ---

    // 1. DTO источника данных
    private val DataSourceDto = structdef {
        field("id", string)   // uniqueId источника
        field("name", string)
    }

    // 2. DTO структурной единицы (База данных или Схема)
    // Делаем структуру рекурсивной для поддержки иерархии DB -> Schema
    private val DbStructureItem = structdef {
        field("id", string)
        field("parentId", string.nullable) // <-- Ссылка на родителя (null для корня)
        field("name", string)
        field("kind", string)
        field("isDefault", bool)
    }

    // 3. DTO таблицы
    private val DbTableDto = structdef {
        field("id", string)          // Временный UUID таблицы
        field("displayName", string) // Имя для UI ("schema.table" или просто "table")
    }

    // 4. DTO колонки
    private val DbColumnDto = structdef {
        field("name", string)
        field("type", string)
        field("isPrimaryKey", bool)
    }

    private val DbTableColumnsDto = structdef {
        field("columns", immutableList(DbColumnDto))
    }

    init {
        setting(Kotlin11Generator.Namespace, "com.jetbrains.rider.plugins.codegenjetbrains.model")
        setting(CSharp50Generator.Namespace, "Rider.Plugins.CodeGenJetbrains.Model")

        call("myCall", RdCallRequest, RdCallResponse)
            .doc("This is an example protocol call.")

        call("myIconCall", PredefinedType.void, ShellModel.IconModel)
            .doc("This is an example protocol call for getting a backend icon.")

        // 1. Получить список источников данных
        call("getDataSources", PredefinedType.void, immutableList(DataSourceDto))

        // 2. Получить список схем/баз данных для конкретного источника
        // Вход: ID источника, Выход: Список имен схем (напр. "public", "dbo", "information_schema")
        call("getStructure", string, immutableList(DbStructureItem))

        // 3. Получить список таблиц в указанной схеме
        // Вход: GetTablesRequest, Выход: Список строк вида "schema.table"
        call("getTables", string, immutableList(DbTableDto))

        // 4. Получить колонки таблицы
        // Вход: GetColumnsRequest, Выход: Список колонок с типами
        call("getColumns", string, DbTableColumnsDto)

        call("addToVcs", string, bool)
            .doc("Adds a newly created file to the VCS (e.g. Git) on the frontend side.")
    }
}
