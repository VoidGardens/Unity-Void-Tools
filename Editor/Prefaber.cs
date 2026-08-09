using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VoidTools
{
    public static class Prefaber
    {
        [MenuItem("VoidGardens/Prefaber/Process Selected", priority = 1000)]
        public static void ProcessSelected()
        {
            var selected = Selection.gameObjects;
            if (selected == null || selected.Length == 0)
            {
                Debug.Log("Prefaber: No scene objects selected.");
                return;
            }

            // Scene objects only and top-level (avoid processing children twice if parent is also selected)
            var all = selected.Where(go => go != null && go.scene.IsValid()).ToArray();
            if (all.Length == 0)
            {
                Debug.Log("Prefaber: Selection contains no scene objects.");
                return;
            }

            var set = new HashSet<Transform>(all.Select(g => g.transform));
            var targets = new List<GameObject>();
            foreach (var go in all)
            {
                var p = go.transform.parent;
                bool parentSelected = false;
                while (p != null)
                {
                    if (set.Contains(p)) { parentSelected = true; break; }
                    p = p.parent;
                }
                if (!parentSelected) targets.Add(go);
            }

            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Prefaber Process Selected");

            foreach (var go in targets)
            {
                if (go == null) continue;
                ProcessOne(go);
            }

            Undo.CollapseUndoOperations(group);
        }

        private static void ProcessOne(GameObject go)
        {
            // Unpack prefab/model instance at outermost root if needed
            var outer = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
            if (outer != null && PrefabUtility.IsPartOfPrefabInstance(outer))
            {
                Undo.RegisterFullObjectHierarchyUndo(outer, "Unpack Prefab Instance");
                PrefabUtility.UnpackPrefabInstance(outer, PrefabUnpackMode.Completely, InteractionMode.UserAction);
            }

            // Capture original name and parent
            string originalName = go.name;
            var oldParent = go.transform.parent;

            // Reset transforms of the selected object
            Undo.RecordObject(go.transform, "Reset Transforms");
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            // Create empty parent under original parent and reset its transforms
            var empty = new GameObject("Empty");
            Undo.RegisterCreatedObjectUndo(empty, "Create Empty Parent");
            empty.transform.SetParent(oldParent, false);
            empty.transform.localPosition = Vector3.zero;
            empty.transform.localRotation = Quaternion.identity;
            empty.transform.localScale = Vector3.one;

            // Parent selected object to the new empty
            Undo.SetTransformParent(go.transform, empty.transform, "Parent To Empty");

            // Rename: parent gets original name, child gets _mesh postfix
            Undo.RecordObject(empty, "Rename Empty Parent");
            empty.name = originalName;
            Undo.RecordObject(go, "Rename Mesh Child");
            go.name = originalName + "_mesh";
        }
    }
}
