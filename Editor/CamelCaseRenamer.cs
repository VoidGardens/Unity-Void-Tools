#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace VoidTools
{
    /// <summary>
    /// Auto-rename tool. Normalizes the names of selected Project assets and/or
    /// Hierarchy GameObjects to the studio convention:
    ///     Capitalized_Words_Separated_By_Underscores
    ///
    /// Rules:
    ///  - Every word starts with a capital and words are joined by '_'.
    ///  - camelCase / PascalCase is split:  MainTex -> Main_Tex.
    ///  - Existing separators (space, '-', '.') become '_'.
    ///  - Numbers are kept.
    ///  - 'draft' and 'diffuse' are stripped out entirely (case-insensitive).
    ///
    /// Place this file inside any 'Editor' folder (e.g. Assets/Editor/).
    /// </summary>
    public class VoidGardensAutoRename : EditorWindow
    {
        // Words removed entirely from any name (case-insensitive).
        private static readonly string[] ForbiddenWords = { "draft", "diffuse", "decor" };

        private readonly List<RenameItem> _items = new List<RenameItem>();
        private Vector2 _scroll;

        // ------------------------------------------------------------------ Menu

        [MenuItem("VoidGardens/Auto Rename")]
        public static void Open()
        {
            var w = GetWindow<VoidGardensAutoRename>(false, "Auto Rename");
            w.minSize = new Vector2(440, 320);
            w.Refresh();
            w.Show();
        }

        // Quick path: rename the current selection straight away (Ctrl/Cmd+Shift+R).
        [MenuItem("VoidGardens/Rename Selection Now %#r")]
        public static void RenameSelectionNow()
        {
            var items = BuildTargets();
            var changing = items.Where(i => i.Changed).ToList();
            if (changing.Count == 0)
            {
                EditorUtility.DisplayDialog("VoidGardens Auto Rename",
                    "Nothing to rename. Select assets or GameObjects first.", "OK");
                return;
            }

            string preview = string.Join("\n",
                changing.Take(15).Select(i => i.Original + "  ->  " + i.Proposed));
            if (changing.Count > 15) preview += "\n... and " + (changing.Count - 15) + " more";

            if (EditorUtility.DisplayDialog("VoidGardens Auto Rename",
                    "Rename " + changing.Count + " item(s)?\n\n" + preview, "Rename", "Cancel"))
            {
                int n = Apply(items);
                Debug.Log("[VoidGardens] Renamed " + n + " item(s).");
            }
        }

        // ------------------------------------------------------------- Core rule

        /// <summary>
        /// Converts a raw name to the convention: capitalized words joined by '_'.
        /// Returns string.Empty if every word was stripped (caller should skip).
        /// </summary>
        public static string Normalize(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

            string s = raw.Trim();

            // Split camelCase / PascalCase:  "MainTex" -> "Main Tex"
            s = Regex.Replace(s, "([a-z0-9])([A-Z])", "$1 $2");
            // Split acronym boundaries:  "UVMap" -> "UV Map",  "XMLParser" -> "XML Parser"
            s = Regex.Replace(s, "([A-Za-z])([0-9])", "$1 $2");
            s = Regex.Replace(s, "([0-9])([A-Za-z])", "$1 $2");
            // Any existing separator becomes a word break.
            s = Regex.Replace(s, "[\\s\\-\\._]+", " ");

            IEnumerable<string> words = s
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(w => Regex.Replace(w, "[^A-Za-z0-9]", ""))   // drop stray symbols
                .Where(w => w.Length > 0)
                .Where(w => !ForbiddenWords.Any(f => string.Equals(f, w, StringComparison.OrdinalIgnoreCase)))
                .Select(Capitalize);

            return string.Join("_", words);
        }

        private static string Capitalize(string w)
        {
            return w.Length == 0 ? w : char.ToUpperInvariant(w[0]) + w.Substring(1);
        }

        // --------------------------------------------------------- Build targets

        private static List<RenameItem> BuildTargets()
        {
            var items = new List<RenameItem>();
            foreach (var obj in Selection.objects)
            {
                if (obj == null) continue;

                if (AssetDatabase.Contains(obj))
                {
                    string path = AssetDatabase.GetAssetPath(obj);
                    if (string.IsNullOrEmpty(path)) continue;
                    string original = Path.GetFileNameWithoutExtension(path);
                    items.Add(new RenameItem
                    {
                        Obj = obj,
                        IsAsset = true,
                        Path = path,
                        Original = original,
                        Proposed = Normalize(original)
                    });
                }
                else if (obj is GameObject go)
                {
                    items.Add(new RenameItem
                    {
                        Obj = go,
                        IsAsset = false,
                        Original = go.name,
                        Proposed = Normalize(go.name)
                    });
                }
            }
            return items;
        }

        // ---------------------------------------------------------------- Apply

        private static int Apply(List<RenameItem> items)
        {
            int renamed = 0;
            var errors = new StringBuilder();

            // Assets — batched for performance / a single refresh.
            var assetItems = items.Where(i => i.IsAsset && i.Changed).ToList();
            if (assetItems.Count > 0)
            {
                AssetDatabase.StartAssetEditing();
                try
                {
                    foreach (var it in assetItems)
                    {
                        string err = AssetDatabase.RenameAsset(it.Path, it.Proposed);
                        if (string.IsNullOrEmpty(err)) renamed++;
                        else errors.AppendLine("- " + it.Original + " -> " + it.Proposed + ": " + err);
                    }
                }
                finally
                {
                    AssetDatabase.StopAssetEditing();
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }

            // Scene objects — undoable.
            var sceneItems = items.Where(i => !i.IsAsset && i.Changed).ToList();
            if (sceneItems.Count > 0)
            {
                Undo.RecordObjects(sceneItems.Select(i => i.Obj).ToArray(), "VoidGardens Auto Rename");
                foreach (var it in sceneItems)
                {
                    it.Obj.name = it.Proposed;
                    renamed++;
                    if (it.Obj is GameObject go && go.scene.IsValid())
                        EditorSceneManager.MarkSceneDirty(go.scene);
                }
            }

            if (errors.Length > 0)
                Debug.LogWarning("[VoidGardens] Some renames failed:\n" + errors);

            return renamed;
        }

        // ------------------------------------------------------------------- GUI

        private void OnEnable() => Refresh();
        private void OnSelectionChange() { Refresh(); Repaint(); }

        private void Refresh()
        {
            _items.Clear();
            _items.AddRange(BuildTargets());
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Convention:  Capitalized_Words_With_Underscores",
                EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "MainTex -> Main_Tex   -   camelCase is split\n" +
                "Spaces, - and . become _   -   numbers kept\n" +
                "'draft' and 'diffuse' are removed",
                MessageType.None);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Refresh", GUILayout.Width(80))) Refresh();
                GUILayout.FlexibleSpace();
                int willChange = _items.Count(i => i.Changed);
                EditorGUILayout.LabelField(willChange + " of " + _items.Count + " will change",
                    GUILayout.Width(160));
            }

            if (_items.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "Select assets in the Project window or GameObjects in the Hierarchy.",
                    MessageType.Info);
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (var it in _items)
            {
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(it.IsAsset ? "Asset" : "Scene", GUILayout.Width(42));
                    EditorGUILayout.LabelField(it.Original, GUILayout.MinWidth(70));
                    EditorGUILayout.LabelField("->", GUILayout.Width(20));

                    Color prev = GUI.contentColor;
                    if (string.IsNullOrEmpty(it.Proposed)) GUI.contentColor = new Color(1f, 0.5f, 0.5f);
                    else if (it.Changed) GUI.contentColor = new Color(0.55f, 0.9f, 0.55f);
                    else GUI.contentColor = Color.gray;

                    EditorGUILayout.LabelField(
                        string.IsNullOrEmpty(it.Proposed) ? "(all words stripped - skipped)" : it.Proposed,
                        GUILayout.MinWidth(70));
                    GUI.contentColor = prev;
                }
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(_items.All(i => !i.Changed)))
            {
                if (GUILayout.Button("Rename", GUILayout.Height(30)))
                {
                    int n = Apply(_items);
                    ShowNotification(new GUIContent("Renamed " + n + " item(s)"));
                    Refresh();
                }
            }
        }

        // ----------------------------------------------------------------- Model

        private class RenameItem
        {
            public UnityEngine.Object Obj;
            public bool IsAsset;
            public string Path;
            public string Original;
            public string Proposed;

            public bool Changed => !string.IsNullOrEmpty(Proposed) && Proposed != Original;
        }
    }
}
#endif