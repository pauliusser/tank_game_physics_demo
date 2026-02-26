// How to Use:

// 1. Take a Snapshot during Play Mode:
// Enter Play Mode (Ctrl/Cmd + P)
// Select GameObjects whose component values you want to capture
// Tune component values (e.g., Rigidbody mass, Renderer color, etc.)
// Open Tools > PlayMode Snapshot window
// Click "Take Snapshot" button

// 2. Apply Snapshot after Play Mode:
// Exit Play Mode
// Ensure the same GameObjects exist in the scene
// Open the PlayMode Snapshot window
// Click "Apply Snapshot" to restore all captured values

// 3. Additional Features:
// Save/Load: Save snapshots to JSON files for later use
// Auto-save: Automatically saves when exiting Play Mode (optional)
// Visual preview: See what components/values were captured
// Undo support: Use Ctrl/Cmd + Z to undo applied changes

// What It Captures:
// ✅ Public fields on components
// ✅ Private fields with [SerializeField] attribute
// ✅ Properties with [SerializeField] attribute
// ✅ Common Unity types: Vector3, Quaternion, Color, etc.
// ✅ Basic types: int, float, bool, string, enums

using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;

public class PlayModeSnapshot : EditorWindow
{
    [System.Serializable]
    private class ComponentSnapshot
    {
        public string componentType;
        public string componentTypeFullName;
        public string assemblyName; // Added to help with type resolution
        public List<FieldValue> fieldValues = new List<FieldValue>();
        public List<PropertyValue> propertyValues = new List<PropertyValue>();
        
        [System.Serializable]
        public class FieldValue
        {
            public string fieldName;
            public string value;
            public string fieldType;
            public string fieldTypeFullName;
        }
        
        [System.Serializable]
        public class PropertyValue
        {
            public string propertyName;
            public string value;
            public string propertyType;
            public string propertyTypeFullName;
        }
    }
    
    [System.Serializable]
    private class GameObjectSnapshot
    {
        public string gameObjectName;
        public string gameObjectPath;
        public List<ComponentSnapshot> components = new List<ComponentSnapshot>();
    }
    
    [System.Serializable]
    private class SnapshotData
    {
        public List<GameObjectSnapshot> snapshots = new List<GameObjectSnapshot>();
        public string timestamp;
        public string sceneName;
    }
    
    private static List<GameObjectSnapshot> currentSnapshots = new List<GameObjectSnapshot>();
    private static string lastSavedSnapshotPath;
    private Vector2 scrollPos;
    private bool showComponents = true;
    private static PlayModeSnapshot windowInstance;
    private static Dictionary<string, Type> typeCache = new Dictionary<string, Type>();
    private string debugInfo = "";
    
    [MenuItem("Tools/PlayMode Snapshot")]
    public static void ShowWindow()
    {
        windowInstance = GetWindow<PlayModeSnapshot>("PlayMode Snapshot");
    }
    
    void OnGUI()
    {
        GUILayout.Label("PlayMode Component Snapshot", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Status
        bool isPlaying = EditorApplication.isPlaying;
        GUILayout.Label($"Status: {(isPlaying ? "PLAY MODE" : "EDIT MODE")}", 
                       isPlaying ? EditorStyles.boldLabel : EditorStyles.label);
        
        EditorGUILayout.Space();
        
        // Control buttons
        EditorGUILayout.BeginHorizontal();
        
        GUI.enabled = isPlaying && Selection.gameObjects.Length > 0;
        if (GUILayout.Button("Take Snapshot", GUILayout.Height(40)))
        {
            TakeSnapshot();
        }
        
        GUI.enabled = !isPlaying && currentSnapshots.Count > 0;
        if (GUILayout.Button("Apply Snapshot", GUILayout.Height(40)))
        {
            ApplySnapshot();
        }
        
        GUI.enabled = currentSnapshots.Count > 0;
        if (GUILayout.Button("Clear", GUILayout.Height(40)))
        {
            currentSnapshots.Clear();
            lastSavedSnapshotPath = null;
            debugInfo = "";
        }
        
        GUI.enabled = true;
        
        if (GUILayout.Button("Save to File", GUILayout.Height(40)))
        {
            SaveSnapshotToFile();
        }
        
        if (GUILayout.Button("Load from File", GUILayout.Height(40)))
        {
            LoadSnapshotFromFile();
        }
        
        if (GUILayout.Button("Debug Types", GUILayout.Width(100)))
        {
            ShowDebugInfo();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Debug info
        if (!string.IsNullOrEmpty(debugInfo))
        {
            EditorGUILayout.HelpBox(debugInfo, MessageType.Info);
        }
        
        // Snapshot info
        GUILayout.Label($"Snapshots: {currentSnapshots.Count} GameObjects", EditorStyles.boldLabel);
        
        if (currentSnapshots.Count == 0)
        {
            EditorGUILayout.HelpBox("No snapshots taken. Enter Play Mode, select GameObjects, and click 'Take Snapshot'.", 
                                  MessageType.Info);
        }
        else
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            
            foreach (var goSnapshot in currentSnapshots)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label($"📷 {goSnapshot.gameObjectName}", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                GUILayout.Label($"{goSnapshot.components.Count} components");
                EditorGUILayout.EndHorizontal();
                
                GUILayout.Label($"Path: {goSnapshot.gameObjectPath}", EditorStyles.miniLabel);
                
                if (showComponents)
                {
                    foreach (var compSnapshot in goSnapshot.components)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.textArea);
                        GUILayout.Label($"• {GetTypeShortName(compSnapshot.componentTypeFullName ?? compSnapshot.componentType)}", EditorStyles.miniBoldLabel);
                        
                        // Show field values
                        foreach (var field in compSnapshot.fieldValues)
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label($"  {field.fieldName}:", GUILayout.Width(120));
                            GUILayout.Label(TruncateString(field.value, 50), EditorStyles.miniLabel);
                            EditorGUILayout.EndHorizontal();
                        }
                        
                        EditorGUILayout.EndVertical();
                    }
                }
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        if (!string.IsNullOrEmpty(lastSavedSnapshotPath))
        {
            EditorGUILayout.HelpBox($"Last saved: {Path.GetFileName(lastSavedSnapshotPath)}", MessageType.None);
        }
    }
    
    private void TakeSnapshot()
    {
        currentSnapshots.Clear();
        typeCache.Clear();
        debugInfo = "";
        
        foreach (GameObject go in Selection.gameObjects)
        {
            var goSnapshot = new GameObjectSnapshot
            {
                gameObjectName = go.name,
                gameObjectPath = GetGameObjectPath(go)
            };
            
            // Get all components except Transform
            var components = go.GetComponents<Component>()
                .Where(c => c != null && !(c is Transform))
                .ToList();
            
            foreach (var component in components)
            {
                var type = component.GetType();
                var compSnapshot = new ComponentSnapshot
                {
                    componentType = type.AssemblyQualifiedName,
                    componentTypeFullName = type.FullName,
                    assemblyName = type.Assembly.GetName().Name
                };
                
                // Get serializable fields
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(f => IsSerializableField(f));
                
                foreach (var field in fields)
                {
                    try
                    {
                        var value = field.GetValue(component);
                        compSnapshot.fieldValues.Add(new ComponentSnapshot.FieldValue
                        {
                            fieldName = field.Name,
                            value = ConvertToString(value),
                            fieldType = field.FieldType.Name,
                            fieldTypeFullName = field.FieldType.FullName
                        });
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Failed to get field {field.Name} on {type.Name}: {e.Message}");
                    }
                }
                
                // Get serializable properties
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(p => p.CanRead && p.CanWrite && IsSerializableProperty(p));
                
                foreach (var property in properties)
                {
                    try
                    {
                        var value = property.GetValue(component, null);
                        compSnapshot.propertyValues.Add(new ComponentSnapshot.PropertyValue
                        {
                            propertyName = property.Name,
                            value = ConvertToString(value),
                            propertyType = property.PropertyType.Name,
                            propertyTypeFullName = property.PropertyType.FullName
                        });
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Failed to get property {property.Name} on {type.Name}: {e.Message}");
                    }
                }
                
                if (compSnapshot.fieldValues.Count > 0 || compSnapshot.propertyValues.Count > 0)
                {
                    goSnapshot.components.Add(compSnapshot);
                }
            }
            
            if (goSnapshot.components.Count > 0)
            {
                currentSnapshots.Add(goSnapshot);
            }
        }
        
        string typeList = string.Join(", ", currentSnapshots
            .SelectMany(go => go.components)
            .Select(c => GetTypeShortName(c.componentTypeFullName))
            .Distinct());
        
        debugInfo = $"Captured types: {typeList}";
        Debug.Log($"Snapshot taken: {currentSnapshots.Count} GameObjects, {currentSnapshots.Sum(g => g.components.Count)} components");
        ShowNotification(new GUIContent($"Snapshot: {currentSnapshots.Count} GameObjects"));
    }
    
    private void ApplySnapshot()
    {
        if (currentSnapshots.Count == 0)
        {
            EditorUtility.DisplayDialog("No Snapshot", "No snapshot to apply. Take a snapshot first.", "OK");
            return;
        }
        
        int appliedCount = 0;
        int skippedCount = 0;
        int typeNotFoundCount = 0;
        List<string> missingTypes = new List<string>();
        
        foreach (var goSnapshot in currentSnapshots)
        {
            GameObject targetGO = FindGameObjectByPath(goSnapshot.gameObjectPath);
            if (targetGO == null)
            {
                Debug.LogWarning($"GameObject not found: {goSnapshot.gameObjectPath}");
                skippedCount++;
                continue;
            }
            
            foreach (var compSnapshot in goSnapshot.components)
            {
                Type componentType = ResolveComponentType(compSnapshot);
                if (componentType == null)
                {
                    string typeName = compSnapshot.componentTypeFullName ?? compSnapshot.componentType;
                    Debug.LogWarning($"Component type not found: {typeName}");
                    if (!missingTypes.Contains(typeName))
                        missingTypes.Add(typeName);
                    typeNotFoundCount++;
                    continue;
                }
                
                Component component = targetGO.GetComponent(componentType);
                if (component == null)
                {
                    Debug.Log($"Adding missing component: {componentType.Name} to {targetGO.name}");
                    component = targetGO.AddComponent(componentType);
                    
                    // Wait one frame for component initialization
                    EditorApplication.delayCall += () => {
                        if (component != null)
                        {
                            ApplyComponentValues(component, compSnapshot, ref appliedCount);
                        }
                    };
                }
                else
                {
                    ApplyComponentValues(component, compSnapshot, ref appliedCount);
                }
            }
        }
        
        // Force immediate update
        EditorUtility.SetDirty(Selection.activeGameObject);
        SceneView.RepaintAll();
        
        string result = $"Applied: {appliedCount} values, Skipped: {skippedCount} GameObjects";
        if (typeNotFoundCount > 0)
        {
            result += $", Missing types: {typeNotFoundCount}";
            debugInfo = $"Missing types:\n{string.Join("\n", missingTypes)}";
            
            EditorUtility.DisplayDialog("Some Components Not Found", 
                $"{typeNotFoundCount} component types could not be found.\n\n" +
                "Common reasons:\n" +
                "1. Script name changed\n" +
                "2. Script moved to different namespace\n" +
                "3. Script is not in the current assembly\n" +
                "4. Script compilation errors\n\n" +
                "Check the console for details.", 
                "OK");
        }
        else
        {
            debugInfo = result;
        }
        
        Debug.Log(result);
        ShowNotification(new GUIContent($"Applied: {appliedCount} values"));
    }
    
    private void ApplyComponentValues(Component component, ComponentSnapshot snapshot, ref int appliedCount)
    {
        Type componentType = component.GetType();
        Undo.RecordObject(component, "Apply PlayMode Snapshot");
        
        // Apply field values
        foreach (var fieldValue in snapshot.fieldValues)
        {
            try
            {
                var field = componentType.GetField(fieldValue.fieldName, 
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null && IsSerializableField(field))
                {
                    object convertedValue = ConvertFromString(fieldValue.value, field.FieldType);
                    if (convertedValue != null)
                    {
                        field.SetValue(component, convertedValue);
                        appliedCount++;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to set field {fieldValue.fieldName} on {componentType.Name}: {e.Message}");
            }
        }
        
        // Apply property values
        foreach (var propertyValue in snapshot.propertyValues)
        {
            try
            {
                var property = componentType.GetProperty(propertyValue.propertyName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (property != null && property.CanWrite && IsSerializableProperty(property))
                {
                    object convertedValue = ConvertFromString(propertyValue.value, property.PropertyType);
                    if (convertedValue != null)
                    {
                        property.SetValue(component, convertedValue, null);
                        appliedCount++;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to set property {propertyValue.propertyName} on {componentType.Name}: {e.Message}");
            }
        }
        
        EditorUtility.SetDirty(component);
    }
    
    private Type ResolveComponentType(ComponentSnapshot snapshot)
    {
        // Try cache first
        string cacheKey = snapshot.componentType ?? snapshot.componentTypeFullName;
        if (typeCache.ContainsKey(cacheKey))
            return typeCache[cacheKey];
        
        Type type = null;
        
        // Try multiple resolution strategies
        if (!string.IsNullOrEmpty(snapshot.componentType))
        {
            type = Type.GetType(snapshot.componentType);
        }
        
        if (type == null && !string.IsNullOrEmpty(snapshot.componentTypeFullName))
        {
            // Try direct full name lookup
            type = Type.GetType(snapshot.componentTypeFullName);
            
            if (type == null)
            {
                // Search in all assemblies
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = assembly.GetType(snapshot.componentTypeFullName);
                    if (type != null) break;
                }
            }
        }
        
        if (type == null && !string.IsNullOrEmpty(snapshot.componentTypeFullName))
        {
            // Try without namespace (just class name)
            string shortName = GetTypeShortName(snapshot.componentTypeFullName);
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetTypes().FirstOrDefault(t => t.Name == shortName);
                if (type != null) break;
            }
        }
        
        if (type != null)
        {
            typeCache[cacheKey] = type;
        }
        
        return type;
    }
    
    private string GetTypeShortName(string fullTypeName)
    {
        if (string.IsNullOrEmpty(fullTypeName)) return "Unknown";
        
        if (fullTypeName.Contains(","))
        {
            // Assembly qualified name
            return fullTypeName.Split(',')[0].Trim().Split('.').Last();
        }
        
        if (fullTypeName.Contains("."))
        {
            // Namespace.ClassName
            return fullTypeName.Split('.').Last();
        }
        
        return fullTypeName;
    }
    
    private void SaveSnapshotToFile()
    {
        if (currentSnapshots.Count == 0)
        {
            EditorUtility.DisplayDialog("No Data", "No snapshot data to save.", "OK");
            return;
        }
        
        string path = EditorUtility.SaveFilePanel(
            "Save Snapshot",
            "Assets",
            $"Snapshot_{DateTime.Now:yyyyMMdd_HHmmss}.json",
            "json");
        
        if (!string.IsNullOrEmpty(path))
        {
            var data = new SnapshotData
            {
                snapshots = currentSnapshots,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            };
            
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json);
            
            lastSavedSnapshotPath = path;
            debugInfo = $"Saved to: {Path.GetFileName(path)}";
            Debug.Log($"Snapshot saved to: {path}");
            ShowNotification(new GUIContent("Saved to file"));
        }
    }
    
    private void LoadSnapshotFromFile()
    {
        string path = EditorUtility.OpenFilePanel("Load Snapshot", "Assets", "json");
        
        if (!string.IsNullOrEmpty(path) && File.Exists(path))
        {
            string json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<SnapshotData>(json);
            
            if (data != null && data.snapshots != null)
            {
                currentSnapshots = data.snapshots;
                lastSavedSnapshotPath = path;
                typeCache.Clear();
                
                // Analyze loaded types
                var allTypes = currentSnapshots
                    .SelectMany(go => go.components)
                    .Select(c => GetTypeShortName(c.componentTypeFullName ?? c.componentType))
                    .Distinct()
                    .ToList();
                
                debugInfo = $"Loaded {currentSnapshots.Count} GameObjects\nTypes: {string.Join(", ", allTypes)}";
                
                Debug.Log($"Snapshot loaded: {currentSnapshots.Count} GameObjects from {data.timestamp}");
                ShowNotification(new GUIContent($"Loaded {currentSnapshots.Count} GameObjects"));
                Repaint();
            }
        }
    }
    
    private void ShowDebugInfo()
    {
        if (currentSnapshots.Count == 0)
        {
            debugInfo = "No snapshots loaded.";
            return;
        }
        
        var typeInfo = new List<string>();
        foreach (var goSnapshot in currentSnapshots)
        {
            foreach (var compSnapshot in goSnapshot.components)
            {
                string shortName = GetTypeShortName(compSnapshot.componentTypeFullName ?? compSnapshot.componentType);
                Type resolvedType = ResolveComponentType(compSnapshot);
                
                string status = resolvedType != null ? "✓ Found" : "✗ Missing";
                typeInfo.Add($"{status}: {shortName} (from: {compSnapshot.componentTypeFullName})");
            }
        }
        
        debugInfo = "Type Resolution Status:\n" + string.Join("\n", typeInfo.Distinct());
    }
    
    // Helper methods (keep the same as before with minor improvements)
    private static bool IsSerializableField(FieldInfo field)
    {
        return (field.IsPublic || field.GetCustomAttribute<SerializeField>() != null) 
               && !field.IsStatic
               && !field.IsInitOnly;
    }
    
    private static bool IsSerializableProperty(PropertyInfo property)
    {
        return property.GetCustomAttribute<SerializeField>() != null;
    }
    
    private static string ConvertToString(object value)
    {
        if (value == null) return "null";
        
        if (value is UnityEngine.Object unityObj)
            return unityObj != null ? unityObj.name : "null";
        
        if (value is Vector3 vec3)
            return $"{vec3.x},{vec3.y},{vec3.z}";
        
        if (value is Vector2 vec2)
            return $"{vec2.x},{vec2.y}";
        
        if (value is Quaternion quat)
            return $"{quat.x},{quat.y},{quat.z},{quat.w}";
        
        if (value is Color color)
            return $"{color.r},{color.g},{color.b},{color.a}";
        
        if (value.GetType().IsEnum)
            return value.ToString();
        
        return value.ToString();
    }
    
    private static object ConvertFromString(string str, Type targetType)
    {
        if (str == "null") return null;
        
        try
        {
            if (targetType == typeof(string)) return str;
            if (targetType == typeof(int)) return int.Parse(str);
            if (targetType == typeof(float)) return float.Parse(str);
            if (targetType == typeof(bool)) return bool.Parse(str);
            if (targetType == typeof(double)) return double.Parse(str);
            
            if (targetType == typeof(Vector3))
            {
                var parts = str.Split(',');
                return new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
            }
            
            if (targetType == typeof(Vector2))
            {
                var parts = str.Split(',');
                return new Vector2(float.Parse(parts[0]), float.Parse(parts[1]));
            }
            
            if (targetType == typeof(Quaternion))
            {
                var parts = str.Split(',');
                return new Quaternion(float.Parse(parts[0]), float.Parse(parts[1]), 
                                     float.Parse(parts[2]), float.Parse(parts[3]));
            }
            
            if (targetType == typeof(Color))
            {
                var parts = str.Split(',');
                return new Color(float.Parse(parts[0]), float.Parse(parts[1]), 
                                float.Parse(parts[2]), float.Parse(parts[3]));
            }
            
            if (targetType.IsEnum)
                return Enum.Parse(targetType, str);
            
            return Convert.ChangeType(str, targetType);
        }
        catch
        {
            Debug.LogWarning($"Failed to convert '{str}' to {targetType.Name}");
            return null;
        }
    }
    
    private static string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        
        return "/" + path;
    }
    
    private static GameObject FindGameObjectByPath(string path)
    {
        if (path.StartsWith("/"))
            path = path.Substring(1);
        
        string[] parts = path.Split('/');
        if (parts.Length == 0) return null;
        
        GameObject current = null;
        foreach (var part in parts)
        {
            if (current == null)
            {
                current = GameObject.Find(part);
                if (current == null) return null;
            }
            else
            {
                Transform child = current.transform.Find(part);
                if (child == null)
                {
                    child = FindDeepChild(current.transform, part);
                    if (child == null) return null;
                }
                current = child.gameObject;
            }
        }
        
        return current;
    }
    
    private static Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            
            Transform result = FindDeepChild(child, name);
            if (result != null) return result;
        }
        return null;
    }
    
    private string TruncateString(string str, int maxLength)
    {
        if (string.IsNullOrEmpty(str) || str.Length <= maxLength)
            return str;
        
        return str.Substring(0, maxLength - 3) + "...";
    }
    
    // Keyboard shortcuts
    [MenuItem("Tools/Take Snapshot %#s", false, 100)]
    private static void MenuTakeSnapshot()
    {
        if (!EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Not in Play Mode", "You must be in Play Mode to take a snapshot.", "OK");
            return;
        }
        
        if (Selection.gameObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("No Selection", "Select GameObjects first to take a snapshot.", "OK");
            return;
        }
        
        if (windowInstance == null)
            ShowWindow();
        
        windowInstance.TakeSnapshot();
    }
    
    [MenuItem("Tools/Apply Snapshot %#a", false, 101)]
    private static void MenuApplySnapshot()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("In Play Mode", "Exit Play Mode first to apply a snapshot.", "OK");
            return;
        }
        
        if (currentSnapshots.Count == 0)
        {
            EditorUtility.DisplayDialog("No Snapshot", "Take a snapshot first or load one from file.", "OK");
            return;
        }
        
        if (windowInstance == null)
            ShowWindow();
        
        windowInstance.ApplySnapshot();
    }
    
    // Auto-save on Play Mode exit
    [InitializeOnLoad]
    public static class PlayModeStateNotifier
    {
        static PlayModeStateNotifier()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode && currentSnapshots.Count > 0)
            {
                string directory = "Assets/Snapshots";
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                
                string autoSavePath = $"{directory}/AutoSnapshot_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                
                var data = new SnapshotData
                {
                    snapshots = currentSnapshots,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
                };
                
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(autoSavePath, json);
                
                AssetDatabase.Refresh();
                Debug.Log($"Auto-snapshot saved to: {autoSavePath}");
            }
            
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                currentSnapshots.Clear();
                typeCache.Clear();
            }
        }
    }
}