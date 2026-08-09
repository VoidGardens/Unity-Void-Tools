using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

using static System.IO.Path;
using static UnityEditor.AssetDatabase;

namespace VoidTools
{    
    public static class ProjectSetup
    {
        [MenuItem("VoidGardens/Project Setup/Import Essential Assets")]
        static void ImportEsstinals()
        {
            
        }

        [MenuItem("VoidGardens/Project Setup/Install Essential Packages")]
        static void ImportPackages()
        {
            Packages.InstallPackages(new[]
            {
              "https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main" 
            });
        }

        [MenuItem("VoidGardens/Project Setup/Create Folders")]
        static void CreateFolders()
        {
            Folders.Create("_Project", "Animation", "Art", "Art/Materials", "Art/FBX", "Art/Textures", "Prefabs", "Scripts", "Scripts/Runtime", "Scripts/Editor");
            Refresh();
            Folders.Move("_Project", "Scenes");
            Folders.Move("_Project", "Settings");
            Folders.Delete("TutorialInfo");
            Refresh();

            const string pathToInputActions = "Assets/InputSystem_Actions.inputactions";
            string destination = "Assets/_Project/Settings/InputSystem_Actions.inputactions";
            MoveAsset(pathToInputActions, destination);

            const string pathToReadme = "Assets/Readme.asset";
            DeleteAsset(pathToReadme);
            Refresh();
        }
    }

    static class Assets
    {
        public static void ImportAsset(string asset, string folder)
        {
            string basePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            string assetsFolder = Combine(basePath, "Unity/Asset Store-5.x");
            UnityEditor.AssetDatabase.ImportPackage(Combine(assetsFolder, folder, asset), false);
        }
    }

    static class Packages
    {
        static AddRequest request;
        static Queue<string> packageToInstall = new Queue<string>();

        static async void StartNextPackageInstallation()
        {
            request = Client.Add(packageToInstall.Dequeue());

            while (!request.IsCompleted) await Task.Delay(10);

            if (request.Status == StatusCode.Success) Debug.Log("Installed: " + request.Result.packageId);
            else if (request.Status >= StatusCode.Failure) Debug.LogError(request.Error.message);
                
            if (packageToInstall.Count > 0)
            {
                await Task.Delay(1000);
                StartNextPackageInstallation();
            }
        }

        public static void InstallPackages(string[] packages)
        {
            foreach (string package in packages)
            {
                packageToInstall.Enqueue(package);
            }

            if (packageToInstall.Count > 0)
            {
                StartNextPackageInstallation();
            }
        }
    }

    static class Folders
    {
        public static void Delete(string folderName)
        {
            string pathToDelete = $"Assets/{folderName}";

            if (IsValidFolder(pathToDelete))
            {
                DeleteAsset(pathToDelete);
            }
        }

        public static void Move(string newParent, string folderName)
        {
            string sourcePath = $"Assets/{folderName}";
            if (IsValidFolder(sourcePath))
            {
                string destinationPath = $"Assets/{newParent}/{folderName}";
                string error = MoveAsset(sourcePath, destinationPath);

                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogError($"Fialed to move {folderName}: {error}");
                }
            }
        }

        public static void CreateSubFolders(string rootPath, string folderHierarchy)
        {
            var folders = folderHierarchy.Split('/');
            var currentPath = rootPath;

            foreach (var folder in folders)
            {
                currentPath = Combine(currentPath, folder);
                if (!Directory.Exists(currentPath))
                {
                    Directory.CreateDirectory(currentPath);
                }
            }
        }

        public static void Create(string root, params string[] folders)
        {
            var fullPath = Combine(Application.dataPath, root);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            foreach (var folder in folders)
            {
                
            }
        }
    }
}
