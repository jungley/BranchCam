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
    public class EditorSettingsManager
    {
        public SaveEditorSettingsData SettingsData = new SaveEditorSettingsData();

        private static EditorSettingsManager instance;
        public static EditorSettingsManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new EditorSettingsManager();
                }
                return instance;
            }
        }


        private const string LastOpenedFilePathKey = "Last File Path Key";

        public string LastUsedJsonPath
        {
            get
            {
                string filepath = PlayerPrefs.GetString(LastOpenedFilePathKey, string.Empty);
                return File.Exists(filepath) ? filepath : string.Empty;
            }
        }

        public void SetLastFilePath(string filePath)
        {
            PlayerPrefs.SetString(LastOpenedFilePathKey, filePath);
            PlayerPrefs.Save();
        }

        public string GetLastFileFolderPath(string filePath = "")
        {
            return string.IsNullOrEmpty(LastUsedJsonPath) ? string.Empty : System.IO.Path.GetDirectoryName(LastUsedJsonPath);
        }


        public void FlipIsCornerPreview()
        {
            SettingsData.IsCornerPreviewEnabled = !SettingsData.IsCornerPreviewEnabled;
        }

        public void FlipIsNodePreview()
        {
            SettingsData.IsNodePreviewEnabled = !SettingsData.IsNodePreviewEnabled;
        }
    }
}