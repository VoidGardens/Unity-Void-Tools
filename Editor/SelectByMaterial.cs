using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace VoidTools
{
    public class SelectObjectsWithMaterialInspector
    {
        
        private static readonly List<Material> _tempMaterials = new List<Material>();
        
        [MenuItem("CONTEXT/Renderer/Select Objects With Missing Materials")]
        private static void SelectObjectsWithMissingMaterials(MenuCommand command)
        {
            Renderer sourceRenderer = (Renderer)command.context;

            sourceRenderer.GetSharedMaterials(_tempMaterials);
            bool sourceHasMissing = false;
            for (int i = 0; i < _tempMaterials.Count; i++)
            {
                if (_tempMaterials[i] == null)
                {
                    sourceHasMissing = true;
                    break;
                }
            }

            _tempMaterials.Clear();

            if (!sourceHasMissing)
            {
                Debug.LogWarning($"На объекте '{sourceRenderer.name}' нет потерянных (Missing) материалов.");
                return;
            }

            List<GameObject> foundObjects = new List<GameObject>();
            var renderers = GetAllRenderers();
            
            int total = renderers.Count;
            float progressStep = 1f / total;

            try
            {
                for (int i = 0; i < total; i++)
                {
                    Renderer renderer = renderers[i];

                    if (i % 500 == 0)
                    {
                        if (EditorUtility.DisplayCancelableProgressBar(
                            "Searching Missing Materials", 
                            $"Checking object {i}/{total}...", 
                            i * progressStep))
                        {
                            break;
                        }
                    }

                    renderer.GetSharedMaterials(_tempMaterials);
                    
                    bool hasMissing = false;
                    for (int m = 0; m < _tempMaterials.Count; m++)
                    {
                        if (_tempMaterials[m] == null)
                        {
                            hasMissing = true;
                            break;
                        }
                    }

                    if (hasMissing)
                    {
                        foundObjects.Add(renderer.gameObject);
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                _tempMaterials.Clear();
            }

            ApplySelection(foundObjects, "с потерянными (Missing) материалами");
        }


        [MenuItem("CONTEXT/Material/Select Objects in Scene or Prefab (By Material)")]
        private static void SelectObjectsByMaterial(MenuCommand command)
        {
            Material selectedMaterial = (Material)command.context;
            if (selectedMaterial == null)
            {
                Debug.LogError("No material selected.");
                return;
            }

            List<GameObject> foundObjects = new List<GameObject>();
            var renderers = GetAllRenderers();
            
            int total = renderers.Count;
            float progressStep = 1f / total;

            try
            {
                for (int i = 0; i < total; i++)
                {
                    Renderer renderer = renderers[i];
                    
                    if (i % 500 == 0)
                    {
                        if (EditorUtility.DisplayCancelableProgressBar(
                            "Searching Objects", 
                            $"Checking object {i}/{total}...", 
                            i * progressStep))
                        {
                            break;
                        }
                    }

                    if (renderer.sharedMaterial == selectedMaterial)
                    {
                        foundObjects.Add(renderer.gameObject);
                        continue;
                    }

                    renderer.GetSharedMaterials(_tempMaterials);
                    for (int m = 0; m < _tempMaterials.Count; m++)
                    {
                        if (_tempMaterials[m] == selectedMaterial)
                        {
                            foundObjects.Add(renderer.gameObject);
                            break;
                        }
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                _tempMaterials.Clear();
            }

            ApplySelection(foundObjects, $"с материалом '{selectedMaterial.name}'");
        }

        [MenuItem("CONTEXT/Material/Select Objects in Scene or Prefab (By Main Texture)")]
        private static void SelectObjectsByMainTexture(MenuCommand command)
        {
            Material selectedMaterial = (Material)command.context;
            if (selectedMaterial == null) return;

            Texture mainTexture = selectedMaterial.mainTexture;
            if (mainTexture == null)
            {
                Debug.LogWarning($"У материала '{selectedMaterial.name}' не назначена главная текстура (_MainTex).");
                return;
            }

            List<GameObject> foundObjects = new List<GameObject>();
            var renderers = GetAllRenderers();
            
            int total = renderers.Count;
            float progressStep = 1f / total;

            try
            {
                for (int i = 0; i < total; i++)
                {
                    Renderer renderer = renderers[i];

                    if (i % 500 == 0)
                    {
                        if (EditorUtility.DisplayCancelableProgressBar(
                            "Searching Objects", 
                            $"Checking object {i}/{total}...", 
                            i * progressStep))
                        {
                            break;
                        }
                    }

                    renderer.GetSharedMaterials(_tempMaterials);
                    
                    for (int m = 0; m < _tempMaterials.Count; m++)
                    {
                        Material mat = _tempMaterials[m];
                        if (mat != null && mat.mainTexture == mainTexture)
                        {
                            foundObjects.Add(renderer.gameObject);
                            break;
                        }
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                _tempMaterials.Clear();
            }

            ApplySelection(foundObjects, $"использующих текстуру '{mainTexture.name}'");
        }

        private static List<Renderer> GetAllRenderers()
        {
            List<Renderer> results = new List<Renderer>();
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

            if (prefabStage != null)
            {
                prefabStage.prefabContentsRoot.GetComponentsInChildren(true, results);
            }
            else
            {
    #if UNITY_2023_1_OR_NEWER
                results.AddRange(Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None));
    #else
                results.AddRange(Object.FindObjectsOfType<Renderer>(true)); 
    #endif
            }
            return results;
        }

        private static void ApplySelection(List<GameObject> objects, string logDescription)
        {
            if (objects.Count > 0)
            {
                Selection.objects = objects.ToArray();
                if (objects.Count < 100) 
                {
                    SceneView.FrameLastActiveSceneView();
                }
                Debug.Log($"Выделено {objects.Count} объектов {logDescription}.");
            }
            else
            {
                Debug.Log($"Не найдено объектов {logDescription}.");
                Selection.activeObject = null;
            }
        }
    }
}