using UnityEngine;

namespace RydenCam.BranchCamEditor
{
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
