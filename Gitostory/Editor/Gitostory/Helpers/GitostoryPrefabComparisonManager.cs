using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using GitostorySpace;
using System.Collections.Generic;

public class GitostoryPrefabComparisonManager : MonoBehaviour
{
    // Checks if the current scene is the comparison scene
    public static bool IsInComparison => SceneManager.GetActiveScene().name == "PrefabComparisonScene";

    private static List<ComparisonResult> _comparisonResult;

    /// <summary>
    /// Retrieves the comparison results.
    /// </summary>
    /// <returns>A list of comparison results.</returns>
    public static List<ComparisonResult> GetComparisonResult()
    {
        return _comparisonResult;
    }

    /// <summary>
    /// Previews the comparison between the current version of a prefab and a previous version.
    /// </summary>
    /// <param name="originalPrefabPath">The path to the current version of the prefab.</param>
    /// <param name="previousVersionPath">The path to the previous version of the prefab.</param>
    public static void PreviewPrefab(string originalPrefabPath, string previousVersionPath)
    {
        // Save the current scene path if not already in comparison scene
        Scene previousScene = SceneManager.GetActiveScene();
        if (previousScene.name != "PrefabComparisonScene")
        {
            EditorPrefs.SetString("PreviousScenePath", previousScene.path);
        }

        // Validate the asset paths
        if (!AssetExists(originalPrefabPath) || !AssetExists(previousVersionPath))
        {
            Debug.LogError("One or both prefab paths are invalid.");
            return;
        }

        // Create a new scene for comparison
        Scene comparisonScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        comparisonScene.name = "PrefabComparisonScene";

        // Load the prefabs and instantiate them
        GameObject originalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(originalPrefabPath);
        GameObject modifiedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(previousVersionPath);

        GameObject originalInstance = PrefabUtility.InstantiatePrefab(originalPrefab) as GameObject;
        GameObject modifiedInstance = PrefabUtility.InstantiatePrefab(modifiedPrefab) as GameObject;

        // Adjust positions to prevent overlapping
        AdjustPrefabPositions(originalInstance, modifiedInstance);

        // Add labels above prefabs
        CreateLabelAbovePrefab(originalInstance, "Current Version");
        CreateLabelAbovePrefab(modifiedInstance, "Previous Version");

        // Frame the view to fit both prefabs
        FrameSceneView(originalInstance, modifiedInstance);

        // Compare the prefabs and store the result
        GameObjectComparer gitostoryPrefabComparer = new GameObjectComparer();
        _comparisonResult = gitostoryPrefabComparer.ComparePrefabs(originalInstance, modifiedInstance);
    }

    /// <summary>
    /// Adjusts the positions of the prefabs to prevent overlapping.
    /// </summary>
    /// <param name="originalInstance">The current version of the prefab.</param>
    /// <param name="modifiedInstance">The previous version of the prefab.</param>
    private static void AdjustPrefabPositions(GameObject originalInstance, GameObject modifiedInstance)
    {
        var originalBounds = CalculateBounds(originalInstance);
        var modifiedBounds = CalculateBounds(modifiedInstance);

        float separationDistance = 5f; // Adjust as necessary for visual separation
        float totalWidth = originalBounds.size.x + modifiedBounds.size.x + separationDistance;

        originalInstance.transform.position = new Vector3(-totalWidth / 2 + originalBounds.extents.x, 0, 0);
        modifiedInstance.transform.position = new Vector3(totalWidth / 2 - modifiedBounds.extents.x, 0, 0);
    }

    /// <summary>
    /// Calculates the bounds of a game object.
    /// </summary>
    /// <param name="gameObject">The game object to calculate bounds for.</param>
    /// <returns>The bounds of the game object.</returns>
    private static Bounds CalculateBounds(GameObject gameObject)
    {
        var rendererComponents = gameObject.GetComponentsInChildren<Renderer>();
        if (rendererComponents.Length == 0) return new Bounds(gameObject.transform.position, Vector3.zero);

        var bounds = rendererComponents[0].bounds;
        foreach (var renderer in rendererComponents)
        {
            bounds.Encapsulate(renderer.bounds);
        }

        return bounds;
    }

    /// <summary>
    /// Creates a label above the specified prefab.
    /// </summary>
    /// <param name="prefab">The prefab to create a label for.</param>
    /// <param name="label">The label text.</param>
    private static void CreateLabelAbovePrefab(GameObject prefab, string label)
    {
        var labelObject = new GameObject(label);
        var textMesh = labelObject.AddComponent<TextMesh>();
        textMesh.text = label;
        textMesh.characterSize = 0.3f;
        textMesh.anchor = TextAnchor.LowerCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = Color.white;

        var prefabRenderers = prefab.GetComponentsInChildren<Renderer>();
        float maxY = float.MinValue;
        foreach (Renderer renderer in prefabRenderers)
        {
            if (renderer.bounds.max.y > maxY)
            {
                maxY = renderer.bounds.max.y;
            }
        }

        labelObject.transform.position = maxY != float.MinValue ?
            new Vector3(prefab.transform.position.x, maxY + 1f, prefab.transform.position.z) :
            new Vector3(prefab.transform.position.x, prefab.transform.position.y + 1f, prefab.transform.position.z);
    }

    /// <summary>
    /// Checks if an asset exists at the specified path.
    /// </summary>
    /// <param name="path">The path to the asset.</param>
    /// <returns>True if the asset exists, false otherwise.</returns>
    private static bool AssetExists(string path)
    {
        return AssetDatabase.LoadAssetAtPath<GameObject>(path) != null;
    }

    /// <summary>
    /// Frames the scene view to fit both prefabs.
    /// </summary>
    /// <param name="originalInstance">The current version of the prefab.</param>
    /// <param name="modifiedInstance">The previous version of the prefab.</param>
    private static void FrameSceneView(GameObject originalInstance, GameObject modifiedInstance)
    {
        var originalBounds = CalculateBounds(originalInstance);
        var modifiedBounds = CalculateBounds(modifiedInstance);
        float totalWidth = originalBounds.size.x + modifiedBounds.size.x + 5f;
        SceneView.lastActiveSceneView.Frame(new Bounds(Vector3.zero, new Vector3(totalWidth, 5, 5)), false);
    }

    /// <summary>
    /// Exits the comparison mode and returns to the previous scene.
    /// </summary>
    internal static void ExitComparison()
    {
        var scenePath = EditorPrefs.GetString("PreviousScenePath");
        EditorSceneManager.OpenScene(scenePath);    
    }
}
