package com.jetbrains.rider.plugins.codegenjetbrains.actions

import com.intellij.icons.AllIcons
import com.intellij.openapi.actionSystem.AnAction
import com.intellij.openapi.actionSystem.AnActionEvent
import com.intellij.openapi.actionSystem.DefaultActionGroup
import com.jetbrains.rider.plugins.codegenjetbrains.actions.testsgeneration.CreateTestAction
import com.jetbrains.rider.plugins.codegenjetbrains.actions.testsgeneration.GetManyTestAction
import com.jetbrains.rider.plugins.codegenjetbrains.actions.testsgeneration.GetGridTestAction
import com.jetbrains.rider.plugins.codegenjetbrains.actions.testsgeneration.UpdateTestAction

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
        return arrayOf(
          GenerateCommandAction(),
          CreateTestAction(),
          UpdateTestAction(),
          GetManyTestAction(),
          GetGridTestAction(),
          GenerateRepositoryAction())
    }
}
