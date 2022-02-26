using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguageTextTemplatesMono : MonoBehaviour
{
    public LanguageTextTemplatesScriptable m_target;
    public bool m_createTemplateAtRootOnAwake;

    [ContextMenu("Create template at root")]
    public void CreateTemplateFileAtProjectRoot()
    {
        m_target.m_value.CreateCustomFilesAtRoot();
    }
    public void Awake()
    {
        if(m_createTemplateAtRootOnAwake)
            CreateTemplateFileAtProjectRoot();
    }
}
