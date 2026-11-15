using System.Text.RegularExpressions;
using SelfBulletting.Gameplay.Levels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SelfBulletting.Editor.LevelGeneration
{
    /// <summary>
    /// Level tools. A level is a prefab with a <see cref="Level"/> on its root; the catalogs set the play order:
    /// one for Classic, one with the curved levels of the aim modes. Generate and Clear work on the level open in
    /// Prefab Mode (or on a Level in the open scene) and support Undo.
    /// </summary>
    internal static class LevelGeneratorMenu
    {
        private const string MenuRoot = "SelfBulletting/Level/";
        private const string SettingsPath = "Assets/Project/Editor/LevelGeneration/LevelGenerationSettings.asset";
        private const string CatalogPath = "Assets/Project/Configs/LevelCatalog.asset";
        private const string CurvedCatalogPath = "Assets/Project/Configs/CurvedLevelCatalog.asset";
        private const string LevelsFolder = "Assets/Project/Levels";
        private const string CatalogLevelsField = "_levels";

        // Curved levels take their seeds from another range, so they never repeat a Classic layout.
        private const int CurvedSeedOffset = 100;

        private static readonly Vector3 _defaultStart = new Vector3(2f, 0f, 2f);
        private static readonly Vector3 _defaultFinish = new Vector3(10f, 0f, 22f);
        private static readonly Regex _levelNumber = new Regex(@"^Level_(\d+)$");

        [MenuItem(MenuRoot + "Create New Level")]
        private static void CreateLevel()
        {
            if (TryGetSettings(out LevelGenerationSettings settings) && TryGetCatalog(CatalogPath, out LevelCatalog catalog))
            {
                CreateLevel(settings, catalog, settings.Seed + catalog.Count, isCurved: false);
            }
        }

        [MenuItem(MenuRoot + "Create New Curved Level")]
        private static void CreateCurvedLevel()
        {
            if (TryGetSettings(out LevelGenerationSettings settings) && TryGetCatalog(CurvedCatalogPath, out LevelCatalog catalog))
            {
                CreateLevel(settings, catalog, settings.Seed + CurvedSeedOffset + catalog.Count, isCurved: true);
            }
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

        [MenuItem(MenuRoot + "Generate Path")]
        private static void GeneratePath()
        {
            if (!TryGetEditedLevel(out Level level, out Scene scene) || !TryGetSettings(out LevelGenerationSettings settings))
            {
                return;
            }

            int waypoints = PathGenerator.Fill(level, settings.ToPathParameters(settings.Seed, settings.BendCount), recordUndo: true);
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log($"[{nameof(LevelGeneratorMenu)}] Generated a path through {waypoints} waypoints (seed {settings.Seed}) in {level.name}.");
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
            if (TryGetCatalog(CatalogPath, out LevelCatalog catalog))
            {
                Selection.activeObject = catalog;
            }
        }

        [MenuItem(MenuRoot + "Select Curved Level Catalog")]
        private static void SelectCurvedCatalog()
        {
            if (TryGetCatalog(CurvedCatalogPath, out LevelCatalog catalog))
            {
                Selection.activeObject = catalog;
            }
        }

        private static void CreateLevel(LevelGenerationSettings settings, LevelCatalog catalog, int seed, bool isCurved)
        {
            int number = GetNextLevelNumber();
            string path = AssetDatabase.GenerateUniqueAssetPath($"{LevelsFolder}/Level_{number:00}.prefab");

            // Built in a preview scene, so the open scene stays untouched.
            Scene preview = EditorSceneManager.NewPreviewScene();
            Level level = BuildEmptyLevel(System.IO.Path.GetFileNameWithoutExtension(path), catalog, preview);
            int obstacles = ObstacleGenerator.Fill(level, settings, seed, recordUndo: false);
            int waypoints = isCurved
                ? PathGenerator.Fill(level, settings.ToPathParameters(seed, settings.BendCount), recordUndo: false)
                : 0;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(level.gameObject, path);
            EditorSceneManager.ClosePreviewScene(preview);

            AddToCatalog(catalog, prefab.GetComponent<Level>());
            AssetDatabase.OpenAsset(prefab);
            Debug.Log($"[{nameof(LevelGeneratorMenu)}] Created {path} with {obstacles} obstacles and {waypoints} waypoints " +
                $"(seed {seed}) as level {catalog.Count} of {catalog.name}.");
        }

        // Classic and curved levels live in one folder, so a new level takes the number after the highest one there.
        private static int GetNextLevelNumber()
        {
            int highest = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { LevelsFolder }))
            {
                string name = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid));
                Match match = _levelNumber.Match(name);
                if (match.Success)
                {
                    highest = Mathf.Max(highest, int.Parse(match.Groups[1].Value));
                }
            }

            return highest + 1;
        }

        // New levels copy the start and finish of the first level of their catalog, so every level fits the same
        // camera and ground.
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

        private static bool TryGetCatalog(string path, out LevelCatalog catalog)
        {
            catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(path);
            if (catalog != null)
            {
                return true;
            }

            Debug.LogError($"[{nameof(LevelGeneratorMenu)}] {path} is missing.");
            return false;
        }
    }
}
