using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_2018_3_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
#endif

namespace VoidTools
{
    public class CollectionObjectMover : EditorWindow
    {
        private readonly List<GameObject> collections = new List<GameObject>();
        private string contextKey; // scene path or prefab asset path
        [MenuItem("VoidGardens/Collection Object Mover")] private static void Open() { GetWindow<CollectionObjectMover>("Collections"); }

        private void OnGUI()
        {
            UpdateContext();
            GUILayout.Label("Drag empty GameObjects here to register collections.", EditorStyles.boldLabel);
            DrawDragArea();
            if (collections.Count == 0)
            {
                EditorGUILayout.HelpBox("No collections added. Drag scene or prefab GameObjects into the area above.", MessageType.Info);
                return;
            }
            for (int i = 0; i < collections.Count; i++)
            {
                var go = collections[i];
                if (!IsValidForCurrentContext(go))
                {
                    collections.RemoveAt(i);
                    i--;
                    continue;
                }
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(go.name, GUILayout.Height(24))) MoveSelectionTo(go);
                if (GUILayout.Button("X", GUILayout.Width(24)))
                {
                    collections.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawDragArea()
        {
            var rect = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));
            GUI.Box(rect, "Drop Collections Here", EditorStyles.helpBox);
            var evt = Event.current;
            if (!rect.Contains(evt.mousePosition)) return;
            if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (var obj in DragAndDrop.objectReferences)
                    {
                        var go = obj as GameObject;
                        if (go == null) continue;
                        if (IsValidForCurrentContext(go) && !collections.Contains(go)) collections.Add(go);
                    }
                }
                evt.Use();
            }
        }

        private void MoveSelectionTo(GameObject targetCollection)
        {
            var selection = Selection.gameObjects;
            if (selection == null || selection.Length == 0) return;
            Undo.IncrementCurrentGroup();
            var group = Undo.GetCurrentGroup();
            foreach (var go in selection)
            {
                if (go == targetCollection) continue;
                if (IsAncestor(go.transform, targetCollection.transform)) continue;
                Undo.SetTransformParent(go.transform, targetCollection.transform, "Move To Collection");
            }
            Undo.CollapseUndoOperations(group);
        }

        private bool IsAncestor(Transform potentialChild, Transform potentialAncestor)
        {
            var t = potentialAncestor.parent;
            while (t != null)
            {
                if (t == potentialChild) return true;
                t = t.parent;
            }
            return false;
        }

        private void UpdateContext()
        {
            var key = GetCurrentContextKey();
            if (contextKey != key)
            {
                contextKey = key;
                collections.Clear(); // drop previous context's collections to avoid invalid references
            }
        }

        private string GetCurrentContextKey()
        {
    #if UNITY_2018_3_OR_NEWER
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null) return "prefab:" + stage.assetPath;
    #endif
            var scene = SceneManager.GetActiveScene();
            return "scene:" + scene.path;
        }

        private bool IsValidForCurrentContext(GameObject go)
        {
            if (go == null) return false;
    #if UNITY_2018_3_OR_NEWER
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null)
            {
                // Inside prefab stage: only allow objects that are part of the prefab contents
                return go.scene.IsValid() && stage.IsPartOfPrefabContents(go);
            }
    #endif
            // In a normal scene: object must belong to the active scene
            return go.scene.IsValid() && go.scene == SceneManager.GetActiveScene();
        }

        private void OnHierarchyChange()
        {
            // Prune invalid objects if hierarchy changed (scene switch, deletion, prefab exit)
            for (int i = collections.Count - 1; i >= 0; i--)
            {
                if (!IsValidForCurrentContext(collections[i])) collections.RemoveAt(i);
            }
        }
    }
}
