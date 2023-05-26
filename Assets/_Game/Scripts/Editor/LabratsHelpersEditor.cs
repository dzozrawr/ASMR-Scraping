using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

//https://docs.unity3d.com/ScriptReference/MenuItem.html
public static class LabratsHelpersEditor
{
    [MenuItem("Labrats/ Open Application.persistentDataPath %#o")]
    public static void OpenSaveFileLocation()
    {
        var path =SaveSystem.filePath;

        if (Directory.Exists(path))
        {
            //can do both ways
            //System.Diagnostics.Process.Start("explorer.exe", "/select," + path.Replace(@"/", @"\") + @"\");
            //replace fs with bs for explorer pathing
            System.Diagnostics.Process.Start("explorer.exe", path.Replace(@"/", @"\") + @"\");
        }
    }

    [MenuItem("Labrats/ Delete Save File %#d")]
    public static void DeleteSaveFile()
    {
        // PlayerPrefs.DeleteAll();
        var fileName = SaveSystem.fileName;
        var path = Path.Combine(SaveSystem.filePath, fileName);

        if (File.Exists(path))
        {
            if (EditorUtility.DisplayDialog("Delete Save File", "Are you sure you want to delete the save file?", "Yeet", "OOF! I was wrong."))
            {
                Debug.Log("Deleted save file at: " + path);
                File.Delete(path);
            }
        }
    }
}
