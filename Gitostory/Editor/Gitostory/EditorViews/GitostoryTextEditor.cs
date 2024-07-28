using UnityEngine;
using UnityEditor;
using System.IO;
using Unity.VisualScripting;
using GitostorySpace;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

internal sealed class GitostoryTextEditor : EditorWindow
{
    private string _leftWindowText;
    private string _rightWindowText;
    private Vector2 _leftScrollPosition;
    private Vector2 _rightScrollPosition;
    private bool _currentVersionModified = false;

    private readonly string _leftWindowHeader = "Previous Version";
    private readonly string _rightWindowHeader = "Current Version";

    // Paths to the script files
    private string _previousFilePath;
    private string _currentFilePath;

    private string _previousLeftText = "";
    private string _highlightedLeftText = "";
    private string _previousRightText = "";
    private string _highlightedRightText = "";
    private GUIStyle _richTextStyle;



    public static void ShowWindow()
    {
        GetWindow<GitostoryTextEditor>("Gitostory Text Cpm");
    }

    private void OnEnable()
    {
    }



    private void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.Label("Gitostory recommends utilizing an IDE or advanced text editor with GIT support, which is likely more effective for handling diffs..", GitostoryGUIExtensions.CreateBoldTextStyle());
        GUILayout.EndVertical();

        GUILayout.BeginHorizontal();

        // Left Text Area
        GUILayout.BeginVertical();
        EditorGUILayout.LabelField(_leftWindowHeader, EditorStyles.boldLabel);
        _leftScrollPosition = EditorGUILayout.BeginScrollView(_leftScrollPosition, GUILayout.Width(position.width / 2 - 10), GUILayout.ExpandHeight(true));

        _leftWindowText = EditorGUILayout.TextArea(_leftWindowText, _richTextStyle, GUILayout.ExpandHeight(true));
 
        _leftWindowText = _highlightedLeftText; // Display highlighted text

        EditorGUILayout.EndScrollView();
        if (GUILayout.Button("Open Previous In Default Editor"))
        {
            OpenInDefaultEditor(_previousFilePath);
        }
        GUILayout.EndVertical();

        // Right Text Area
        GUILayout.BeginVertical();
        EditorGUILayout.LabelField(_rightWindowHeader, EditorStyles.boldLabel);
        _rightScrollPosition = EditorGUILayout.BeginScrollView(_rightScrollPosition, GUILayout.Width(position.width / 2 - 10), GUILayout.ExpandHeight(true));

        _rightWindowText = EditorGUILayout.TextArea(_rightWindowText, _richTextStyle, GUILayout.ExpandHeight(true));
        _rightWindowText = _highlightedRightText;

        EditorGUILayout.EndScrollView();
        if (GUILayout.Button("Open Current In Default Editor"))
        {
            OpenInDefaultEditor(_currentFilePath);
        }
        if (_currentVersionModified && GUILayout.Button("Save Changes"))
        {
            SaveCurrentVersion();
        }
  
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
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
            if (System.IO.File.Exists(filePath))
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

    private void SaveCurrentVersion()
    { 
        if (!string.IsNullOrEmpty(_currentFilePath))
        {
            File.WriteAllText(_currentFilePath, _rightWindowText);
            _currentVersionModified = false; // Reset modification flag
        }
        else
        {
            Debug.LogWarning("Cannot save: No file path specified for current version.");
        }
    }


    public void SetText(string currentText, string previousText, string currentFilePath, string previousFilePath)
    {
  
        _richTextStyle = new GUIStyle(EditorStyles.textArea) { richText = true };

        _leftWindowText = previousText;
        _rightWindowText = currentText;
        _previousFilePath = previousFilePath;
        _currentFilePath = currentFilePath;

        _previousLeftText = _leftWindowText;
        _previousRightText = _rightWindowText;
    }

}
