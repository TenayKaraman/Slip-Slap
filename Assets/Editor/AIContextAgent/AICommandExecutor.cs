using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;

[Serializable]
public class AICommandList
{
    public List<AICommand> commands;
}

[Serializable]
public class AICommand
{
    public string action;   // "create", "add_component", "delete", "set_property"
    public string target;   // GameObject adı
    public string param;    // Ek bilgi (örn. component adı veya property adı)
    public string value;    // Yeni değer (örn. "5", "true", "EnemyBoss")
}

public class AICommandExecutor : EditorWindow
{
    private static string commandsFile = "AI_Project_Context/ai_commands.json";

    [MenuItem("AI Tools/Execute AI Commands")]
    public static void ShowWindow()
    {
        ExecuteCommands();
    }

    public static void ExecuteCommands()
    {
        if (!File.Exists(commandsFile))
        {
            EditorUtility.DisplayDialog("AI Command Executor", "No ai_commands.json file found!", "OK");
            return;
        }

        string json = File.ReadAllText(commandsFile);
        AICommandList cmdList = JsonUtility.FromJson<AICommandList>(json);

        if (cmdList == null || cmdList.commands == null || cmdList.commands.Count == 0)
        {
            Debug.LogWarning("No commands found in ai_commands.json");
            return;
        }

        foreach (AICommand cmd in cmdList.commands)
        {
            switch (cmd.action.ToLower())
            {
                case "create":
                    CreateObject(cmd.target);
                    break;
                case "add_component":
                    AddComponent(cmd.target, cmd.param);
                    break;
                case "delete":
                    DeleteObject(cmd.target);
                    break;
                case "set_property":
                    SetProperty(cmd.target, cmd.param, cmd.value);
                    break;
                default:
                    Debug.LogWarning("Unknown action: " + cmd.action);
                    break;
            }
        }

        Debug.Log("AI Commands executed successfully!");
    }

    private static void CreateObject(string name)
    {
        GameObject obj = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(obj, "AI Create Object");
        Debug.Log("Created new GameObject: " + name);
    }

    private static void AddComponent(string target, string componentName)
    {
        GameObject obj = GameObject.Find(target);
        if (obj == null)
        {
            Debug.LogWarning("Target not found: " + target);
            return;
        }

        Type compType = Type.GetType(componentName);
        if (compType == null)
        {
            Debug.LogWarning("Component type not found: " + componentName);
            return;
        }

        obj.AddComponent(compType);
        Debug.Log($"Added {componentName} to {target}");
    }

    private static void DeleteObject(string target)
    {
        GameObject obj = GameObject.Find(target);
        if (obj == null)
        {
            Debug.LogWarning("Target not found: " + target);
            return;
        }

        Undo.DestroyObjectImmediate(obj);
        Debug.Log("Deleted GameObject: " + target);
    }

    private static void SetProperty(string target, string propertyPath, string newValue)
    {
        GameObject obj = GameObject.Find(target);
        if (obj == null)
        {
            Debug.LogWarning("Target not found: " + target);
            return;
        }

        // Property "ComponentName.PropertyName" şeklinde olmalı
        string[] parts = propertyPath.Split('.');
        if (parts.Length != 2)
        {
            Debug.LogWarning("Property path format must be ComponentName.PropertyName");
            return;
        }

        string componentName = parts[0];
        string propName = parts[1];

        Component comp = obj.GetComponent(componentName);
        if (comp == null)
        {
            Debug.LogWarning($"Component {componentName} not found on {target}");
            return;
        }

        var field = comp.GetType().GetField(propName);
        var prop = comp.GetType().GetProperty(propName);

        object convertedValue = null;

        try
        {
            if (field != null)
                convertedValue = Convert.ChangeType(newValue, field.FieldType);
            else if (prop != null)
                convertedValue = Convert.ChangeType(newValue, prop.PropertyType);
            else
            {
                Debug.LogWarning($"Property {propName} not found on {componentName}");
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Value conversion failed: " + e.Message);
            return;
        }

        if (field != null)
        {
            field.SetValue(comp, convertedValue);
            Debug.Log($"Set {target}.{componentName}.{propName} = {newValue}");
        }
        else if (prop != null && prop.CanWrite)
        {
            prop.SetValue(comp, convertedValue);
            Debug.Log($"Set {target}.{componentName}.{propName} = {newValue}");
        }
        else
        {
            Debug.LogWarning($"Property {propName} is not writable on {componentName}");
        }

        EditorUtility.SetDirty(comp);
    }
}
