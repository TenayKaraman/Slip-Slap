using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class AIContextAgent : EditorWindow
{
    private static string outputPath = "AI_Project_Context";

    [MenuItem("AI Tools/Generate Context")]
    public static void ShowWindow()
    {
        GenerateContext();
        EditorUtility.DisplayDialog("AI Context Agent", "Project context generated!", "OK");
    }

    public static void GenerateContext()
    {
        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        SaveFolderStructure();
        SaveScriptsList();
        SaveSceneHierarchy();
    }

    private static void SaveFolderStructure()
    {
        string[] folders = Directory.GetDirectories(Application.dataPath, "*", SearchOption.AllDirectories);
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("# Project Folder Structure\n");
        foreach (string folder in folders)
        {
            string relPath = folder.Replace(Application.dataPath, "Assets");
            sb.AppendLine(relPath);
        }
        File.WriteAllText(Path.Combine(outputPath, "folder_structure.md"), sb.ToString());
    }

    private static void SaveScriptsList()
    {
        string[] scripts = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("# Scripts in Project\n");
        foreach (string script in scripts)
        {
            string relPath = script.Replace(Application.dataPath, "Assets");
            sb.AppendLine(relPath);
        }
        File.WriteAllText(Path.Combine(outputPath, "scripts_list.md"), sb.ToString());
    }

    private static void SaveSceneHierarchy()
    {
        Scene scene = SceneManager.GetActiveScene();
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("# Scene Hierarchy");
        sb.AppendLine("Scene: " + scene.name + "\n");

        foreach (GameObject obj in scene.GetRootGameObjects())
        {
            PrintObjectHierarchy(obj.transform, 0, sb);
        }

        File.WriteAllText(Path.Combine(outputPath, "scene_hierarchy.md"), sb.ToString());
    }

    private static void PrintObjectHierarchy(Transform obj, int indent, StringBuilder sb)
    {
        sb.AppendLine(new string(' ', indent * 2) + "- " + obj.name);

        Component[] comps = obj.GetComponents<Component>();
        foreach (Component comp in comps)
        {
            if (comp != null)
                sb.AppendLine(new string(' ', (indent + 1) * 2) + $"[Component] {comp.GetType().Name}");
        }

        foreach (Transform child in obj)
        {
            PrintObjectHierarchy(child, indent + 1, sb);
        }
    }
}
