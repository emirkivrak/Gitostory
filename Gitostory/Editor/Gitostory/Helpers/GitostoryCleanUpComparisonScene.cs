using UnityEditor.SceneManagement;
using UnityEngine;

public class GitostoryCleanUpComparisonScene : MonoBehaviour
{
    public string PreviousScene;

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 200, 40), "Cleanup and Return"))
        {
            CleanupAndReturn();
        }
    }

    private void CleanupAndReturn()
    {
        if (!string.IsNullOrEmpty(PreviousScene))
        {
            EditorSceneManager.OpenScene(PreviousScene);
        }
        else
        {
            Debug.LogError("Previous scene path is invalid or not set.");
        }
    }
}