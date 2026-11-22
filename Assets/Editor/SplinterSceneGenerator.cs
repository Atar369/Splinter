#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SplinterSceneGenerator
{
    [MenuItem("Splinter/Generate Play Scene")]
    public static void GeneratePlayScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        var camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.035f, 0.045f, 0.065f);
        camera.orthographic = true;
        camera.orthographicSize = 5f;

        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        var scenePath = "Assets/Scenes/Splinter.unity";
        EditorSceneManager.SaveScene(scene, scenePath);

        var buildSettings = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(scenePath, true)
        };
        EditorBuildSettings.scenes = buildSettings;

        EditorUtility.DisplayDialog(
            "Splinter",
            "The play scene was generated at Assets/Scenes/Splinter.unity.",
            "OK");
    }
}
#endif
