package com.jetbrains.rider.plugins.codegenjetbrains.actions

import com.intellij.icons.AllIcons
import com.intellij.openapi.actionSystem.AnAction
import com.intellij.openapi.actionSystem.AnActionEvent
import com.intellij.openapi.actionSystem.DefaultActionGroup

class CodeGenActionGroup : DefaultActionGroup(
    "Code Generation",
    "Description of custom actions group",
    AllIcons.Diff.MagicResolve
){
    init {
        // Устанавливаем группу как popup
        templatePresentation.isPopupGroup = true
    }

    override fun getChildren(p0: AnActionEvent?): Array<out AnAction> {
        return arrayOf(CreateTestAction(), GenerateRepositoryAction())
    }
}
