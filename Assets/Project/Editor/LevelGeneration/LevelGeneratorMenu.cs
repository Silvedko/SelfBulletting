using SelfBulletting.Gameplay.Levels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SelfBulletting.Editor.LevelGeneration
{
    /// <summary>
    /// Level tools. A level is a prefab with a <see cref="Level"/> on its root; the catalog sets the play order.
    /// Generate and Clear work on the level open in Prefab Mode (or on a Level in the open scene) and support Undo.
    /// </summary>
    internal static class LevelGeneratorMenu
    {
        private const string MenuRoot = "SelfBulletting/Level/";
        private const string SettingsPath = "Assets/Project/Editor/LevelGeneration/LevelGenerationSettings.asset";
        private const string CatalogPath = "Assets/Project/Configs/LevelCatalog.asset";
        private const string LevelsFolder = "Assets/Project/Levels";
        private const string CatalogLevelsField = "_levels";

        private static readonly Vector3 _defaultStart = new Vector3(2f, 0f, 2f);
        private static readonly Vector3 _defaultFinish = new Vector3(10f, 0f, 22f);

        [MenuItem(MenuRoot + "Create New Level")]
        private static void CreateLevel()
        {
            if (!TryGetSettings(out LevelGenerationSettings settings) || !TryGetCatalog(out LevelCatalog catalog))
            {
                return;
            }

            int number = catalog.Count + 1;
            string path = AssetDatabase.GenerateUniqueAssetPath($"{LevelsFolder}/Level_{number:00}.prefab");

            // Built in a preview scene, so the open scene stays untouched.
            Scene preview = EditorSceneManager.NewPreviewScene();
            Level level = BuildEmptyLevel(System.IO.Path.GetFileNameWithoutExtension(path), catalog, preview);
            int seed = settings.Seed + catalog.Count;
            int obstacles = ObstacleGenerator.Fill(level, settings, seed, recordUndo: false);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(level.gameObject, path);
            EditorSceneManager.ClosePreviewScene(preview);

            AddToCatalog(catalog, prefab.GetComponent<Level>());
            AssetDatabase.OpenAsset(prefab);
            Debug.Log($"[{nameof(LevelGeneratorMenu)}] Created {path} with {obstacles} obstacles (seed {seed}) as level {number}.");
        }

        [MenuItem(MenuRoot + "Generate Obstacles")]
        private static void Generate()
        {
            if (!TryGetEditedLevel(out Level level, out Scene scene) || !TryGetSettings(out LevelGenerationSettings settings))
            {
                return;
            }

            int obstacles = ObstacleGenerator.Fill(level, settings, settings.Seed, recordUndo: true);
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log($"[{nameof(LevelGeneratorMenu)}] Generated {obstacles} obstacles (seed {settings.Seed}) in {level.name}.");
        }

        [MenuItem(MenuRoot + "Clear Obstacles")]
        private static void Clear()
        {
            if (!TryGetEditedLevel(out Level level, out Scene scene))
            {
                return;
            }

            ObstacleGenerator.Clear(level, recordUndo: true);
            EditorSceneManager.MarkSceneDirty(scene);
        }

        [MenuItem(MenuRoot + "Select Generation Settings")]
        private static void SelectSettings()
        {
            if (TryGetSettings(out LevelGenerationSettings settings))
            {
                Selection.activeObject = settings;
            }
        }

        [MenuItem(MenuRoot + "Select Level Catalog")]
        private static void SelectCatalog()
        {
            if (TryGetCatalog(out LevelCatalog catalog))
            {
                Selection.activeObject = catalog;
            }
        }

        // New levels copy the start and finish of the first level, so every level fits the same camera and ground.
        private static Level BuildEmptyLevel(string name, LevelCatalog catalog, Scene scene)
        {
            Vector3 start = catalog.Count > 0 ? catalog.Get(0).StartPoint : _defaultStart;
            Vector3 finish = catalog.Count > 0 ? catalog.Get(0).FinishPoint : _defaultFinish;

            var root = new GameObject(name);
            SceneManager.MoveGameObjectToScene(root, scene);
            Level level = root.AddComponent<Level>();

            var serialized = new SerializedObject(level);
            serialized.FindProperty("_start").objectReferenceValue = CreateChild(root.transform, "Start", start);
            serialized.FindProperty("_finish").objectReferenceValue = CreateChild(root.transform, "Finish", finish);
            serialized.FindProperty("_obstaclesRoot").objectReferenceValue = CreateChild(root.transform, "Obstacles", Vector3.zero);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return level;
        }

        private static Transform CreateChild(Transform parent, string name, Vector3 position)
        {
            Transform child = new GameObject(name).transform;
            child.SetParent(parent, false);
            child.position = position;
            return child;
        }

        private static void AddToCatalog(LevelCatalog catalog, Level level)
        {
            var serialized = new SerializedObject(catalog);
            SerializedProperty levels = serialized.FindProperty(CatalogLevelsField);
            levels.arraySize++;
            levels.GetArrayElementAtIndex(levels.arraySize - 1).objectReferenceValue = level;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
        }

        private static bool TryGetEditedLevel(out Level level, out Scene scene)
        {
            PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null && stage.prefabContentsRoot.TryGetComponent(out level))
            {
                scene = stage.scene;
                return true;
            }

            level = Object.FindAnyObjectByType<Level>();
            scene = level != null ? level.gameObject.scene : default;
            if (level != null && level.ObstaclesRoot != null)
            {
                return true;
            }

            Debug.LogError($"[{nameof(LevelGeneratorMenu)}] Open a level prefab from {LevelsFolder} in Prefab Mode first.");
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

        private static bool TryGetCatalog(out LevelCatalog catalog)
        {
            catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath);
            if (catalog != null)
            {
                return true;
            }

            Debug.LogError($"[{nameof(LevelGeneratorMenu)}] {CatalogPath} is missing.");
            return false;
        }
    }
}
