using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Collections.Generic;
using GitostorySpace;

internal sealed class GitostoryTextEditor : EditorWindow
{
    private string _diffText;
    private Vector2 _scrollPosition;

    // Paths to the script files
    private string _previousFilePath;
    private string _currentFilePath;

    private GUIStyle _richTextStyle;

    public static void ShowWindow()
    {
        GetWindow<GitostoryTextEditor>("Gitostory Text Comparison");
    }

    private void OnEnable()
    {
        _richTextStyle = new GUIStyle(EditorStyles.textArea) { richText = true };
    }

    private void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.Label("Gitostory recommends utilizing an IDE or advanced text editor with GIT support, which is likely more effective for handling diffs.", EditorStyles.boldLabel);
        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        EditorGUILayout.LabelField("Changes", EditorStyles.boldLabel);
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandHeight(true));

        EditorGUILayout.TextArea(_diffText, _richTextStyle, GUILayout.ExpandHeight(true));

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Open Previous In Default Editor"))
        {
            OpenInDefaultEditor(_previousFilePath);
        }
        if (GUILayout.Button("Open Current In Default Editor"))
        {
            OpenInDefaultEditor(_currentFilePath);
        }

        GUILayout.EndVertical();
    }

    private void OpenInDefaultEditor(string filePath)
    {
        Debug.Log(filePath);
        if (filePath.StartsWith("Assets/"))
        {
            Object asset = AssetDatabase.LoadMainAssetAtPath(filePath);

            if (asset != null)
            {
                AssetDatabase.OpenAsset(asset);
            }
            else
            {
                Debug.LogError($"Asset not found at path: {filePath}");
            }
        }
        else
        {
            if (File.Exists(filePath))
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo()
                {
                    FileName = filePath,
                    UseShellExecute = true
                };
                Process.Start(processStartInfo);
            }
            else
            {
                Debug.LogError($"File not found: {filePath}");
            }
        }
    }

    public void SetText(string currentText, string previousText, string currentFilePath, string previousFilePath)
    {
        _previousFilePath = previousFilePath;
        _currentFilePath = currentFilePath;

        _diffText = GenerateDiffText(previousText, currentText);
    }

    private string GenerateDiffText(string previousText, string currentText)
    {
        var diffs = GitostoryUtilDiff.DiffText(previousText, currentText);
        string diffText = "";

        foreach (var diff in diffs)
        {
            switch (diff.Type)
            {
                case GitostoryUtilDiff.DiffType.Inserted:
                    diffText += $"<color=green>+ {diff.Text}</color>\n";
                    break;
                case GitostoryUtilDiff.DiffType.Deleted:
                    diffText += $"<color=red>- {diff.Text}</color>\n";
                    break;
                case GitostoryUtilDiff.DiffType.Unchanged:
                    diffText += $"  {diff.Text}\n";
                    break;
            }
        }

        return diffText;
    }
}
