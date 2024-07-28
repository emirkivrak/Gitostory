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
        private string gitignorePath;
        private bool isRepositoryValid;
        private bool resetWithMetaFile;
        private bool trackRenamesAndFolderChanges;


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
            gitignorePath = GitostoryConfig.Paths.GITIGNORE_PATH;
            resetWithMetaFile = GitostoryConfig.Preferences.ResetWithMetaFile;
            trackRenamesAndFolderChanges = GitostoryConfig.Preferences.TrackRenamesAndFolderChanges;
            isRepositoryValid = IsValidRepository(repositoryRoot);
        }

        private void OnGUI()
        {
            GUILayout.Label("Gitostory Configuration", EditorStyles.boldLabel);

            DrawTempRootField();
            DrawRepositoryRootField();
            DrawGitignorePathField();
            DrawValidationMessage();
            DrawResetWithMetaFileOption();
            DrawTrackRenamesAndFolderChangesOption();

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
                Gitostory.AddToGitignore(GitostoryConfig.Paths.TEMP_ROOT);
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

        private void DrawGitignorePathField()
        {
            GUILayout.Label("Gitignore Relative File Path", EditorStyles.label);
            gitignorePath = EditorGUILayout.TextField(gitignorePath);
            if (GUILayout.Button("Select Gitignore File"))
            {
                string selectedPath = EditorUtility.OpenFilePanel("Select Gitignore File", gitignorePath, "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    gitignorePath = selectedPath;
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

        private void DrawResetWithMetaFileOption()
        {
            GUILayout.Label("Reset with Meta File", EditorStyles.label);
            resetWithMetaFile = EditorGUILayout.Toggle(resetWithMetaFile);
        }

        private void DrawTrackRenamesAndFolderChangesOption()
        {
            GUILayout.Label("Track Renames and Folder Changes", EditorStyles.label);
            trackRenamesAndFolderChanges = EditorGUILayout.Toggle(trackRenamesAndFolderChanges);
        }

        private void SaveConfiguration()
        {
            GitostoryConfig.Paths.TEMP_ROOT = tempRoot;
            GitostoryConfig.Paths.REPOSITORY_ROOT = repositoryRoot;
            GitostoryConfig.Paths.GITIGNORE_PATH = gitignorePath;
            GitostoryConfig.Preferences.ResetWithMetaFile = resetWithMetaFile;
            GitostoryConfig.Preferences.TrackRenamesAndFolderChanges = trackRenamesAndFolderChanges;
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
            EditorPrefs.DeleteKey(GitostoryConfig.Paths.GITIGNORE_PATH_KEY_DEFAULT);
            EditorPrefs.DeleteKey(GitostoryConfig.Preferences.RESET_WITH_META_FILE_KEY_DEFAULT);
            EditorPrefs.DeleteKey(GitostoryConfig.Preferences.TRACK_RENAMES_AND_FOLDER_CHANGES_KEY_DEFAULT);
            LoadSettings();
            EditorUtility.DisplayDialog("Gitostory Config", "Defaults Restored", "OK");
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
