using Eloi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "LanguageTextTemplates", menuName = "ScriptableObjects/Text Template/Languages", order = 1)]
public class LanguageTextTemplatesScriptable : ScriptableObject
{
    public TextTemplatesLanguageCollection m_value = new TextTemplatesLanguageCollection();
    public void GetTextTemplatesCollection( out ITextTemplatesLanguageCollectionFromRoot collection){
        collection = m_value;
    }
    public void CreateFileBackup(string path)
    {
        m_value.CreateCustomFilesAtRoot(path);
    }
    public void CreateFileBackupAtCurrentDirectory()
    {
        m_value.CreateCustomFilesAtRoot(Directory.GetCurrentDirectory());
    }
}


[System.Serializable]
public class TextTemplateFromRootFile : ITextTemplateFromRootFolder
{
    public TextAsset m_backupFile;
    public string m_subFilePath;

    public void GetText(in string rootFolderPath, out string text)
    {
        TextTemplateFromRootFile template = this;
        TextTemplateStorageUtility.GetText(rootFolderPath, in template, out text);
    }
}


[System.Serializable]
public class TextTemplatesLanguageCollection: ITextTemplatesLanguageCollectionFromRoot
{
    public List<TextKeyLanguageToValue> m_textTemplate = new List<TextKeyLanguageToValue>();
    public void GetText(in string rootPath, in string templateNameId, in string languageId, out bool found, out string textInTemplate)
    {
        GetText(in rootPath, in templateNameId, in languageId, out found, out ITextTemplateFromRootFolder template);
        if (found)
        {
            template.GetText(in rootPath, out textInTemplate);
        }
        else {
            textInTemplate = "";
        }
    }
     public void GetText(in string rootPath, in string templateNameId, in string languageId, out bool found,  out ITextTemplateFromRootFolder textInTemplate)
    {
        string tId = templateNameId ;
        string lId = languageId;
       IEnumerable<TextKeyLanguageToValue> results = m_textTemplate.Where(
            k => E_StringUtility.AreEquals(in tId, in k.m_keyId, true, true)
        && E_StringUtility.AreEquals(in lId, in k.m_languageId, true, true));

        if (results.Count() > 0)
        {

            found = true;
            textInTemplate = (ITextTemplateFromRootFolder)results.First();
        }
        else {
            found = false;
            textInTemplate = null;
        }

    }

    public void CreateCustomFilesAtRoot()
    {
        string root = Directory.GetCurrentDirectory();
        CreateCustomFilesAtRoot(root);
    }
    public void CreateCustomFilesAtRoot(string rootFolderPath)
    {
        for (int i = 0; i < m_textTemplate.Count; i++)
        {
            TextTemplateStorageUtility.CreateFileFromBackup(in rootFolderPath, false, in m_textTemplate[i].m_template);
        }
    }

   
}

[System.Serializable]
public class TextKeyLanguage {
    public string m_keyId = "";
    public string m_languageId="";
}

[System.Serializable]
public class TextKeyLanguageToValue : TextKeyLanguage , ITextTemplateFromRootFolder
{
    public TextTemplateFromRootFile m_template;

    public void GetText(in string rootFolderPath, out string text)
    {
        m_template.GetText(rootFolderPath, out text);
    }
}

public class TextTemplateFileRuntimePath : ITextTemplateFromRootFolder {
    public string m_folderRoot;
    public ITextTemplateFromRootFolder m_source;

    public void GetText(out string text)
    {
        TextTemplateStorageUtility.GetProjectPath(out string root);
        m_source.GetText(root, out text);
    }

    public void GetText(in string rootFolderPath, out string text)
    {
        m_source.GetText(rootFolderPath, out text);
    }
}


[System.Serializable]
public class TextTemplateBackup
{
    public TextAsset m_inAppBackupText;
}

//public interface ITextTemplatesLanguageCollection
//{
//    void GetText(in string templateNameId, in string languageId, out bool found, out string textInTemplate);
//    void GetText(in string templateNameId, in string languageId, out bool found, out ITextTemplateContent textInTemplate);
//}
public interface ITextTemplatesLanguageCollectionFromRoot
{
    void GetText(in string rootPath, in string templateNameId, in string languageId, out bool found, out string textInTemplate);
}

//public interface ITextTemplateContent
//{
//    void GetText(out string text);
//}
public interface ITextTemplateFromRootFolder
{
    void GetText(in string rootFolderPath, out string text);
}

public class TextTemplateStorageUtility{
    public static string m_projectRootPath;
    public static void GetProjectPath(out string rootPath){
        if(m_projectRootPath==null || m_projectRootPath.Length==0)
            m_projectRootPath = Directory.GetCurrentDirectory();
        rootPath = m_projectRootPath;
    }
    public static void GetFolderPathFor(in string rootPath, in TextTemplateFromRootFile template, out string fullPath)
    {
        CheckIfCustomRootOrProjectRoot(rootPath, out string rootToUse);
        Eloi.E_FilePathUnityUtility.MeltPathTogether(out fullPath, in rootToUse,  template.m_subFilePath );
       // E_FilePathUnityUtility.AllBackslash(in fullPath, out fullPath);
    }
    public static bool IsFileExist(in string rootPath, in TextTemplateFromRootFile template){

        CheckIfCustomRootOrProjectRoot(rootPath, out string rootToUse);
        GetFolderPathFor(in rootToUse, in template, out string fullpath);
        return File.Exists(fullpath);
    }

  
    
   
    public static void CreateFileFromBackup(in string rootPath, in bool overrideExisting, in TextTemplateFromRootFile template)
    {
        CheckIfCustomRootOrProjectRoot(rootPath, out string rootToUse);

        bool existFile = IsFileExist(in rootToUse, in template);
        if (!existFile || (existFile && overrideExisting))
        {
            GetFolderPathFor(in rootToUse, in template, out string fullpath);
            // Debug.Log(string.Join(" || ", rootPath, fullpath, Path.GetDirectoryName(fullpath)));
            Directory.CreateDirectory(Path.GetDirectoryName(fullpath));

            if (template.m_backupFile != null)
            {
                string contents = System.Text.Encoding.Default.GetString(template.m_backupFile.bytes);
                File.WriteAllText(fullpath, template.m_backupFile != null ? contents : "");
            }
            else
            {
                File.WriteAllText(fullpath,"");
            }
        }
    }

    private static void CheckIfCustomRootOrProjectRoot(string rootPath, out string rootToUse)
    {
        string rp = rootPath;
        if (Eloi.E_StringUtility.IsNullOrEmpty(rootPath))
            GetProjectPath(out rootToUse);
        else rootToUse = rootPath;
    }

    public static void ImportFromComputerFile(in string rootPath, in TextTemplateFromRootFile template, out string text)
    {
        CheckIfCustomRootOrProjectRoot(rootPath, out string rootToUse);
      
        GetFolderPathFor(in rootToUse, in template, out string fullpath);
       // Debug.Log(string.Join(" |A| ", rootToUse, fullpath ));
        if (File.Exists(fullpath))
            text = File.ReadAllText(fullpath);
        else ImportFromTextAssetUnityBack(in template, out text);
    }
    public static void ImportFromTextAssetUnityBack( in TextTemplateFromRootFile template, out string text) {

        if (template.m_backupFile != null)
            text = template.m_backupFile.text;
        else text = "";
        // Debug.Log(string.Join(" |B| ", text, ""));
    }

    public static void GetText(in string rootPath,  in TextTemplateFromRootFile template, out string text)
    {
        ImportFromComputerFile(in rootPath, in template, out text);
    }


}



