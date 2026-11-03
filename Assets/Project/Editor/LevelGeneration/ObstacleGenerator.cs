using System.Collections.Generic;
using SelfBulletting.Gameplay.Levels;
using UnityEditor;
using UnityEngine;

namespace SelfBulletting.Editor.LevelGeneration
{
    /// <summary>
    /// Replaces the obstacles of a level with a freshly generated layout.
    /// </summary>
    internal static class ObstacleGenerator
    {
        private const string UndoName = "Generate Obstacles";

        /// <returns>The number of placed obstacles.</returns>
        public static int Fill(Level level, LevelGenerationSettings settings, int seed, bool recordUndo)
        {
            Clear(level, recordUndo);

            List<Vector3> positions = ObstacleLayout.Generate(settings.ToLayoutParameters(seed), level.StartPoint, level.FinishPoint);
            foreach (Vector3 position in positions)
            {
                var obstacle = (GameObject)PrefabUtility.InstantiatePrefab(settings.ObstaclePrefab, level.ObstaclesRoot);
                obstacle.transform.position = position;
                if (recordUndo)
                {
                    Undo.RegisterCreatedObjectUndo(obstacle, UndoName);
                }
            }

            return positions.Count;
        }

        public static void Clear(Level level, bool recordUndo)
        {
            Transform root = level.ObstaclesRoot;
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                GameObject obstacle = root.GetChild(i).gameObject;
                if (recordUndo)
                {
                    Undo.DestroyObjectImmediate(obstacle);
                }
                else
                {
                    Object.DestroyImmediate(obstacle);
                }
            }
        }
    }
}
