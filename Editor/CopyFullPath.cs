using System.IO;
using UnityEditor;
using UnityEngine;

namespace VoidTools
{
    public static class CopyFullPath
    {
        [MenuItem("VoidGardens/Copy Full Path", false, 2000)]
        private static void CopySelectedAssetFullPath()
        {
            string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string fullPath = Path.GetFullPath(Path.Combine(projectRoot, assetPath));

            // For files, copy the containing folder only (strip file name + extension).
            // For folders, copy the folder's own full path as-is.
            string result = AssetDatabase.IsValidFolder(assetPath)
                ? fullPath
                : Path.GetDirectoryName(fullPath);

            EditorGUIUtility.systemCopyBuffer = result;
            Debug.Log($"[Copy Full Path] Copied: {result}");
        }

        [MenuItem("VoidGardens/Copy Full Path", true)]
        private static bool ValidateCopySelectedAssetFullPath()
        {
            return Selection.activeObject != null &&
                !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(Selection.activeObject));
        }
    }
}
