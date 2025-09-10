using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using System;

public class AIAssistant : EditorWindow
{
    private string userPrompt = "";
    private static string contextPath = "AI_Project_Context";
    private static string promptFile = "AI_Project_Context/ai_prompt.txt";
    private static string responseFile = "AI_Project_Context/ai_response.json";

    [MenuItem("AI Tools/AI Assistant")]
    public static void ShowWindow()
    {
        GetWindow<AIAssistant>("AI Assistant");
    }

    private void OnGUI()
    {
        GUILayout.Label("Talk to AI Assistant", EditorStyles.boldLabel);
        GUILayout.Space(5);

        userPrompt = EditorGUILayout.TextArea(userPrompt, GUILayout.Height(60));

        if (GUILayout.Button("Send to AI"))
        {
            SavePrompt(userPrompt);
            RunAI();
        }

        if (GUILayout.Button("Execute Last AI Response"))
        {
            if (File.Exists(responseFile))
            {
                AICommandExecutor.ExecuteCommands();
            }
            else
            {
                EditorUtility.DisplayDialog("AI Assistant", "No response file found!", "OK");
            }
        }
    }

    private void SavePrompt(string prompt)
    {
        if (!Directory.Exists(contextPath))
            Directory.CreateDirectory(contextPath);

        File.WriteAllText(promptFile, prompt);
        UnityEngine.Debug.Log("Prompt saved: " + promptFile);
    }

    private void RunAI()
    {
        // Burada dışarıdaki bir AI işlemine bağlanıyoruz (Python scripti vs.)
        // Şimdilik sadece kullanıcıya dosyayı verdiğini söylüyoruz
        EditorUtility.DisplayDialog(
            "AI Assistant",
            "Prompt saved.\nNow run the external AI bridge to get response.",
            "OK"
        );
    }
}
