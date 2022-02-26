using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class UI_TextMultiLang : MonoBehaviour
{
    public LanguageTextTemplatesScriptable m_scriptable;

    public Text m_targetText;
    //Change this by using a abstract directory target
    public string m_rootPath;
    public string m_fieldKey = "GDPR";
    public string m_languageAlias = "EN";
    public void Start()
    {
        //Change this by an abstract directory target.
        m_scriptable.CreateFileBackupAtCurrentDirectory();
        Refresh();
    }
    public void OnEnable()
    {
        Refresh();
    }

    void Refresh()
    {
        UserLanguagePreference.Instance.GetLanguageAliasToUse(out m_languageAlias);
        m_scriptable.GetTextTemplatesCollection(out var collection);
        collection.GetText(in m_rootPath, in m_fieldKey, in m_languageAlias,
            out bool found, out string text);
        if (found) { 
            if(m_targetText!=null)
                m_targetText.text = text;
        }
    }

}
