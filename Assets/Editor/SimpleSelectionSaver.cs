using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// How to Use:
// Place the script anywhere in your Assets folder (preferably in an Editor folder)
// Open the window: Go to Window > Simple Selection Saver
// Save a selection:
// Select objects in your scene
// Enter a name in the window
// Click "Save Selection"
// Load a selection:
// Find your saved set in the list
// Click "Load"
// Quick Save (Keyboard shortcut):
// Select objects
// Press Ctrl/Cmd + Alt + S
// Enter a name when prompted


public class SimpleSelectionSaver : EditorWindow
{
    [System.Serializable]
    private class SelectionSet
    {
        public string setName;
        public List<string> objectPaths = new List<string>();
    }
    
    [System.Serializable]
    private class SelectionData
    {
        public List<SelectionSet> sets = new List<SelectionSet>();
    }
    
    private static SelectionData data;
    private string newSetName = "NewSet";
    private Vector2 scrollPos;
    private static string SavePath => "Assets/Editor/SelectionSets.json";
    
    [MenuItem("Window/Simple Selection Saver")]
    public static void ShowWindow()
    {
        GetWindow<SimpleSelectionSaver>("Selection Saver");
        LoadData();
    }
    
    private static void LoadData()
    {
        if (System.IO.File.Exists(SavePath))
        {
            string json = System.IO.File.ReadAllText(SavePath);
            data = JsonUtility.FromJson<SelectionData>(json);
        }
        else
        {
            data = new SelectionData();
        }
    }
    
    private static void SaveData()
    {
        string json = JsonUtility.ToJson(data, true);
        System.IO.File.WriteAllText(SavePath, json);
        AssetDatabase.Refresh();
    }
    
    private void OnGUI()
    {
        if (data == null) LoadData();
        
        GUILayout.Label("Simple Selection Saver", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Current selection info
        int selectedCount = Selection.gameObjects?.Length ?? 0;
        GUILayout.Label($"Currently Selected: {selectedCount} objects");
        EditorGUILayout.Space();
        
        // Save section
        EditorGUILayout.BeginHorizontal();
        newSetName = EditorGUILayout.TextField("Set Name:", newSetName);
        
        EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(newSetName) || selectedCount == 0);
        if (GUILayout.Button("Save Selection", GUILayout.Width(120)))
        {
            SaveCurrentSelection(newSetName);
            newSetName = "NewSet";
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(20);
        
        // Saved sets section
        GUILayout.Label("Saved Sets:", EditorStyles.boldLabel);
        
        if (data.sets.Count == 0)
        {
            EditorGUILayout.HelpBox("No saved selection sets. Select objects and save a set above.", MessageType.Info);
        }
        else
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            
            for (int i = 0; i < data.sets.Count; i++)
            {
                var set = data.sets[i];
                
                EditorGUILayout.BeginHorizontal();
                
                // Set name with object count
                GUILayout.Label($"{set.setName} ({set.objectPaths.Count} objects)", GUILayout.Width(200));
                
                // Load button
                if (GUILayout.Button("Load", GUILayout.Width(60)))
                {
                    LoadSelectionSet(set);
                }
                
                // Delete button
                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                {
                    data.sets.RemoveAt(i);
                    SaveData();
                    break;
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
        }
    }
    
    private void SaveCurrentSelection(string setName)
    {
        // Check if name already exists
        var existingSet = data.sets.FirstOrDefault(s => s.setName == setName);
        if (existingSet != null)
        {
            if (!EditorUtility.DisplayDialog("Overwrite Set", 
                $"Set '{setName}' already exists. Overwrite?", "Yes", "No"))
            {
                return;
            }
            data.sets.Remove(existingSet);
        }
        
        // Create new set
        var newSet = new SelectionSet { setName = setName };
        
        foreach (GameObject obj in Selection.gameObjects)
        {
            newSet.objectPaths.Add(GetObjectPath(obj));
        }
        
        data.sets.Add(newSet);
        SaveData();
        
        Debug.Log($"Saved selection set: '{setName}' with {newSet.objectPaths.Count} objects");
    }
    
    private void LoadSelectionSet(SelectionSet set)
    {
        List<Object> objectsToSelect = new List<Object>();
        
        foreach (string path in set.objectPaths)
        {
            GameObject obj = GameObject.Find(path);
            if (obj != null)
            {
                objectsToSelect.Add(obj);
            }
            else
            {
                Debug.LogWarning($"Object not found: {path}");
            }
        }
        
        Selection.objects = objectsToSelect.ToArray();
        
        if (objectsToSelect.Count > 0)
        {
            EditorGUIUtility.PingObject(objectsToSelect[0]);
            SceneView.lastActiveSceneView?.FrameSelected();
        }
        
        Debug.Log($"Loaded selection set: '{set.setName}' - {objectsToSelect.Count}/{set.objectPaths.Count} objects found");
    }
    
    private string GetObjectPath(GameObject obj)
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
    
    // Quick access menu items
    [MenuItem("Tools/Quick Save Selection %&s", false, 100)]
    private static void QuickSaveSelection()
    {
        if (data == null) LoadData();
        
        string defaultName = $"Selection_{System.DateTime.Now:yyyyMMdd_HHmmss}";
        string setName = EditorInputDialog.Show("Save Selection", "Set name:", defaultName);
        
        if (!string.IsNullOrEmpty(setName))
        {
            var newSet = new SelectionSet { setName = setName };
            
            foreach (GameObject obj in Selection.gameObjects)
            {
                newSet.objectPaths.Add(GetObjectPathStatic(obj));
            }
            
            data.sets.Add(newSet);
            SaveData();
            Debug.Log($"Quick saved: '{setName}' ({newSet.objectPaths.Count} objects)");
        }
    }
    
    [MenuItem("Tools/Quick Save Selection %&s", true)]
    private static bool ValidateQuickSaveSelection()
    {
        return Selection.gameObjects.Length > 0;
    }
    
    private static string GetObjectPathStatic(GameObject obj)
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
}

// Simple input dialog
public class EditorInputDialog : EditorWindow
{
    private string description;
    private string inputText = "";
    private System.Action<string> onOk;
    
    public static string Show(string title, string description, string defaultValue)
    {
        EditorInputDialog window = CreateInstance<EditorInputDialog>();
        window.titleContent = new GUIContent(title);
        window.description = description;
        window.inputText = defaultValue;
        window.minSize = new Vector2(300, 120);
        window.maxSize = new Vector2(300, 120);
        
        // Center the window
        var position = window.position;
        position.center = new Rect(0, 0, Screen.currentResolution.width, Screen.currentResolution.height).center;
        window.position = position;
        
        window.ShowModal();
        return window.inputText;
    }
    
    void OnGUI()
    {
        GUILayout.Label(description, EditorStyles.wordWrappedLabel);
        GUILayout.Space(10);
        
        GUI.SetNextControlName("InputField");
        inputText = EditorGUILayout.TextField(inputText);
        EditorGUI.FocusTextInControl("InputField");
        
        GUILayout.Space(20);
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("OK", GUILayout.Width(80)) || (UnityEngine.Event.current.isKey && UnityEngine.Event.current.keyCode == KeyCode.Return))
        {
            Close();
        }
        
        if (GUILayout.Button("Cancel", GUILayout.Width(80)))
        {
            inputText = "";
            Close();
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    void OnDestroy()
    {
        // Window is being closed
    }
}