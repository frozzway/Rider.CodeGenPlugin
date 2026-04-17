package com.jetbrains.rider.plugins.codegenjetbrains

import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.project.Project
import com.intellij.openapi.startup.ProjectActivity
import com.intellij.openapi.vcs.ProjectLevelVcsManager
import com.intellij.openapi.vcs.changes.VcsDirtyScopeManager
import com.intellij.openapi.vfs.LocalFileSystem
import com.jetbrains.rd.framework.impl.RdCall
import com.jetbrains.rd.framework.impl.RdTask
import com.jetbrains.rider.plugins.codegenjetbrains.model.*
import com.jetbrains.rider.projectView.solution
import java.io.File

class VcsIntegrationListener : ProjectActivity {

    override suspend fun execute(project: Project) {
        val model = project.solution.rdCodeGenJetbrainsModel

        // Подписываемся на вызов из C#
        (model.addToVcs as? RdCall<String, Boolean>)?.set { _, filePath ->
            val task = RdTask<Boolean>()

            ApplicationManager.getApplication().executeOnPooledThread {
                var success = false
                try {
                    val virtualFile = LocalFileSystem.getInstance().refreshAndFindFileByIoFile(File(filePath))

                    if (virtualFile != null) {
                        val vcsManager = ProjectLevelVcsManager.getInstance(project)
                        val vcs = vcsManager.getVcsFor(virtualFile)
                        val checkinEnvironment = vcs?.checkinEnvironment

                        if (checkinEnvironment != null) {
                            checkinEnvironment.scheduleUnversionedFilesForAddition(listOf(virtualFile))
                            VcsDirtyScopeManager.getInstance(project).fileDirty(virtualFile)
                            success = true
                        }
                    }
                } catch (e: Exception) {
                    throw e
                } finally {
                    // Сообщаем протоколу, что вызов завершен, и передаем результат в C#
                    task.set(success)
                }
            }

            // Возвращаем объект задачи сразу же
            return@set task
        }
    }
}
