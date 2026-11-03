using System.Collections.Generic;
using SelfBulletting.Gameplay.Levels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SelfBulletting.Editor.LevelGeneration
{
    /// <summary>
    /// Fills the Level of the open scene with obstacles. Every run replaces the previous obstacles and supports Undo,
    /// so a layout can be regenerated with another seed and then tweaked by hand.
    /// </summary>
    internal static class LevelGeneratorMenu
    {
        private const string MenuRoot = "SelfBulletting/Level/";
        private const string SettingsPath = "Assets/Project/Editor/LevelGeneration/LevelGenerationSettings.asset";
        private const string UndoName = "Generate Obstacles";

        [MenuItem(MenuRoot + "Generate Obstacles")]
        private static void Generate()
        {
            if (!TryGetLevel(out Level level) || !TryGetSettings(out LevelGenerationSettings settings))
            {
                return;
            }

            ClearObstacles(level);

            List<Vector3> positions = ObstacleLayout.Generate(settings.ToLayoutParameters(), level.StartPoint, level.FinishPoint);
            foreach (Vector3 position in positions)
            {
                var obstacle = (GameObject)PrefabUtility.InstantiatePrefab(settings.ObstaclePrefab, level.ObstaclesRoot);
                obstacle.transform.position = position;
                Undo.RegisterCreatedObjectUndo(obstacle, UndoName);
            }

            EditorSceneManager.MarkSceneDirty(level.gameObject.scene);
            Debug.Log($"[{nameof(LevelGeneratorMenu)}] Generated {positions.Count} obstacles.");
        }

        [MenuItem(MenuRoot + "Clear Obstacles")]
        private static void Clear()
        {
            if (!TryGetLevel(out Level level))
            {
                return;
            }

            ClearObstacles(level);
            EditorSceneManager.MarkSceneDirty(level.gameObject.scene);
        }

        [MenuItem(MenuRoot + "Select Generation Settings")]
        private static void SelectSettings()
        {
            if (TryGetSettings(out LevelGenerationSettings settings))
            {
                Selection.activeObject = settings;
            }
        }

        private static void ClearObstacles(Level level)
        {
            Transform root = level.ObstaclesRoot;
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Undo.DestroyObjectImmediate(root.GetChild(i).gameObject);
            }
        }

        private static bool TryGetLevel(out Level level)
        {
            level = Object.FindAnyObjectByType<Level>();
            if (level != null && level.ObstaclesRoot != null)
            {
                return true;
            }

            Debug.LogError($"[{nameof(LevelGeneratorMenu)}] Open the Game scene: it needs a Level with an obstacles root.");
            return false;
        }

        private static bool TryGetSettings(out LevelGenerationSettings settings)
        {
            settings = AssetDatabase.LoadAssetAtPath<LevelGenerationSettings>(SettingsPath);
            if (settings != null && settings.HasValidPrefab)
            {
                return true;
            }

            Debug.LogError($"[{nameof(LevelGeneratorMenu)}] {SettingsPath} is missing or has no obstacle prefab with an Obstacle component.");
            return false;
        }
    }
}
