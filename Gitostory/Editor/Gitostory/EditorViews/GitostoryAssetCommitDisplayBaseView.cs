using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GitostorySpace
{
    public class GitostoryAssetCommitDisplayBaseView
    {
        // commit hash, path of asset and its type
        public event Action<string, string, GitostorySupportedType> OnPreviewAsset;

        public Gitostory GitUtils;

        public bool _isInitialized;
        private Vector2 _scrollPosition;

       public GitostoryAssetCommitDisplayBaseView(Gitostory gitUtils)
        {
            if (_isInitialized) return;
            GitUtils = gitUtils;
            _isInitialized = true;
        }

        public void ShowCommit(GitostoryPastCommitData commitData, string absolutePath, string relativePath, GitostorySupportedType assetType, bool isInInspect, bool isPreviewSupported)
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true),GUILayout.MinHeight(120)) ;

            EditorGUILayout.BeginHorizontal("box");
            GUILayout.BeginVertical();

            GUIStyle boldStyle = new GUIStyle(EditorStyles.boldLabel);
            boldStyle.fontStyle = FontStyle.Bold;

            GUIStyle normalStyle = new GUIStyle(EditorStyles.label);

            if (isInInspect)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Selected", GitostoryGUIExtensions.CreateSubHeaderStyle(Color.green));
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Message:", boldStyle, GUILayout.Width(70));
            EditorGUILayout.LabelField(commitData.CommitMessage, normalStyle);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Author:", boldStyle, GUILayout.Width(70));
            EditorGUILayout.LabelField(commitData.Author, normalStyle);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Date:", boldStyle, GUILayout.Width(70));
            EditorGUILayout.LabelField(commitData.CommitDate, normalStyle);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Commit:", boldStyle, GUILayout.Width(70));
            EditorGUILayout.LabelField(commitData.CommitHash.Substring(0, 10) + "....", normalStyle);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.BeginVertical();

            // Always true for now, just opens file.
            isPreviewSupported = true;

            if (isPreviewSupported)
            {
                if (GUILayout.Button(GetBtnName(assetType), GitostoryGUIExtensions.CreateButtonStyle(Color.white), GUILayout.ExpandWidth(true)))
                {
                    PreviewCommit(commitData.CommitHash, absolutePath, relativePath, assetType == GitostorySupportedType.Script);
                    GUIUtility.ExitGUI();
                }
            }

            if (isPreviewSupported)
            {
                if (GUILayout.Button("Rollback", GitostoryGUIExtensions.CreateButtonStyle(Color.white), GUILayout.ExpandWidth(true)))
                {
                    // Confirmation dialog prompt
                    if (EditorUtility.DisplayDialog(
                            "Confirm Rollback " +commitData.CommitDate,
                            "Do you want to revert to the selected version? This will overwrite the current version with the selected one. Any unsaved changes will be lost.",
                            "Revert",
                            "Cancel"))
                    {
                        Rollback(commitData.CommitHash, relativePath);
                    }

                }
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            EditorGUILayout.EndScrollView();
        }


        private void Rollback(string commitHash, string relativePath)
        {
           GitUtils.Rollback(commitHash,relativePath);
           AssetDatabase.Refresh();
        }

        private string GetBtnName(GitostorySupportedType assetType)
        {
            if (assetType == GitostorySupportedType.Prefab)
            {
                return "Open In Prefab Comparison";
            }
            else if (assetType == GitostorySupportedType.Texture)
            {
                return "Show in Inspector";
            }
            else if (assetType == GitostorySupportedType.Material)
            {
                return "Show in Inspector";
            }
            else if (assetType == GitostorySupportedType.Scene)
            {
                return "Open Scene";
            }
            else if (assetType == GitostorySupportedType.Script)
            {
                return "Open In Script Comparison";
            }
            else if (assetType == GitostorySupportedType.Animation)
            {
                return "Open Animation";
            }
            else return "Open";
        }

        void PreviewCommit(string commitHash, string absolutePath, string relativePath,bool changeExtentionToTxt)
        {
            var nameOfFile = Path.GetFileName(absolutePath);

            // Implement error handling and feedback
            GitUtils.GetFilePreviousVersion(relativePath, commitHash,changeExtentionToTxt);


            AssetDatabase.Refresh();

            var loadedAssetPath = Path.Combine(GitostoryConfig.Paths.TEMP_ROOT, nameOfFile);

            if (changeExtentionToTxt)
            {
                loadedAssetPath = Path.ChangeExtension(loadedAssetPath, ".txt");
            }

            var gitostoryType = GitostoryHelpers.GetGitostoryTypeOfAsset(loadedAssetPath);

            OnPreviewAsset?.Invoke(commitHash,loadedAssetPath, gitostoryType);
        }
    }
}