using UnityEngine;
using UnityEditor; // Required for EditorWindow
using System.IO; // For file operations
using GitostorySpace;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class GitostoryAssetPreviewController
{
    private GitostoryTextEditor _scriptShowWindow;

    public void Initialize()
    {
       
    }

    public void PreviewAsset(string currentVersionPath, string previousVersionPath, GitostorySupportedType typeOfAsset)
    {
        if (typeOfAsset == GitostorySupportedType.Script)
        {
            // Change the previous version path's extension to .txt
            previousVersionPath = Path.ChangeExtension(previousVersionPath, ".txt");

            // Correct way to get or create the window
            _scriptShowWindow = EditorWindow.GetWindow<GitostoryTextEditor>("Gitostory", true);

            // Read the current and previous version contents
            string currentVersionContent = File.ReadAllText(currentVersionPath);
            string previousVersionContent = File.ReadAllText(previousVersionPath);

            // Add the prominent line at the beginning of the previous version content
            string warningMessage = "############# GITOSTORY CONVERTED PREVIOUS VERSION TO TXT FOR PREVENTING COMPILE ERRORS ############# \n";
            previousVersionContent = warningMessage + previousVersionContent;

            // Write the modified previous version content back to the .txt file
            File.WriteAllText(previousVersionPath, previousVersionContent);

            // Set the text in the editor window and show it
            _scriptShowWindow.SetText(currentVersionContent, previousVersionContent, currentVersionPath, previousVersionPath);
            _scriptShowWindow.Show();

            // Change file's extension back to previous extension if needed
        }
        else if (typeOfAsset == GitostorySupportedType.Prefab)
        {
            GitostoryPrefabComparisonManager.PreviewPrefab(currentVersionPath, previousVersionPath);
        }
        else if (typeOfAsset == GitostorySupportedType.Scene)
        {
            // GitostorySceneComparisonManager.PreviewScene(currentVersionPath, previousVersionPath);


            // Load the previous version scene asset
            var previousSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(previousVersionPath);

            // Get the instance ID of the asset
            var instanceID = previousSceneAsset.GetInstanceID();

            // Open the asset
            AssetDatabase.OpenAsset(instanceID);

            // Get the currently open scene
            Scene openedScene = SceneManager.GetActiveScene();

            // Construct the new name with a "PreviousVersion_Old" suffix
            string newSceneName = $"{openedScene.name}_PreviousVersion_Old";

            // Save the scene with the new name
            EditorSceneManager.SaveScene(openedScene, $"{Path.GetDirectoryName(previousVersionPath)}/{newSceneName}.unity");

            // Refresh the AssetDatabase to reflect changes
            AssetDatabase.Refresh();

        }
        else
        {
            // Just open, without comparison 
            var instanceID = AssetDatabase.LoadAssetAtPath<Object>(previousVersionPath).GetInstanceID();
            AssetDatabase.OpenAsset(instanceID);
        }
    }

}
