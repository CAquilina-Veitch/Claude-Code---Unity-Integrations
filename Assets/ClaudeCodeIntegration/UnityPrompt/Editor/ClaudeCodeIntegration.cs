using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;

public class ClaudeCodeIntegration : EditorWindow
{
    private string promptText = "";
    private GameObject selectedObject;
    private Vector2 scrollPosition;
    private bool isProcessing = false;
    private string lastResult = "";

    [MenuItem("GameObject/Ask Claude Code", false, 0)]
    static void AskClaudeCodeFromContext()
    {
        if (Selection.activeGameObject != null)
        {
            OpenWindow(Selection.activeGameObject);
        }
    }

    [MenuItem("Window/Claude Code Prompt")]
    static void OpenWindow()
    {
        OpenWindow(Selection.activeGameObject);
    }

    static void OpenWindow(GameObject target)
    {
        ClaudeCodeIntegration window = GetWindow<ClaudeCodeIntegration>("Claude Code Prompt");
        window.selectedObject = target;
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Claude Code Integration", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        // Object selection
        selectedObject = (GameObject)EditorGUILayout.ObjectField("Target GameObject:", selectedObject, typeof(GameObject), true);
        
        if (selectedObject == null)
        {
            EditorGUILayout.HelpBox("Select a GameObject to modify", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();
        
        // Prompt input
        GUILayout.Label("Describe what you want this object to do:", EditorStyles.label);
        promptText = EditorGUILayout.TextArea(promptText, GUILayout.Height(80));
        
        EditorGUILayout.Space();
        
        // Process button
        GUI.enabled = !isProcessing && !string.IsNullOrEmpty(promptText);
        if (GUILayout.Button(isProcessing ? "Processing..." : "Ask Claude Code", GUILayout.Height(30)))
        {
            ProcessPrompt();
        }
        GUI.enabled = true;
        
        EditorGUILayout.Space();
        
        // Results
        if (!string.IsNullOrEmpty(lastResult))
        {
            GUILayout.Label("Claude Code Response:", EditorStyles.boldLabel);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            EditorGUILayout.TextArea(lastResult, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndScrollView();
        }
    }

    void ProcessPrompt()
    {
        if (selectedObject == null || string.IsNullOrEmpty(promptText))
            return;

        isProcessing = true;
        lastResult = "";

        try
        {
            // Generate context about the GameObject
            string context = GenerateGameObjectContext(selectedObject);
            
            // Add Unity integration instructions
            string unityInstructions = GenerateUnityInstructions(selectedObject);
            
            // Create temporary files for communication
            string tempDir = Path.Combine(Application.temporaryCachePath, "ClaudeCode");
            Directory.CreateDirectory(tempDir);
            
            string contextFile = Path.Combine(tempDir, "context.txt");
            string promptFile = Path.Combine(tempDir, "prompt.txt");
            
            File.WriteAllText(contextFile, context);
            File.WriteAllText(promptFile, $"Unity GameObject Request: {promptText}\n\nContext:\n{context}\n\n{unityInstructions}");
            
            // Call Claude Code CLI - try multiple possible locations
            string claudeCodePath = FindClaudeCodeExecutable();
            if (string.IsNullOrEmpty(claudeCodePath))
            {
                lastResult = "Error: Claude Code not found. Please ensure 'claude' is installed and in your PATH, or update the script with the correct path.";
                return;
            }

            // Read the prompt content to pass to claude
            string fullPrompt = File.ReadAllText(promptFile);
            
            ProcessStartInfo startInfo;
            
            if (claudeCodePath == "wsl")
            {
                // Create a temporary bash script to show prompt and start claude
                string tempScript = Path.Combine(Application.temporaryCachePath, "claude_launch.sh");
                string wslWorkingDir = ConvertToWSLPath(Directory.GetParent(Application.dataPath).FullName);
                string wslTempScript = ConvertToWSLPath(tempScript);
                
                string scriptContent = $@"#!/bin/bash
cd '{wslWorkingDir}'
echo ""===========================================""
echo ""Unity GameObject Request from Claude Code""
echo ""===========================================""
echo """"
cat << 'EOF'
{fullPrompt}
EOF
echo """"
echo ""===========================================""
echo ""Loading Unity Command Functions...""
echo ""===========================================""
source ./unity_commands.sh
echo """"
echo ""===========================================""
echo ""Starting Claude Code...""
echo ""===========================================""
echo """"
claude --dangerously-skip-permissions '{fullPrompt.Replace("'", "'\\''")}'";
                
                File.WriteAllText(tempScript, scriptContent);
                
                startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c start \"Claude Code - Unity Integration\" cmd /k \"wsl -d Ubuntu bash '{wslTempScript}'\"",
                    UseShellExecute = true,
                    CreateNoWindow = false
                };
            }
            else
            {
                // Create a temporary batch script for Windows
                string tempBatch = Path.Combine(Application.temporaryCachePath, "claude_launch.bat");
                string workingDir = Directory.GetParent(Application.dataPath).FullName;
                
                string batchContent = $@"@echo off
cd /d ""{workingDir}""
echo ==========================================
echo Unity GameObject Request from Claude Code
echo ==========================================
echo.
echo {fullPrompt.Replace("\"", "\"\"")}
echo.
echo ==========================================
echo Starting Claude Code...
echo ==========================================
echo.
claude ""{fullPrompt.Replace("\"", "\"\"")}""
pause
";
                
                File.WriteAllText(tempBatch, batchContent);
                
                startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c start \"Claude Code - Unity Integration\" \"{tempBatch}\"",
                    UseShellExecute = true,
                    CreateNoWindow = false
                };
            }

            Process.Start(startInfo);
            lastResult = "Claude Code opened in new terminal window with your request and GameObject context.";
            
            // Cleanup
            try
            {
                File.Delete(contextFile);
                File.Delete(promptFile);
            }
            catch { }
        }
        catch (System.Exception ex)
        {
            lastResult = $"Error calling Claude Code: {ex.Message}";
        }
        finally
        {
            isProcessing = false;
            Repaint();
        }
    }

    string GenerateGameObjectContext(GameObject obj)
    {
        StringBuilder context = new StringBuilder();
        
        context.AppendLine($"GameObject: {obj.name}");
        context.AppendLine($"Tag: {obj.tag}");
        context.AppendLine($"Layer: {LayerMask.LayerToName(obj.layer)}");
        context.AppendLine($"Active: {obj.activeInHierarchy}");
        context.AppendLine();
        
        // Transform info
        Transform t = obj.transform;
        context.AppendLine($"Transform:");
        context.AppendLine($"  Position: {t.position}");
        context.AppendLine($"  Rotation: {t.eulerAngles}");
        context.AppendLine($"  Scale: {t.localScale}");
        context.AppendLine();
        
        // Components
        Component[] components = obj.GetComponents<Component>();
        context.AppendLine($"Components ({components.Length}):");
        
        foreach (Component component in components)
        {
            if (component == null) continue;
            
            context.AppendLine($"  - {component.GetType().Name}");
            
            // Add some component-specific details
            if (component is Renderer renderer)
            {
                context.AppendLine($"    Material: {(renderer.material ? renderer.material.name : "None")}");
            }
            else if (component is Collider collider)
            {
                context.AppendLine($"    IsTrigger: {collider.isTrigger}");
            }
            else if (component is Rigidbody rb)
            {
                context.AppendLine($"    Mass: {rb.mass}");
                context.AppendLine($"    UseGravity: {rb.useGravity}");
            }
        }
        
        // Children
        if (obj.transform.childCount > 0)
        {
            context.AppendLine();
            context.AppendLine($"Children ({obj.transform.childCount}):");
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                context.AppendLine($"  - {obj.transform.GetChild(i).name}");
            }
        }
        
        // Parent
        if (obj.transform.parent != null)
        {
            context.AppendLine();
            context.AppendLine($"Parent: {obj.transform.parent.name}");
        }
        
        return context.ToString();
    }

    string GenerateUnityInstructions(GameObject obj)
    {
        StringBuilder instructions = new StringBuilder();
        
        instructions.AppendLine("=== UNITY INTEGRATION INSTRUCTIONS ===");
        instructions.AppendLine();
        instructions.AppendLine("You now have DIRECT CONTROL over Unity GameObjects through command functions!");
        instructions.AppendLine();
        instructions.AppendLine("AVAILABLE UNITY COMMANDS:");
        instructions.AppendLine();
        instructions.AppendLine("1. **unity_list_objects** - See all GameObjects in the scene");
        instructions.AppendLine("2. **unity_get_scene_info** - Get detailed info about all objects and components");
        instructions.AppendLine("3. **unity_add_component <object> <script>** - Add any component to any GameObject");
        instructions.AppendLine("4. **unity_remove_component <object> <component>** - Remove components");
        instructions.AppendLine("5. **unity_set_property <object> <component> <property> <value>** - Modify component properties");
        instructions.AppendLine("6. **unity_create_object <name> [x,y,z]** - Create new GameObjects");
        instructions.AppendLine("7. **unity_delete_object <name>** - Delete GameObjects");
        instructions.AppendLine();
        instructions.AppendLine("WORKFLOW:");
        instructions.AppendLine("1. Use unity_list_objects to see what's available");
        instructions.AppendLine("2. Create scripts in Assets/Scripts/ if needed");
        instructions.AppendLine("3. Use unity_add_component to attach your scripts");
        instructions.AppendLine("4. Use unity_set_property to configure component properties");
        instructions.AppendLine();
        instructions.AppendLine("EXAMPLES:");
        instructions.AppendLine($"  unity_add_component \"{obj.name}\" \"SpinObject\"");
        instructions.AppendLine($"  unity_set_property \"{obj.name}\" \"SpinObject\" \"speed\" \"2.0\"");
        instructions.AppendLine("  unity_create_object \"NewCube\" \"0,5,0\"");
        instructions.AppendLine();
        instructions.AppendLine($"**Target GameObject**: {obj.name}");
        instructions.AppendLine($"**Current Components**: {string.Join(", ", System.Array.ConvertAll(obj.GetComponents<Component>(), c => c?.GetType().Name ?? "null"))}");
        instructions.AppendLine();
        instructions.AppendLine("You can now modify Unity scenes in real-time! Type 'unity_help' for more commands.");
        instructions.AppendLine("=== END UNITY INSTRUCTIONS ===");
        
        return instructions.ToString();
    }

    string FindClaudeCodeExecutable()
    {
        // First try WSL Ubuntu (most common for your setup)
        try
        {
            ProcessStartInfo testInfo = new ProcessStartInfo
            {
                FileName = "wsl",
                Arguments = "-d Ubuntu claude --version",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (Process testProcess = Process.Start(testInfo))
            {
                testProcess.WaitForExit(5000); // 5 second timeout
                if (testProcess.ExitCode == 0)
                {
                    return "wsl"; // We'll use WSL to call claude-code
                }
            }
        }
        catch { }

        // Fallback to other common locations
        string[] possiblePaths = {
            "claude-code", // Try PATH first
            "claude-code.exe",
            @"C:\Users\" + System.Environment.UserName + @"\AppData\Local\Programs\claude-code\claude-code.exe",
            @"C:\Program Files\claude-code\claude-code.exe",
            @"C:\Program Files (x86)\claude-code\claude-code.exe"
        };

        foreach (string path in possiblePaths)
        {
            try
            {
                // For simple command names, try to execute with --version to test
                if (path == "claude-code" || path == "claude-code.exe")
                {
                    ProcessStartInfo testInfo = new ProcessStartInfo
                    {
                        FileName = path,
                        Arguments = "--version",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    using (Process testProcess = Process.Start(testInfo))
                    {
                        testProcess.WaitForExit(3000); // 3 second timeout
                        if (testProcess.ExitCode == 0)
                        {
                            return path;
                        }
                    }
                }
                // For full paths, check if file exists
                else if (File.Exists(path))
                {
                    return path;
                }
            }
            catch
            {
                // Continue trying other paths
                continue;
            }
        }

        return null; // Not found
    }

    string ConvertToWSLPath(string windowsPath)
    {
        // Convert Windows path to WSL path
        // Example: C:\Users\... becomes /mnt/c/Users/...
        if (windowsPath.Length >= 3 && windowsPath[1] == ':')
        {
            char driveLetter = char.ToLower(windowsPath[0]);
            string restOfPath = windowsPath.Substring(2).Replace('\\', '/');
            return $"/mnt/{driveLetter}{restOfPath}";
        }
        return windowsPath; // Return as-is if not a typical Windows path
    }
}