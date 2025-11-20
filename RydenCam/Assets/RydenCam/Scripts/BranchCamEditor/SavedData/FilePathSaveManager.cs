using Assets.RydenCam.Scripts.Editor.CameraShotEdtior;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamEditor.Managers
{
    public class FilePathSaveManager
    {
        /// putting this here temporarily until a better place is found
        public SaveEditorSettingsData SettingsData { get; set; }


        private static FilePathSaveManager instance;
        public static FilePathSaveManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new FilePathSaveManager();
                    instance.SettingsData = new SaveEditorSettingsData();
                }
                return instance;
            }
        }

        public const string LastOpened_NodeGraphKey = "LastFilePathKey_NodeGraph";
        public const string LastOpened_CameraShotsKey = "LastFilePathKey_CameraShots";
        public const string LastOpened_EditorSettingsKey = "LastFilePathKey_EditorSettings";


        public string GetLastFilePathSaved(string key)
        {
            string filepath = PlayerPrefs.GetString(key, string.Empty);
            return File.Exists(filepath) ? filepath : string.Empty;
        }

        public string GetLastFolderPathSaved(string key)
        {
            string filepath = GetLastFilePathSaved(key);

            return string.IsNullOrEmpty(filepath) ? string.Empty : System.IO.Path.GetDirectoryName(filepath);
        }

        public void SetLastFilePath(string filePath, string lastOpenedKey)
        {
            PlayerPrefs.SetString(lastOpenedKey, filePath);
            PlayerPrefs.Save();
        }

        public void ClearLastFilePath(string lastOpenedKey)
        {
            PlayerPrefs.DeleteKey(lastOpenedKey);
            PlayerPrefs.Save();

        }
    }
}