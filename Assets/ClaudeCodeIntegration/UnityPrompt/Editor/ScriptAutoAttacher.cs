using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

[InitializeOnLoad]
public class ScriptAutoAttacher
{
    static ScriptAutoAttacher()
    {
        EditorApplication.projectChanged += OnProjectChanged;
    }

    static void OnProjectChanged()
    {
        CheckForAutoAttachScripts();
    }

    static void CheckForAutoAttachScripts()
    {
        // Find all C# scripts in Assets/ folder (including subfolders)
        string assetsPath = Application.dataPath;
        if (!Directory.Exists(assetsPath))
        {
            return;
        }

        string[] scriptFiles = Directory.GetFiles(assetsPath, "*.cs", SearchOption.AllDirectories);
        
        foreach (string scriptFile in scriptFiles)
        {
            ProcessScriptForAutoAttach(scriptFile);
        }
    }

    static void ProcessScriptForAutoAttach(string scriptPath)
    {
        try
        {
            string content = File.ReadAllText(scriptPath);
            
            // Look for the auto-attach comment
            Match match = Regex.Match(content, @"//\s*UNITY_AUTO_ATTACH:\s*(.+)", RegexOptions.IgnoreCase);
            if (!match.Success) return;

            string targetObjectName = match.Groups[1].Value.Trim();
            
            // Find the GameObject
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            GameObject targetObject = null;
            
            foreach (GameObject obj in allObjects)
            {
                if (obj.name == targetObjectName)
                {
                    targetObject = obj;
                    break;
                }
            }

            if (targetObject == null)
            {
                Debug.LogWarning($"ScriptAutoAttacher: Could not find GameObject '{targetObjectName}' for script {Path.GetFileName(scriptPath)}");
                return;
            }

            // Get the script class name (assume it matches the filename)
            string className = Path.GetFileNameWithoutExtension(scriptPath);
            
            // Wait for compilation and then attach
            EditorApplication.delayCall += () => AttachScriptToObject(targetObject, className, scriptPath);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ScriptAutoAttacher error processing {scriptPath}: {ex.Message}");
        }
    }

    static void AttachScriptToObject(GameObject targetObject, string className, string scriptPath)
    {
        try
        {
            // Try to find the MonoBehaviour type
            System.Type scriptType = null;
            
            // Look through all assemblies for the type
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                scriptType = assembly.GetType(className);
                if (scriptType != null && scriptType.IsSubclassOf(typeof(MonoBehaviour)))
                    break;
            }

            if (scriptType == null)
            {
                Debug.LogWarning($"ScriptAutoAttacher: Could not find MonoBehaviour type '{className}'. Make sure the script has compiled successfully.");
                return;
            }

            // Check if component already exists
            if (targetObject.GetComponent(scriptType) != null)
            {
                Debug.Log($"ScriptAutoAttacher: Component '{className}' already attached to '{targetObject.name}'");
                return;
            }

            // Add the component
            targetObject.AddComponent(scriptType);
            
            // Remove the auto-attach comment so we don't process it again
            RemoveAutoAttachComment(scriptPath);
            
            Debug.Log($"ScriptAutoAttacher: Successfully attached '{className}' to '{targetObject.name}'");
            
            // Mark the object as dirty to ensure changes are saved
            EditorUtility.SetDirty(targetObject);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ScriptAutoAttacher error attaching script: {ex.Message}");
        }
    }

    static void RemoveAutoAttachComment(string scriptPath)
    {
        try
        {
            string content = File.ReadAllText(scriptPath);
            
            // Remove the auto-attach comment line
            content = Regex.Replace(content, @"//\s*UNITY_AUTO_ATTACH:.*\r?\n?", "", RegexOptions.IgnoreCase);
            
            File.WriteAllText(scriptPath, content);
            AssetDatabase.Refresh();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ScriptAutoAttacher error removing comment: {ex.Message}");
        }
    }
}