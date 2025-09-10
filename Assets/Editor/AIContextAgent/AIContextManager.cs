using UnityEditor;
using UnityEngine;
using System.IO;

public class AIContextManager : EditorWindow
{
    private string devNotes = "";
    private static string filePath = "project_context.md";

    [MenuItem("AI Tools/Context Manager")]
    public static void ShowWindow()
    {
        GetWindow<AIContextManager>("AI Context Manager");
    }

    private void OnGUI()
    {
        GUILayout.Label("AI Project Context Manager", EditorStyles.boldLabel);

        if (GUILayout.Button("Open Project Context File"))
        {
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "# Project Context\n\n");
            }
            Application.OpenURL("file:///" + Path.GetFullPath(filePath));
        }

        GUILayout.Space(10);

        GUILayout.Label("Quick Note (will be appended):");
        devNotes = EditorGUILayout.TextArea(devNotes, GUILayout.Height(60));

        if (GUILayout.Button("Save Note"))
        {
            AppendNote(devNotes);
            devNotes = "";
        }
    }

    private void AppendNote(string note)
    {
        if (string.IsNullOrWhiteSpace(note)) return;

        string entry = $"- [{System.DateTime.Now}] {note}\n";
        File.AppendAllText(filePath, entry);

        Debug.Log("Note added to project_context.md");
    }

    // Proje build edildiğinde veya play mode başladığında otomatik kaydetme
    [InitializeOnLoadMethod]
    private static void Init()
    {
        EditorApplication.playModeStateChanged += (PlayModeStateChange state) =>
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                File.AppendAllText(filePath, $"\n- [{System.DateTime.Now}] Entered Play Mode\n");
            }
        };
    }
}
