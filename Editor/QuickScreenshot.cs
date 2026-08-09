using UnityEditor;
using UnityEngine;
using System.IO;

// Drop this file into an "Editor" folder anywhere in your Assets.
// Menu: VoidGardens > Screenshot > ...

namespace VoidTools
{    
    public static class QuickScreenshot
    {
        private const string PrefKey = "VoidGardens_ScreenshotPath";
        private static string DefaultPath => Path.Combine(Application.dataPath, "../Screenshots");

        private static string SavePath
        {
            get => EditorPrefs.GetString(PrefKey, DefaultPath);
            set => EditorPrefs.SetString(PrefKey, value);
        }

        [MenuItem("VoidGardens/Screenshot/Capture Play Mode Screenshot %#s")] // Ctrl/Cmd+Shift+S
        public static void CaptureScreenshot()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogWarning("[VoidGardens] Enter Play Mode to capture a screenshot.");
                return;
            }

            string folder = SavePath;
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string fileName = $"screenshot_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
            string fullPath = Path.Combine(folder, fileName);

            ScreenCapture.CaptureScreenshot(fullPath);
            Debug.Log($"[VoidGardens] Screenshot saved to: {fullPath}");

            // Optional: reveal in file explorer
            EditorApplication.delayCall += () => EditorUtility.RevealInFinder(fullPath);
        }

        [MenuItem("VoidGardens/Screenshot/Set Save Path...")]
        public static void SetSavePath()
        {
            string chosen = EditorUtility.OpenFolderPanel("Choose Screenshot Folder", SavePath, "");
            if (!string.IsNullOrEmpty(chosen))
            {
                SavePath = chosen;
                Debug.Log($"[VoidGardens] Screenshot path set to: {chosen}");
            }
        }

        [MenuItem("VoidGardens/Screenshot/Open Save Folder")]
        public static void OpenSaveFolder()
        {
            if (!Directory.Exists(SavePath))
                Directory.CreateDirectory(SavePath);
            EditorUtility.RevealInFinder(SavePath);
        }

        // Disable "Capture" menu item when not in Play Mode (grays it out)
        [MenuItem("VoidGardens/Screenshot/Capture Play Mode Screenshot %#s", true)]
        public static bool ValidateCapture()
        {
            return EditorApplication.isPlaying;
        }
    }
}
