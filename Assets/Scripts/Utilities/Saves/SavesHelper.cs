using System;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Unity.Properties;
using UnityEngine;

namespace Utilities.Saves
{
    public static class SavesHelper
    {
        public static void Save(string directoryPath, string fileName, string contents, string postFixIdentifier, int capacityLimit = 0)
        {
            if (Directory.Exists(directoryPath) == false)
            {
                try
                {
                    Debug.Log($"Created directory at path: {directoryPath}");
                    Directory.CreateDirectory(directoryPath);
                }
                catch (InvalidPathException e)
                {
                    Debug.LogError($"Saves directory could not be created: {e.Message}");
                }
            }
            
            var newFileName = fileName + "_" + postFixIdentifier;

            if (capacityLimit > 0 && Directory.EnumerateFiles(directoryPath).Count() == capacityLimit)
            {
               var files = Directory.GetFiles(directoryPath);
               var oldestFile = files.Select(file => new FileInfo(file)).OrderBy(info => info.LastWriteTime).First();

               try
               {
                   File.Delete(oldestFile.FullName);
               }
               catch (MissingFieldException exception)
               {
                   Debug.LogError($"Could not delete excess file: {exception}");
               }
            }
            
            File.WriteAllTextAsync(directoryPath + newFileName + ".txt", contents).AsUniTask().Forget();
        }
        
        public static string Load(string filePath)
        {
            var directoryPath = Path.GetDirectoryName(filePath);
            if (Directory.Exists(directoryPath) == false)
            {
                Debug.Log($"Directory does not exist: {directoryPath}");
                return string.Empty;
            }

            if (File.Exists(filePath) == false)
            {
                Debug.Log($"File does not exist: {directoryPath}");
                return string.Empty;
            }

            var contents = File.ReadAllText(filePath);
            return contents;
        }
        
        public static UniTask<string> LoadAsync(string filePath)
        {
            var directoryPath = Path.GetDirectoryName(filePath);
            if (Directory.Exists(directoryPath) == false)
            {
                return UniTask.FromException<string>(new DirectoryNotFoundException());
            }

            if (File.Exists(filePath) == false)
            {
                return UniTask.FromException<string>(new FileNotFoundException());
            }

            var contents = File.ReadAllTextAsync(filePath).AsUniTask();
            return contents;
        }

        public static string[] LoadMostRecentFiles(string directoryPath, int maxFilesToLoad = 1)
        {
            if (Directory.Exists(directoryPath) == false)
            {
                Debug.Log($"Directory does not exist: {directoryPath}");
                return Array.Empty<string>();
            }
            
            var fileNames = Directory.GetFiles(directoryPath);

            if (fileNames.Length < 1)
            {
                return Array.Empty<string>();
            }
            
            var recentFiles = fileNames
                .Select(file => new FileInfo(file))
                .OrderByDescending(info => info.LastWriteTime)
                .Take(maxFilesToLoad);
            
            string[] contents = recentFiles.Select(info => Load(info.FullName)).ToArray();
            return contents;
        }
        
        public static UniTask<string[]> LoadMostRecentFilesAsync(string directoryPath, int maxFilesToLoad = 1)
        {
            if (Directory.Exists(directoryPath) == false)
            {
                return UniTask.FromResult(Array.Empty<string>());
            }
            
            var fileNames = Directory.GetFiles(directoryPath);

            if (fileNames.Length < 1)
            {
                return UniTask.FromResult(Array.Empty<string>());
            }
            
            var recentFiles = fileNames
                .Select(file => new FileInfo(file))
                .OrderByDescending(info => info.LastWriteTime)
                .Take(maxFilesToLoad);
            
            var contents = UniTask.WhenAll(recentFiles.Select(info => LoadAsync(info.FullName)).ToArray());
            return contents;
        }
    }
}