using UnityEditor;
using UnityEditor.SceneManagement;

namespace SelfBulletting.Editor
{
    /// <summary>
    /// Play mode always starts from the first scene in Build Settings (Boot), whatever scene is open
    /// in the editor, because only Boot has the bootstrap that creates the app services.
    /// </summary>
    [InitializeOnLoad]
    internal static class BootSceneOnPlay
    {
        static BootSceneOnPlay()
        {
            EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
            EditorSceneManager.playModeStartScene = buildScenes.Length > 0
                ? AssetDatabase.LoadAssetAtPath<SceneAsset>(buildScenes[0].path)
                : null;
        }
    }
}
