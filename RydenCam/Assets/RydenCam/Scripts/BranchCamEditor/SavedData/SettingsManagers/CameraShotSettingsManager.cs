using Assets.RydenCam.Scripts.BranchCamEditor.Managers;
using Assets.RydenCam.Scripts.Editor.CameraShotEdtior;
using RydenCam.BranchCamEditor.BranchCam;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.Common;
using System.Collections.Generic;
using UnityEngine;

namespace RydenCam.BranchCamEditor.Serialization
{
    [ExecuteAlways]
    public static class CameraShotSettingsManager
    {
        public static bool Save(string filePath)
        {
            if (CameraShotsManager.Instance.CameraShots == null) return false;

            if (string.IsNullOrEmpty(filePath))
            {
                Debug.Log("No file path provided. Aborting save");
                return false;
            }

            if (string.IsNullOrEmpty(filePath))
            {
                filePath = SettingsService.ShowSaveAsDialog("Save Camera Shots As", BranchConstants.DefaultDialogueFolder, "CameraShots", "json");
                if (string.IsNullOrEmpty(filePath)) return false;
            }

            CameraShotConfigurationWrapper container = new CameraShotConfigurationWrapper();
            container.Shots = CameraShotsManager.Instance.CameraShots;

            string cameraShotsJson = JsonUtility.ToJson(container);

            bool ok = SettingsService.Save(container, filePath, FilePathSaveManager.LastOpened_CameraShotsKey);
            if (ok) BranchLog.Log($"Saved camera shots to {filePath}");
            
            return ok;
        }

        public static bool SaveAs()
        {
            string path = SettingsService.ShowSaveAsDialog("Save Camera Shots As", BranchConstants.DefaultDialogueFolder, "CameraShots", "json");
            if (string.IsNullOrEmpty(path)) return false;
            return Save(path);
        }

        public static void OpenAndLoad()
        {
            string path = SettingsService.ShowOpenFileDialog("Select Camera Shots JSON", BranchConstants.DefaultDialogueFolder, "json");
            if (string.IsNullOrEmpty(path)) return;

            Load(path);
        }

        public static void Load(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            var container = SettingsService.Load<CameraShotConfigurationWrapper>(filePath);
            if (container == null)
            {
                BranchLog.Error("Failed to load camera shots.");
                return;
            }

            CameraShotsManager.Instance.CameraShots = container.Shots;
        }

        public static void New()
        {
            CameraShotsManager.Instance.CameraShots.Clear();
            BranchLog.Log("New camera shots cleared.");
        }
    }
}