using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;

namespace VoidTools
{    
    [InitializeOnLoad]
    public static class CleanNameCopier
    {
        static CleanNameCopier()
        {
            Editor.finishedDefaultHeaderGUI += DrawCleanNameButton;
        }

        private static void DrawCleanNameButton(Editor editor)
        {
            if (editor.targets.Length == 0 || !(editor.target is GameObject))
                return;

            GameObject go = (GameObject)editor.target;

            // НОВЫЕ КООРДИНАТЫ: 
            // X = 16 (выравнивание по левому краю под иконку)
            // Y = 38 (сразу под иконкой кубика, которая обычно заканчивается на ~38 пикселях)
            // Ширина = 18, Высота = 18
            Rect buttonRect = new Rect(16, 38, 18, 18);

            Color oldColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.4f, 0.9f, 0.4f); 

            if (GUI.Button(buttonRect, new GUIContent("N", "Копировать чистое имя объекта (без Clone и цифр)")))
            {
                CopyCleanName(go.name);
            }

            GUI.backgroundColor = oldColor;
        }

        private static void CopyCleanName(string originalName)
        {
            string cleanName = originalName.Replace("(Clone)", "");
            cleanName = Regex.Replace(cleanName, @"\s*\(\d+\)$", "");
            cleanName = cleanName.Trim();

            EditorGUIUtility.systemCopyBuffer = cleanName;
            Debug.Log($"[CleanNameCopier] Скопировано в буфер: <b>{cleanName}</b>");
        }
    }
}