using System.Collections.Generic;
using SelfBulletting.Gameplay.Levels;
using UnityEditor;
using UnityEngine;

namespace SelfBulletting.Editor.LevelGeneration
{
    /// <summary>
    /// Replaces the waypoints of a level with a freshly generated curve and sets the level field to the generation area.
    /// Creates the path root when the level has none.
    /// </summary>
    internal static class PathGenerator
    {
        private const string UndoName = "Generate Path";
        private const string PathRootName = "Path";
        private const string PathRootField = "_pathRoot";
        private const string FieldField = "_field";

        /// <returns>The number of placed waypoints.</returns>
        public static int Fill(Level level, PathLayoutParameters parameters, bool recordUndo)
        {
            Transform root = GetOrCreateRoot(level, parameters.Area, recordUndo);
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Destroy(root.GetChild(i).gameObject, recordUndo);
            }

            List<Vector3> waypoints = PathLayout.Generate(parameters, level.StartPoint, level.FinishPoint);
            for (int i = 0; i < waypoints.Count; i++)
            {
                var waypoint = new GameObject($"Waypoint_{i + 1}");
                waypoint.transform.SetParent(root, false);
                waypoint.transform.position = waypoints[i];
                if (recordUndo)
                {
                    Undo.RegisterCreatedObjectUndo(waypoint, UndoName);
                }
            }

            return waypoints.Count;
        }

        private static Transform GetOrCreateRoot(Level level, Rect field, bool recordUndo)
        {
            var serialized = new SerializedObject(level);
            SerializedProperty rootProperty = serialized.FindProperty(PathRootField);
            var root = rootProperty.objectReferenceValue as Transform;
            if (root == null)
            {
                root = new GameObject(PathRootName).transform;
                root.SetParent(level.transform, false);
                if (recordUndo)
                {
                    Undo.RegisterCreatedObjectUndo(root.gameObject, UndoName);
                }

                rootProperty.objectReferenceValue = root;
            }

            serialized.FindProperty(FieldField).rectValue = field;
            if (recordUndo)
            {
                serialized.ApplyModifiedProperties();
            }
            else
            {
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }

            return root;
        }

        private static void Destroy(GameObject gameObject, bool recordUndo)
        {
            if (recordUndo)
            {
                Undo.DestroyObjectImmediate(gameObject);
            }
            else
            {
                Object.DestroyImmediate(gameObject);
            }
        }
    }
}
