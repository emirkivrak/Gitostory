using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using GitostorySpace;
using System.Collections.Generic;
using System.Linq;

public class GitostorySceneComparisonManager : MonoBehaviour
{
    // Checks if the current scene is the comparison scene
    public static bool IsInComparison => SceneManager.GetActiveScene().name == "SceneComparison";

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
    /// Previews the comparison between the current version of a scene and a previous version.
    /// </summary>
    /// <param name="currentVersionPath">The path to the current version of the scene.</param>
    /// <param name="previousVersionPath">The path to the previous version of the scene.</param>
    public static void PreviewScene(string currentVersionPath, string previousVersionPath)
    {
        // Save the current scene path if not already in comparison scene
        Scene previousScene = SceneManager.GetActiveScene();
        if (previousScene.name != "SceneComparison")
        {
            EditorPrefs.SetString("PreviousScenePath", previousScene.path);
        }

        // Validate the scene paths
        if (!SceneExists(currentVersionPath) || !SceneExists(previousVersionPath))
        {
            Debug.LogError("One or both scene paths are invalid.");
            return;
        }

        // Create a new scene for comparison
        Scene comparisonScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        comparisonScene.name = "SceneComparison";

        // Load the scenes
        Scene currentVersionScene = EditorSceneManager.OpenScene(currentVersionPath, OpenSceneMode.Additive);
        Scene previousVersionScene = EditorSceneManager.OpenScene(previousVersionPath, OpenSceneMode.Additive);

        // Rename the loaded scenes
        RenameScene(currentVersionScene, "CurrentVersion");
        RenameScene(previousVersionScene, "PreviousVersion_Old");

        // Get root game objects
        GameObject[] currentVersionRoots = currentVersionScene.GetRootGameObjects();
        GameObject[] previousVersionRoots = previousVersionScene.GetRootGameObjects();

        // Adjust positions to prevent overlapping
        AdjustScenePositions(currentVersionRoots, previousVersionRoots);

        // Add labels above root game objects
        CreateLabelsAboveRootObjects(currentVersionRoots, "Current Version");
        CreateLabelsAboveRootObjects(previousVersionRoots, "Previous Version");

        // Frame the view to fit both scenes
        FrameSceneView(currentVersionRoots, previousVersionRoots);
    }

    /// <summary>
    /// Adjusts the positions of the root game objects to prevent overlapping.
    /// </summary>
    /// <param name="currentVersionRoots">The root game objects of the current version.</param>
    /// <param name="previousVersionRoots">The root game objects of the previous version.</param>
    private static void AdjustScenePositions(GameObject[] currentVersionRoots, GameObject[] previousVersionRoots)
    {
        var currentBounds = CalculateBounds(currentVersionRoots);
        var previousBounds = CalculateBounds(previousVersionRoots);

        float separationDistance = 5f; // Adjust as necessary for visual separation
        float totalWidth = currentBounds.size.x + previousBounds.size.x + separationDistance;

        foreach (var root in currentVersionRoots)
        {
            root.transform.position = new Vector3(-totalWidth / 2 + currentBounds.extents.x, 0, 0);
        }
        foreach (var root in previousVersionRoots)
        {
            root.transform.position = new Vector3(totalWidth / 2 - previousBounds.extents.x, 0, 0);
        }
    }

    /// <summary>
    /// Calculates the bounds of multiple game objects.
    /// </summary>
    /// <param name="gameObjects">The game objects to calculate bounds for.</param>
    /// <returns>The bounds of the game objects.</returns>
    private static Bounds CalculateBounds(GameObject[] gameObjects)
    {
        if (gameObjects.Length == 0) return new Bounds(Vector3.zero, Vector3.zero);

        var rendererComponents = gameObjects.SelectMany(go => go.GetComponentsInChildren<Renderer>()).ToArray();
        if (rendererComponents.Length == 0) return new Bounds(Vector3.zero, Vector3.zero);

        var bounds = rendererComponents[0].bounds;
        foreach (var renderer in rendererComponents)
        {
            bounds.Encapsulate(renderer.bounds);
        }

        return bounds;
    }

    /// <summary>
    /// Creates labels above the specified root game objects.
    /// </summary>
    /// <param name="roots">The root game objects to create labels for.</param>
    /// <param name="label">The label text.</param>
    private static void CreateLabelsAboveRootObjects(GameObject[] roots, string label)
    {
        foreach (var root in roots)
        {
            var labelObject = new GameObject(label);
            var textMesh = labelObject.AddComponent<TextMesh>();
            textMesh.text = label;
            textMesh.characterSize = 0.3f;
            textMesh.anchor = TextAnchor.LowerCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = Color.white;

            var renderers = root.GetComponentsInChildren<Renderer>();
            float maxY = renderers.Max(renderer => renderer.bounds.max.y);

            labelObject.transform.position = new Vector3(root.transform.position.x, maxY + 1f, root.transform.position.z);
        }
    }

    /// <summary>
    /// Checks if a scene exists at the specified path.
    /// </summary>
    /// <param name="path">The path to the scene.</param>
    /// <returns>True if the scene exists, false otherwise.</returns>
    private static bool SceneExists(string path)
    {
        return AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null;
    }

    /// <summary>
    /// Frames the scene view to fit both scenes.
    /// </summary>
    /// <param name="currentVersionRoots">The root game objects of the current version.</param>
    /// <param name="previousVersionRoots">The root game objects of the previous version.</param>
    private static void FrameSceneView(GameObject[] currentVersionRoots, GameObject[] previousVersionRoots)
    {
        var currentBounds = CalculateBounds(currentVersionRoots);
        var previousBounds = CalculateBounds(previousVersionRoots);
        float totalWidth = currentBounds.size.x + previousBounds.size.x + 5f;
        SceneView.lastActiveSceneView.Frame(new Bounds(Vector3.zero, new Vector3(totalWidth, 5, 5)), false);
    }

    /// <summary>
    /// Renames a loaded scene.
    /// </summary>
    /// <param name="scene">The scene to rename.</param>
    /// <param name="newName">The new name for the scene.</param>
    private static void RenameScene(Scene scene, string newName)
    {
        EditorSceneManager.SaveScene(scene, $"{scene.path.Replace(".unity", "")}_{newName}.unity");
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
