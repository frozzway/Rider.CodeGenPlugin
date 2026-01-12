package com.jetbrains.rider.plugins.codegenjetbrains

import com.intellij.database.dataSource.LocalDataSource
import com.intellij.database.model.DasColumn
import com.intellij.database.model.DasNamespace
import com.intellij.database.model.DasObject
import com.intellij.database.model.DasTable
import com.intellij.database.model.ObjectKind
import com.intellij.database.psi.DbDataSource
import com.intellij.database.psi.DbPsiFacade
import com.intellij.database.util.DasUtil
import com.intellij.openapi.application.runReadAction
import com.intellij.openapi.project.Project
import com.intellij.openapi.startup.ProjectActivity
import com.jetbrains.rd.framework.impl.RdCall
import com.jetbrains.rider.plugins.codegenjetbrains.model.*
import com.jetbrains.rider.projectView.solution
import java.util.UUID
import java.util.concurrent.ConcurrentHashMap

class DatabaseSchemaListener : ProjectActivity {

    // Кэш объектов: Map<GeneratedUUID, DasObject>
    private val objectCache = ConcurrentHashMap<String, DasObject>()

    override suspend fun execute(project: Project) {
        val model = project.solution.rdCodeGenJetbrainsModel

        // 1. Метод получения источников данных
        (model.getDataSources as? RdCall<Unit, List<DataSourceDto>>)?.set { _ ->
            runReadAction {
                objectCache.clear()
                val facade = DbPsiFacade.getInstance(project)
                facade.dataSources.map { ds ->
                    // uniqueId - это стабильный ID источника данных
                    DataSourceDto(ds.uniqueId, ds.name)
                }
            }
        }

        // 2. Метод получения схем (Databases/Schemas)
        (model.getStructure as? RdCall<String, List<DbStructureItem>>)?.set { dsId ->
            runReadAction {
                val dataSource = findDataSource(project, dsId) ?: return@runReadAction emptyList()
                val dasModel = dataSource.model

                val resultList = mutableListOf<DbStructureItem>()
                val defaultNamespace = dasModel.currentRootNamespace

                // Рекурсивная функция, которая заполняет плоский список resultList
                fun collectNodes(obj: DasObject, parentId: String?) {
                    val kind = obj.kind
                    // Фильтруем только Базы и Схемы
                    if (kind == ObjectKind.DATABASE || kind == ObjectKind.SCHEMA) {

                        val id = cacheObject(obj)

                        val url = (dataSource.delegate as LocalDataSource).url
                        val schemaFromUrl = if (url != null && url.contains("currentSchema=")) {
                            url.substringAfter("currentSchema=").substringBefore("&")
                        } else "public"

                        val isDefaultObj = if (obj == defaultNamespace) {
                            true
                        } else if (kind == ObjectKind.SCHEMA && obj.dasParent == defaultNamespace) {
                            // Сравниваем с тем, что нашли в URL, или с public по умолчанию
                            obj.name.equals(schemaFromUrl, ignoreCase = true)
                        } else {
                            false
                        }

                        resultList.add(DbStructureItem(
                            id = id,
                            parentId = parentId,
                            name = obj.name,
                            kind = kind.toString(),
                            isDefault = isDefaultObj
                        ))

                        // Идем вглубь (например, внутри Database ищем Schema)
                        // Ищем детей типа SCHEMA (обычно базы содержат схемы)
                        obj.getDasChildren(ObjectKind.SCHEMA).forEach { child ->
                            collectNodes(child, id) // Передаем текущий id как parentId
                        }
                    }
                }

                // Запускаем сбор с корней модели
                dasModel.modelRoots.forEach { root ->
                    collectNodes(root, null)
                }

                resultList
            }
        }

        // 3. Метод получения таблиц (schema.table)
        (model.getTables as? RdCall<String, List<DbTableDto>>)?.set { containerId ->
            runReadAction {
                val container = objectCache[containerId] ?: return@runReadAction emptyList()

                val tables = if (container is DasNamespace) {
                    container.getDasChildren(ObjectKind.TABLE)
                } else {
                    emptyList()
                }

                tables.filterIsInstance<DasTable>()
                    .map { table ->
                        val tableId = cacheObject(table)
                        val displayName = table.name

                        DbTableDto(tableId, displayName)
                    }
                    .toList()
                    .sortedBy { it.displayName }
            }
        }

        // 4. Метод получения колонок
        (model.getColumns as? RdCall<String, DbTableColumnsDto>)?.set { tableId ->
            runReadAction {
                val table = objectCache[tableId] as? DasTable ?: return@runReadAction DbTableColumnsDto(emptyList())

                val columns = table.getDasChildren(ObjectKind.COLUMN)
                    .filterIsInstance<DasColumn>()
                    .map { col ->
                        DbColumnDto(col.name, col.dasType.toDataType().typeName, DasUtil.isPrimary(col))
                    }
                    .toList()

                DbTableColumnsDto(columns)
            }
        }
    }

    private fun cacheObject(obj: DasObject): String {
        val id = UUID.randomUUID().toString()
        objectCache[id] = obj
        return id
    }

    // Хелпер для поиска Data Source по ID
    private fun findDataSource(project: Project, id: String): DbDataSource?
        = DbPsiFacade.getInstance(project).dataSources.find { it.uniqueId == id }
}
