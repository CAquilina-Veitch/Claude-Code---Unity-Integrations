using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class ManualScriptAttacher : EditorWindow
{
    private GameObject targetObject;
    private List<string> availableScripts = new List<string>();
    private Vector2 scrollPosition;

    [MenuItem("GameObject/Attach Claude Script", false, 1)]
    static void AttachScriptFromContext()
    {
        if (Selection.activeGameObject != null)
        {
            ManualScriptAttacher window = GetWindow<ManualScriptAttacher>("Attach Claude Script");
            window.targetObject = Selection.activeGameObject;
            window.RefreshScriptList();
            window.Show();
        }
    }

    [MenuItem("Window/Claude Script Attacher")]
    static void OpenWindow()
    {
        ManualScriptAttacher window = GetWindow<ManualScriptAttacher>("Attach Claude Script");
        window.targetObject = Selection.activeGameObject;
        window.RefreshScriptList();
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Claude Script Attacher", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        // Object selection
        targetObject = (GameObject)EditorGUILayout.ObjectField("Target GameObject:", targetObject, typeof(GameObject), true);
        
        if (targetObject == null)
        {
            EditorGUILayout.HelpBox("Select a GameObject to attach scripts to", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();
        
        if (GUILayout.Button("Refresh Script List"))
        {
            RefreshScriptList();
        }
        
        EditorGUILayout.Space();
        
        GUILayout.Label($"Available Scripts for '{targetObject.name}':", EditorStyles.boldLabel);
        
        if (availableScripts.Count == 0)
        {
            EditorGUILayout.HelpBox("No MonoBehaviour scripts found in the project", MessageType.Info);
            return;
        }
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
        
        foreach (string scriptPath in availableScripts)
        {
            string scriptName = Path.GetFileNameWithoutExtension(scriptPath);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(scriptName);
            
            if (GUILayout.Button("Attach", GUILayout.Width(60)))
            {
                AttachScript(scriptPath);
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndScrollView();
    }

    void RefreshScriptList()
    {
        availableScripts.Clear();
        
        // Find all C# scripts in the project
        string[] scriptFiles = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
        
        foreach (string scriptPath in scriptFiles)
        {
            // Skip editor scripts
            if (scriptPath.Contains("Editor") && !scriptPath.Contains("Scripts"))
                continue;
                
            // Check if it's a MonoBehaviour script
            if (IsMonoBehaviourScript(scriptPath))
            {
                availableScripts.Add(scriptPath);
            }
        }
    }

    bool IsMonoBehaviourScript(string scriptPath)
    {
        try
        {
            string content = File.ReadAllText(scriptPath);
            return content.Contains(": MonoBehaviour") && content.Contains("class ");
        }
        catch
        {
            return false;
        }
    }

    void AttachScript(string scriptPath)
    {
        try
        {
            string className = Path.GetFileNameWithoutExtension(scriptPath);
            
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
                EditorUtility.DisplayDialog("Error", $"Could not find MonoBehaviour type '{className}'. Make sure the script has compiled successfully.", "OK");
                return;
            }

            // Check if component already exists
            if (targetObject.GetComponent(scriptType) != null)
            {
                EditorUtility.DisplayDialog("Info", $"Component '{className}' is already attached to '{targetObject.name}'", "OK");
                return;
            }

            // Add the component
            targetObject.AddComponent(scriptType);
            
            Debug.Log($"Successfully attached '{className}' to '{targetObject.name}'");
            
            // Mark the object as dirty to ensure changes are saved
            EditorUtility.SetDirty(targetObject);
            
            EditorUtility.DisplayDialog("Success", $"Successfully attached '{className}' to '{targetObject.name}'", "OK");
        }
        catch (System.Exception ex)
        {
            EditorUtility.DisplayDialog("Error", $"Error attaching script: {ex.Message}", "OK");
        }
    }
}