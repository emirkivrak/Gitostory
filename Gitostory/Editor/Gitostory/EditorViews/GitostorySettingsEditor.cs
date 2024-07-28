using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using LibGit2Sharp;

namespace GitostorySpace
{
    public class GitostorySettingsEditor : EditorWindow
    {
        private string tempRoot;
        private string repositoryRoot;
        private bool isRepositoryValid;

        private const string GITIGNORE_FILE_NAME = ".gitignore";

        [MenuItem("Window/Gitostory/Gitostory Settings")]
        public static void ShowWindow()
        {
            GetWindow<GitostorySettingsEditor>("Gitostory Settings");
        }

        private void OnEnable()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            tempRoot = GitostoryConfig.Paths.TEMP_ROOT;
            repositoryRoot = GitostoryConfig.Paths.REPOSITORY_ROOT;
            isRepositoryValid = IsValidRepository(repositoryRoot);
        }

        private void OnGUI()
        {
            GUILayout.Label("Gitostory Configuration", EditorStyles.boldLabel);

            DrawTempRootField();
            DrawRepositoryRootField();
            DrawValidationMessage();

            GUILayout.Space(10);

            if (GUILayout.Button("Save"))
            {
                SaveConfiguration();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Restore Defaults"))
            {
                RestoreDefaults();
            }


            GUILayout.Space(10);

            if (GUILayout.Button("Add Gitostory Temp folder to .gitignore"))
            {
                AddTempFolderToGitignore();
            }

        }

        private void DrawTempRootField()
        {
            GUILayout.Label("Temporary Root Folder", EditorStyles.label);
            tempRoot = EditorGUILayout.TextField(tempRoot);
            if (GUILayout.Button("Select Temp Folder"))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("Select Temp Folder", tempRoot, "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    tempRoot = selectedPath;
                }
            }
        }

        private void DrawRepositoryRootField()
        {
            GUILayout.Label("Repository Root Folder", EditorStyles.label);
            repositoryRoot = EditorGUILayout.TextField(repositoryRoot);
            if (GUILayout.Button("Select Repository Folder"))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("Select Repository Folder", repositoryRoot, "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    repositoryRoot = selectedPath;
                    isRepositoryValid = IsValidRepository(repositoryRoot);
                }
            }
        }

        private void DrawValidationMessage()
        {
            if (!isRepositoryValid)
            {
                EditorGUILayout.HelpBox("The selected repository path is not valid.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox("The selected repository path is valid.", MessageType.Info);
            }
        }

        private void SaveConfiguration()
        {
            GitostoryConfig.Paths.TEMP_ROOT = tempRoot;
            GitostoryConfig.Paths.REPOSITORY_ROOT = repositoryRoot;
            isRepositoryValid = IsValidRepository(repositoryRoot);

            if (!Directory.Exists(tempRoot))
            {
                Directory.CreateDirectory(tempRoot);
            }

            EditorUtility.DisplayDialog("Gitostory Config", "Configuration Saved", "OK");
        }

        private void RestoreDefaults()
        {
            EditorPrefs.DeleteKey(GitostoryConfig.Paths.TEMP_ROOT_KEY_DEFAULT);
            EditorPrefs.DeleteKey(GitostoryConfig.Paths.REPOSITORY_ROOT_KEY_DEFAULT);
            LoadSettings();
            EditorUtility.DisplayDialog("Gitostory Config", "Defaults Restored", "OK");
        }

        private void AddTempFolderToGitignore()
        {
            string gitignorePath = Path.Combine("", GITIGNORE_FILE_NAME) ;
            if (!File.Exists(gitignorePath))
            {
                Debug.Log(".gitignore is not found, creating new.");
                File.WriteAllText(gitignorePath, GitostoryConfig.Paths.TEMP_ROOT + Environment.NewLine);
            }
            else
            {
                var lines = File.ReadAllLines(gitignorePath);
                if (!Array.Exists(lines, line => line.Trim() == GitostoryConfig.Paths.TEMP_ROOT.Trim()))
                {
                    Debug.Log(".gitigore is found, updating.");
                    File.AppendAllText(gitignorePath, GitostoryConfig.Paths.TEMP_ROOT + Environment.NewLine);
                }
                else
                {
                    Debug.Log("Temp folder is already in .gitignore");
                }
            }
        }

        private bool IsValidRepository(string path)
        {
            try
            {
                using (var repo = new Repository(path))
                {
                    return repo != null && !repo.Info.IsBare;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Invalid repository path: {ex.Message}");
                return false;
            }
        }
    }
}
