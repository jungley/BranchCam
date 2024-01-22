using UnityEngine;

namespace RydenCam.BranchCamEditor
{
    /// <summary>
    ///Used for loading the conversation when closing then reopening the BranchCam UI
    /// <summary>
    public static class BranchCamEditorPreferences
    {
        private const string LastFilePathKey = "Last File Path Key";

        public static string GetLastFilePath()
        {
            return PlayerPrefs.GetString(LastFilePathKey, string.Empty);
        }

        public static void SetLastFilePath(string filePath)
        {
            PlayerPrefs.SetString(LastFilePathKey, filePath);
            PlayerPrefs.Save();
        }
    }
}
