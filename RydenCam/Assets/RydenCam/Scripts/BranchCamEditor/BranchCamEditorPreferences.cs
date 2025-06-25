using Ink.Parsed;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.Common;
using System.IO;
using UnityEditor.PackageManager;
using UnityEngine;

namespace RydenCam.BranchCamEditor
{
    /// <summary>
    ///Used for loading the conversation when closing then reopening the BranchCam UI
    /// <summary>
    public static class BranchCamEditorPreferences
    {
        private const string LastOpenedFilePathKey = "Last File Path Key";


        public static string LastUsedJsonPath
        {
            get
            {
                string filepath = PlayerPrefs.GetString(LastOpenedFilePathKey, string.Empty);

                return File.Exists(filepath) ? filepath : string.Empty;
            }
        }

        public static void SetLastFilePath(string filePath)
        {
            PlayerPrefs.SetString(LastOpenedFilePathKey, filePath);
            PlayerPrefs.Save();
        }

        public static string GetLastFileFolderPath(string filePath = "")
        {
            return string.IsNullOrEmpty(LastUsedJsonPath) ? string.Empty : System.IO.Path.GetDirectoryName(LastUsedJsonPath);
        }
    }
}