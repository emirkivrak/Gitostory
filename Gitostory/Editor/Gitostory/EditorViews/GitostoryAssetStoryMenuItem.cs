using UnityEditor;
using UnityEngine;

namespace GitostorySpace
{
    public static class GitostoryAssetStoryMenuItem
    {
        [MenuItem("Assets/Gitostory/Show Story", priority = 20)]
        private static void ShowGitHistory()
        {
            Object selectedObject = Selection.activeObject;
            if (selectedObject != null)
            {
                string path = AssetDatabase.GetAssetPath(selectedObject);
                string fullPath = System.IO.Path.GetFullPath(path);

                GitostorySupportedType type = GitostoryHelpers.GetGitostoryTypeOfAsset(path);

                GitostoryMainWindow window = (GitostoryMainWindow)EditorWindow.GetWindow(typeof(GitostoryMainWindow));
                window.Initialize(fullPath, path, type);
                window.Show();
            }
        }
    }
}