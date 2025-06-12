using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

[InitializeOnLoad]
public class CommandProcessor
{
    private static string commandsPath;
    private static string responsesPath;
    private static FileSystemWatcher watcher;
    private static Queue<string> commandQueue = new Queue<string>();

    static CommandProcessor()
    {
        commandsPath = Path.Combine(Application.dataPath, "ClaudeCodeIntegration", "Commands");
        responsesPath = Path.Combine(Application.dataPath, "ClaudeCodeIntegration", "Responses");
        
        // Ensure directories exist
        Directory.CreateDirectory(commandsPath);
        Directory.CreateDirectory(responsesPath);
        
        InitializeFileWatcher();
        EditorApplication.update += ProcessCommandQueue;
        
        Debug.Log("Claude Code CommandProcessor initialized");
    }

    static void InitializeFileWatcher()
    {
        try
        {
            watcher = new FileSystemWatcher(commandsPath, "*.cmd");
            watcher.Created += OnCommandFileCreated;
            watcher.EnableRaisingEvents = true;
            Debug.Log($"FileSystemWatcher initialized for: {commandsPath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to initialize FileSystemWatcher: {ex.Message}");
        }
    }

    static void OnCommandFileCreated(object sender, FileSystemEventArgs e)
    {
        lock (commandQueue)
        {
            commandQueue.Enqueue(e.FullPath);
        }
    }

    static void ProcessCommandQueue()
    {
        if (commandQueue.Count == 0) return;

        lock (commandQueue)
        {
            while (commandQueue.Count > 0)
            {
                string filePath = commandQueue.Dequeue();
                ProcessCommandFile(filePath);
            }
        }
    }

    static void ProcessCommandFile(string filePath)
    {
        try
        {
            // Wait a bit for file to be fully written
            System.Threading.Thread.Sleep(50);
            
            if (!File.Exists(filePath)) return;

            string command = File.ReadAllText(filePath).Trim();
            string response = ProcessCommand(command);
            
            // Write response
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string responsePath = Path.Combine(responsesPath, $"{fileName}.response");
            File.WriteAllText(responsePath, response);
            
            // Delete command file
            File.Delete(filePath);
            
            Debug.Log($"Processed command: {command}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error processing command file {filePath}: {ex.Message}");
            
            // Write error response
            try
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string responsePath = Path.Combine(responsesPath, $"{fileName}.response");
                File.WriteAllText(responsePath, $"ERROR: {ex.Message}");
                File.Delete(filePath);
            }
            catch { }
        }
    }

    static string ProcessCommand(string command)
    {
        string[] parts = command.Split(':');
        if (parts.Length == 0) return "ERROR: Empty command";

        string action = parts[0].ToUpper();

        try
        {
            switch (action)
            {
                case "ADD_COMPONENT":
                    return AddComponent(parts);
                    
                case "REMOVE_COMPONENT":
                    return RemoveComponent(parts);
                    
                case "SET_PROPERTY":
                    return SetProperty(parts);
                    
                case "CREATE_OBJECT":
                    return CreateObject(parts);
                    
                case "DELETE_OBJECT":
                    return DeleteObject(parts);
                    
                case "GET_SCENE_INFO":
                    return GetSceneInfo();
                    
                case "LIST_OBJECTS":
                    return ListObjects();
                    
                case "SET_PARENT":
                    return SetParent(parts);
                    
                case "SET_PRIMITIVE_MESH":
                    return SetPrimitiveMesh(parts);
                    
                default:
                    return $"ERROR: Unknown command '{action}'";
            }
        }
        catch (System.Exception ex)
        {
            return $"ERROR: {ex.Message}";
        }
    }

    static string AddComponent(string[] parts)
    {
        if (parts.Length < 3) return "ERROR: ADD_COMPONENT requires ObjectName:ScriptName";
        
        string objectName = parts[1];
        string scriptName = parts[2];
        
        GameObject obj = GameObject.Find(objectName);
        if (obj == null) return $"ERROR: GameObject '{objectName}' not found";
        
        // Try to find the component type
        System.Type componentType = FindComponentType(scriptName);
        if (componentType == null) return $"ERROR: Component type '{scriptName}' not found";
        
        // Check if component already exists
        if (obj.GetComponent(componentType) != null)
            return $"WARNING: Component '{scriptName}' already exists on '{objectName}'";
        
        // Add the component
        obj.AddComponent(componentType);
        EditorUtility.SetDirty(obj);
        
        return $"SUCCESS: Added '{scriptName}' to '{objectName}'";
    }

    static string RemoveComponent(string[] parts)
    {
        if (parts.Length < 3) return "ERROR: REMOVE_COMPONENT requires ObjectName:ComponentType";
        
        string objectName = parts[1];
        string componentName = parts[2];
        
        GameObject obj = GameObject.Find(objectName);
        if (obj == null) return $"ERROR: GameObject '{objectName}' not found";
        
        System.Type componentType = FindComponentType(componentName);
        if (componentType == null) return $"ERROR: Component type '{componentName}' not found";
        
        Component component = obj.GetComponent(componentType);
        if (component == null) return $"ERROR: Component '{componentName}' not found on '{objectName}'";
        
        Object.DestroyImmediate(component);
        EditorUtility.SetDirty(obj);
        
        return $"SUCCESS: Removed '{componentName}' from '{objectName}'";
    }

    static string SetProperty(string[] parts)
    {
        if (parts.Length < 5) return "ERROR: SET_PROPERTY requires ObjectName:ComponentType:PropertyName:Value";
        
        string objectName = parts[1];
        string componentName = parts[2];
        string propertyName = parts[3];
        string value = parts[4];
        
        GameObject obj = GameObject.Find(objectName);
        if (obj == null) return $"ERROR: GameObject '{objectName}' not found";
        
        System.Type componentType = FindComponentType(componentName);
        if (componentType == null) return $"ERROR: Component type '{componentName}' not found";
        
        Component component = obj.GetComponent(componentType);
        if (component == null) return $"ERROR: Component '{componentName}' not found on '{objectName}'";
        
        // Try to set the property using reflection
        try
        {
            PropertyInfo prop = componentType.GetProperty(propertyName);
            FieldInfo field = componentType.GetField(propertyName);
            
            if (prop != null && prop.CanWrite)
            {
                object convertedValue = ConvertValue(value, prop.PropertyType);
                prop.SetValue(component, convertedValue);
                EditorUtility.SetDirty(obj);
                return $"SUCCESS: Set {objectName}.{componentName}.{propertyName} = {value}";
            }
            else if (field != null)
            {
                object convertedValue = ConvertValue(value, field.FieldType);
                field.SetValue(component, convertedValue);
                EditorUtility.SetDirty(obj);
                return $"SUCCESS: Set {objectName}.{componentName}.{propertyName} = {value}";
            }
            else
            {
                return $"ERROR: Property/Field '{propertyName}' not found in '{componentName}'";
            }
        }
        catch (System.Exception ex)
        {
            return $"ERROR: Failed to set property: {ex.Message}";
        }
    }

    static string CreateObject(string[] parts)
    {
        if (parts.Length < 2) return "ERROR: CREATE_OBJECT requires ObjectName";
        
        string objectName = parts[1];
        
        // Check if object already exists
        if (GameObject.Find(objectName) != null)
            return $"ERROR: GameObject '{objectName}' already exists";
        
        Vector3 position = Vector3.zero;
        Vector3 rotation = Vector3.zero;
        Vector3 scale = Vector3.one;
        
        // Parse optional position, rotation, scale
        if (parts.Length > 2)
        {
            string[] positionParts = parts[2].Split(',');
            if (positionParts.Length == 3)
            {
                if (float.TryParse(positionParts[0], out float x) &&
                    float.TryParse(positionParts[1], out float y) &&
                    float.TryParse(positionParts[2], out float z))
                {
                    position = new Vector3(x, y, z);
                }
            }
        }
        
        GameObject newObj = new GameObject(objectName);
        newObj.transform.position = position;
        newObj.transform.eulerAngles = rotation;
        newObj.transform.localScale = scale;
        
        return $"SUCCESS: Created GameObject '{objectName}' at {position}";
    }

    static string DeleteObject(string[] parts)
    {
        if (parts.Length < 2) return "ERROR: DELETE_OBJECT requires ObjectName";
        
        string objectName = parts[1];
        
        GameObject obj = GameObject.Find(objectName);
        if (obj == null) return $"ERROR: GameObject '{objectName}' not found";
        
        Object.DestroyImmediate(obj);
        
        return $"SUCCESS: Deleted GameObject '{objectName}'";
    }

    static string GetSceneInfo()
    {
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
        var info = new System.Text.StringBuilder();
        
        info.AppendLine("=== SCENE INFO ===");
        info.AppendLine($"Total Objects: {allObjects.Length}");
        info.AppendLine();
        
        foreach (GameObject obj in allObjects)
        {
            info.AppendLine($"GameObject: {obj.name}");
            info.AppendLine($"  Position: {obj.transform.position}");
            info.AppendLine($"  Active: {obj.activeInHierarchy}");
            
            Component[] components = obj.GetComponents<Component>();
            info.AppendLine($"  Components ({components.Length}):");
            
            foreach (Component comp in components)
            {
                if (comp != null)
                    info.AppendLine($"    - {comp.GetType().Name}");
            }
            
            info.AppendLine();
        }
        
        return info.ToString();
    }

    static string ListObjects()
    {
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
        var list = new System.Text.StringBuilder();
        
        list.AppendLine("=== OBJECTS LIST ===");
        
        foreach (GameObject obj in allObjects)
        {
            list.AppendLine(obj.name);
        }
        
        return list.ToString();
    }

    static string SetParent(string[] parts)
    {
        if (parts.Length < 3) return "ERROR: SET_PARENT requires ChildObjectName:ParentObjectName";
        
        string childName = parts[1];
        string parentName = parts[2];
        
        GameObject childObj = GameObject.Find(childName);
        if (childObj == null) return $"ERROR: Child GameObject '{childName}' not found";
        
        GameObject parentObj = GameObject.Find(parentName);
        if (parentObj == null) return $"ERROR: Parent GameObject '{parentName}' not found";
        
        childObj.transform.SetParent(parentObj.transform);
        EditorUtility.SetDirty(childObj);
        
        return $"SUCCESS: Set '{childName}' as child of '{parentName}'";
    }

    static System.Type FindComponentType(string typeName)
    {
        // Try exact match first
        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            System.Type type = assembly.GetType(typeName);
            if (type != null && (type.IsSubclassOf(typeof(MonoBehaviour)) || type.IsSubclassOf(typeof(Component))))
                return type;
        }
        
        // Try partial match
        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.Name == typeName && (type.IsSubclassOf(typeof(MonoBehaviour)) || type.IsSubclassOf(typeof(Component))))
                    return type;
            }
        }
        
        return null;
    }

    static string SetPrimitiveMesh(string[] parts)
    {
        if (parts.Length < 3) return "ERROR: SET_PRIMITIVE_MESH requires ObjectName:PrimitiveType";
        
        string objectName = parts[1];
        string primitiveType = parts[2].ToUpper();
        
        GameObject obj = GameObject.Find(objectName);
        if (obj == null) return $"ERROR: GameObject '{objectName}' not found";
        
        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        if (meshFilter == null) return $"ERROR: MeshFilter component not found on '{objectName}'";
        
        MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
        if (meshRenderer == null) return $"ERROR: MeshRenderer component not found on '{objectName}'";
        
        try
        {
            Mesh primitiveMesh = null;
            
            switch (primitiveType)
            {
                case "PLANE":
                case "QUAD":
                    primitiveMesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
                    break;
                case "CUBE":
                    primitiveMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
                    break;
                case "SPHERE":
                    primitiveMesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
                    break;
                case "CYLINDER":
                    primitiveMesh = Resources.GetBuiltinResource<Mesh>("Cylinder.fbx");
                    break;
                case "CAPSULE":
                    primitiveMesh = Resources.GetBuiltinResource<Mesh>("Capsule.fbx");
                    break;
                default:
                    return $"ERROR: Unknown primitive type '{primitiveType}'. Supported: PLANE, CUBE, SPHERE, CYLINDER, CAPSULE";
            }
            
            if (primitiveMesh == null)
                return $"ERROR: Could not load primitive mesh for '{primitiveType}'";
            
            meshFilter.sharedMesh = primitiveMesh;
            
            // Set default material if none exists
            if (meshRenderer.sharedMaterial == null)
            {
                Material defaultMaterial = Resources.GetBuiltinResource<Material>("Default-Material.mat");
                if (defaultMaterial != null)
                {
                    meshRenderer.sharedMaterial = defaultMaterial;
                }
            }
            
            EditorUtility.SetDirty(obj);
            
            return $"SUCCESS: Set '{objectName}' mesh to {primitiveType}";
        }
        catch (System.Exception ex)
        {
            return $"ERROR: Failed to set primitive mesh: {ex.Message}";
        }
    }

    static object ConvertValue(string value, System.Type targetType)
    {
        if (targetType == typeof(string)) return value;
        if (targetType == typeof(int)) return int.Parse(value);
        if (targetType == typeof(float)) return float.Parse(value, CultureInfo.InvariantCulture);
        if (targetType == typeof(bool)) return bool.Parse(value);
        if (targetType == typeof(Vector3))
        {
            string[] parts = value.Split(',');
            if (parts.Length == 3)
            {
                return new Vector3(
                    float.Parse(parts[0], CultureInfo.InvariantCulture),
                    float.Parse(parts[1], CultureInfo.InvariantCulture),
                    float.Parse(parts[2], CultureInfo.InvariantCulture)
                );
            }
        }
        
        // Default: try to convert using System.Convert
        return System.Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
    }
}