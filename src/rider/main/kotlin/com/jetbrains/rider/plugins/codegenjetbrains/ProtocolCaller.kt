package com.jetbrains.rider.plugins.codegenjetbrains

import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.application.runReadAction
import com.intellij.openapi.components.Service
import com.intellij.openapi.components.service
import com.intellij.openapi.project.Project
import com.jetbrains.rd.framework.impl.RdCall
import com.jetbrains.rd.ui.icons.ProtocolIconRegistryService
import com.jetbrains.rider.plugins.codegenjetbrains.model.RdCallRequest
import com.jetbrains.rider.plugins.codegenjetbrains.model.rdCodeGenJetbrainsModel
import com.jetbrains.rider.projectView.solution
import javax.swing.Icon

@Service(Service.Level.PROJECT)
class ProtocolCaller(private val project: Project) {
    suspend fun doCall(input: String): Int {
        val model = project.solution.rdCodeGenJetbrainsModel
        val request = RdCallRequest(input)
        val response = model.myCall.startSuspending(request)
        return response.myResult
    }

    suspend fun doIconCall(): Icon {
        val model = project.solution.rdCodeGenJetbrainsModel
        val response = model.myIconCall.startSuspending(Unit)
        return ApplicationManager.getApplication().service<ProtocolIconRegistryService>().createIcon(response)
    }
}
