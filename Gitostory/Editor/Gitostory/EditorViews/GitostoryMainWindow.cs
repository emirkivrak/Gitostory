using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using LibGit2Sharp;
using System;

namespace GitostorySpace
{

    /// <summary>
    /// GitostoryMainWindow is an EditorWindow in Unity that displays the version control history
    /// of a selected asset using Gitostory.
    /// </summary>
    public class GitostoryMainWindow : EditorWindow
    {
        // General Fields
        private string _absolutePath;
        private string _relativePath;
        private Gitostory _gitAPI;
        private GitostoryAssetPreviewController _previewController;

        // Commit Data
        private List<GitostoryPastCommitData> _history;
        private Dictionary<string, GitostoryPastCommitData> _commitHashDataMap = new Dictionary<string, GitostoryPastCommitData>();
        private GitostoryPastCommitData _inspectingCommit = new GitostoryPastCommitData();

        // Asset Information
        private string _assetName;
        private GitostorySupportedType _assetType;
        private GitostoryAssetSupportCondition _assetSupportCondition = GitostoryAssetSupportCondition.NotSupported;
        private bool _isPreviewSupported;

        // UI/Display Information
        private Vector2 _scrollPosition;
        private Vector2 _comparisonResultScrollView;
        private GitostoryAssetCommitDisplayBaseView _displayCommitView;
        private GitostoryActionState _actionState = GitostoryActionState.NoHistory;
        private string _statusCode;


        #region Unity Methods

        /// <summary>
        /// Static method to show the window.
        /// </summary>
        public static void ShowWindow()
        {
            GetWindow<GitostoryMainWindow>("Gitostory");
        }


        private void OnEnable()
        {
            if (string.IsNullOrEmpty(_relativePath) || _history == null)
            {
                LoadEditorState();
                Initialize(_absolutePath, _relativePath, _assetType);
            }
        }

        void OnGUI()
        {
            if (_actionState == GitostoryActionState.ShowHistory || _actionState == GitostoryActionState.InCommitPreview)
            {

                // Show the header
                GUILayout.Label($"Story of {_assetName}", GitostoryGUIExtensions.CreateHeaderStyle(Color.white));


                ShowFileStatusAndActions();

                CheckAndDrawComparison();

                // Inform user if preview is not supported
                //if (!_isPreviewSupported)
                //{
                //  GUILayout.Label("Preview is not supported in this asset type.", GitostoryGUIExtensions.CreateTextStyle(Color.red));
                // }

                // Start scroll of stories
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

                DisplayAllCommits();

                GUILayout.EndScrollView();
            }
            else if (_actionState == GitostoryActionState.NoHistory)
            {
                GUILayout.Label($"The beggining of the story for {_assetName}", GitostoryGUIExtensions.CreateHeaderStyle(Color.white));
                GUILayout.Label("If you think its not, please check repository settings from Window -> Gitostory ");
            }

            GUIUtility.ExitGUI();
        }


        private void OnDestroy()
        {
            GitostoryHelpers.CleanUpTemp();

            if (GitostoryPrefabComparisonManager.IsInComparison)
                ExitPrefabComparison();
        }



        #endregion

        #region Initialization of View
        internal void Initialize(string absolutePath, string relativePath, GitostorySupportedType fileType)
        {
            if (!string.IsNullOrEmpty(absolutePath))
            {
                _gitAPI = new Gitostory(GitostoryConfig.Paths.REPOSITORY_ROOT);
                _previewController = new GitostoryAssetPreviewController();

                FetchHistoryAndSetup(relativePath);

                _absolutePath = absolutePath;
                _relativePath = relativePath;
                _assetName = Path.GetFileName(absolutePath);

                FetchHistoryAndSetup(relativePath);
                SetupDisplayView();
                SaveEditorState();
            }
        }

        private void FetchHistoryAndSetup(string filePath)
        {
            _history = _gitAPI.GetAllCommits(filePath);
            foreach (var commit in _history)
            {
                _commitHashDataMap[commit.CommitHash] = commit;
            }
            _assetType = GitostoryHelpers.GetGitostoryTypeOfAsset(filePath);
            UpdateStatus(filePath);
        }

        private void SetupDisplayView()
        {
            _displayCommitView = new GitostoryAssetCommitDisplayBaseView(gitUtils: _gitAPI);
            _displayCommitView.OnPreviewAsset += PreviewCommit;

            if (_history != null && _history.Count > 0)
                _actionState = GitostoryActionState.ShowHistory;

            if (_assetType == GitostorySupportedType.Unsuported)
                _assetSupportCondition = GitostoryAssetSupportCondition.SupportedHistory;
            else
                _assetSupportCondition = GitostoryAssetSupportCondition.SupportedWithPreviewAndComparision;

            _isPreviewSupported = _assetSupportCondition == GitostoryAssetSupportCondition.SupportedWithPreview ||
                          _assetSupportCondition == GitostoryAssetSupportCondition.SupportedWithPreviewAndComparision;
        }

        private void SaveEditorState()
        {
            EditorPrefs.SetString("Gitostory_absolutePath", _absolutePath);
            EditorPrefs.SetString("Gitostory_relativePath", _relativePath);
            EditorPrefs.SetInt("Gitostory_fileType", (int)_assetType);
        }

        private void LoadEditorState()
        {
            if (EditorPrefs.HasKey("Gitostory_absolutePath"))
            {
                _absolutePath = EditorPrefs.GetString("Gitostory_absolutePath");
            }
            if (EditorPrefs.HasKey("Gitostory_relativePath"))
            {
                _relativePath = EditorPrefs.GetString("Gitostory_relativePath");
            }
            if (EditorPrefs.HasKey("Gitostory_fileType"))
            {
                _assetType = (GitostorySupportedType)EditorPrefs.GetInt("Gitostory_fileType");
            }
        }

        #endregion

        #region Commit Preview

        private void PreviewCommit(string commitHash, string filePath, GitostorySupportedType type)
        {
            _actionState = GitostoryActionState.InCommitPreview;
            _inspectingCommit = _commitHashDataMap[commitHash];

            var typeOfAsset = AssetDatabase.GetMainAssetTypeAtPath(filePath);

            var asset = AssetDatabase.LoadAssetAtPath(filePath, typeOfAsset);

            _previewController.PreviewAsset(_relativePath, filePath, type);
        }

        private void DisplayAllCommits()
        {
            foreach (GitostoryPastCommitData commit in _history)
            {
                var isInInspect = commit.CommitHash.Equals(_inspectingCommit.CommitHash);

                DisplayCommit(commit, isInInspect, _isPreviewSupported);
            }
        }

        private void DisplayCommit(GitostoryPastCommitData commit, bool isInInspect, bool isPreviewSupported)
        {
            _displayCommitView.ShowCommit(commit, _absolutePath, _relativePath, _assetType, isInInspect, isPreviewSupported);
        }

        #endregion

        #region Current File Status 

        private void UpdateStatus(string filePath)
        {
            _statusCode = _gitAPI.IsFileModified(filePath) ? "Modified" : "Unmodified";
        }

        private void ShowFileStatusAndActions()
        {
            Refresh();
            GUILayout.Space(10);
            EditorGUILayout.HelpBox($"Status: {_statusCode}", MessageType.Info);
            ShowResetUnstagedChangesButton();
        }

        private void ShowResetUnstagedChangesButton()
        {
            if (_statusCode.Contains("Modified"))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Reset Unstaged Changes"))
                {
                    TryResetUnstagedChanges();
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private void Refresh()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh"))
            {
                // Refresh current file
                Initialize(_absolutePath, _relativePath, _assetType);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void TryResetUnstagedChanges()
        {
            if (EditorUtility.DisplayDialog("Reset Unstaged Changes", "Are you sure you want to reset changes?", "Yes", "No"))
            {
                if (_gitAPI.GitResetSingleFile(_relativePath))
                {
                    AssetDatabase.Refresh();
                    UpdateStatus(_relativePath);
                    Repaint();
                }
            }
        }

        #endregion

        #region Prefab Comparison 


        private void CheckAndDrawComparison()
        {
            // Check if its in prefab comparison scene
            if (GitostoryPrefabComparisonManager.IsInComparison)
            {
                GUILayout.Label("You are in the prefab comparison scene.", GitostoryGUIExtensions.CreateTextStyle(Color.green));
                GUILayout.Label("Change insights", GitostoryGUIExtensions.CreateBoldTextStyle());

                var comparisonResult = GitostoryPrefabComparisonManager.GetComparisonResult();

                if (comparisonResult != null)
                {
                    GUILayout.Label("Comparison Result:", EditorStyles.boldLabel);

                    _comparisonResultScrollView = GUILayout.BeginScrollView(_comparisonResultScrollView, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.MinHeight(150f));

                    if (comparisonResult != null && comparisonResult.Count <= 1)
                    {
                        GUILayout.Label("No changes detected .. ", GitostoryGUIExtensions.CreateTextStyle(Color.green));
                    }

                    GUILayout.BeginVertical();
                    foreach (var result in comparisonResult)
                    {

                        if (result.MismatchType is GitostoryPrefabComparisonMismatchType.ValueMismatch)
                        {
                            if (result.PropertyName == "Local Position") continue;

                            GUILayout.Label($"Component has changes: {result.ComponentName}", GitostoryGUIExtensions.CreateBoldTextStyle());
                            GUILayout.Label($"Changed Property Name: {result.PropertyName}");
                            GUILayout.Label($"Value in Current Version: {result.ValueA} | Value in Selected Version: {result.ValueB}");
                        }
                        else if (result.MismatchType is GitostoryPrefabComparisonMismatchType.ComponentMismatch)
                        {
                            GUILayout.Label($"Component add/remove found.", GitostoryGUIExtensions.CreateBoldTextStyle());
                            if (result.ValueA == "Missing")
                                GUILayout.Label($"Component {result.ComponentName} is not exist in current version");
                            else
                                GUILayout.Label($"Component {result.ComponentName} is not exist in previous version");
                        }
                        else if (result.MismatchType is GitostoryPrefabComparisonMismatchType.HierarchyMismatch)
                        {
                            var number = GitostoryHelpers.ExtractNumberInBrackets(result.ComponentName);
                            GUILayout.Label($"Prefab hierarchy change found.", GitostoryGUIExtensions.CreateBoldTextStyle());
                            if (result.ValueA == "Missing")
                                GUILayout.Label($"Child {number}.th is not exist in current version");
                            else
                                GUILayout.Label($"Child {number}.th is not exist in previous version");
                        }
                        GUILayout.Space(10);
                    }
                    GUILayout.EndVertical();

                    // End the scroll view
                    GUILayout.EndScrollView();

                    if (GUILayout.Button("Exit Comparison"))
                    {
                        ExitPrefabComparison();
                    }
                }

            }

        }

        private void ExitPrefabComparison()
        {
            GitostoryPrefabComparisonManager.ExitComparison();
            GitostoryHelpers.CleanUpTemp();
            _inspectingCommit = new GitostoryPastCommitData();
        }

        #endregion
    }
}