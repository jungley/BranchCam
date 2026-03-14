using RydenCam.Common;
using System;
using System.IO;
using UnityEngine;
using Assets.RydenCam.Scripts.BranchCamEditor.Managers;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RydenCam.BranchCamEditor.Serialization
{
    [ExecuteAlways]
    public static class SettingsService
    {
        public static bool Save<T>(T data, string path, string key)
        {
            if (string.IsNullOrEmpty(path)) return false;
            
            try
            {
                string json = JsonUtility.ToJson(data);
                File.WriteAllText(path, json);
#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
                FilePathSaveManager.Instance.SetLastFilePath(path, key);
#if UNITY_EDITOR
                PingAssetIfProjectPath(path);
#endif
                return true;
            }
            catch (Exception e)
            {
                BranchLog.Error($"SettingsService.Save<{typeof(T).Name}> failed: {e.Message}");
                return false;
            }
        }

        public static T Load<T>(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return default;
            try
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception e)
            {
                BranchLog.Error($"SettingsService.Load<{typeof(T).Name}> failed: {e.Message}");
                return default;
            }
        }

#if UNITY_EDITOR
        public static string ShowSaveAsDialog(string title, string defaultFolder, string defaultName, string extension = "json")
        {
            try
            {
                string result = EditorUtility.SaveFilePanel(title, defaultFolder, defaultName, extension);
                if(string.IsNullOrEmpty(result)) Debug.Log("No file was chosen");
                return result?.Replace("\\", "/") ?? string.Empty;
            }
            catch (Exception)
            {
                BranchLog.Log("Could not open Save File dialog.");
                return string.Empty;
            }
        }

        public static string ShowOpenFileDialog(string title, string defaultFolder, string extension = "json")
        {
            try
            {
                string full = EditorUtility.OpenFilePanel(title, defaultFolder, extension);
                return full?.Replace("\\", "/") ?? string.Empty;
            }
            catch (Exception)
            {
                BranchLog.Log("Cannot open file or no file chosen.");
                return string.Empty;
            }
        }

        private static void PingAssetIfProjectPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            string normalized = path.Replace("\\", "/");
            if (!normalized.StartsWith("Assets/")) return;

            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(normalized);
            if (obj != null) EditorGUIUtility.PingObject(obj);
        }
#endif
    }
}
