#if UNITY_EDITOR
using Assets.RydenCam.Scripts.BranchCamEditor.Managers;
using Assets.RydenCam.Scripts.Editor.CameraShotEditor;
using RydenCam.BranchCamEditor.BranchCam;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.Common;
using UnityEngine;

namespace RydenCam.BranchCamEditor.Serialization
{
    [ExecuteAlways]
    public static class CameraShotSettingsManager
    {
        public static bool Save(string filePath = "")
        {
            if (CameraShotsManager.Instance.CameraShots == null) return false;

            if (string.IsNullOrEmpty(filePath))
            {
                string path = SettingsService.ShowSaveAsDialog("Save Camera Shots As", BranchConstants.DefaultDialogueFolder, "CameraShots", "json");
                if (string.IsNullOrEmpty(path)) return false;
                return Save(path);
            }

            CameraShotConfigurationWrapper container = new CameraShotConfigurationWrapper();
            container.Shots = CameraShotsManager.Instance.CameraShots;

            bool ok = SettingsService.Save(container, filePath, FilePathSaveManager.LastOpened_CameraShotsKey);
            if (ok)
            {
                BranchLog.Log($"Saved camera shots to {filePath}");
                FilePathSaveManager.Instance.SetLastFilePath(filePath, FilePathSaveManager.LastOpened_CameraShotsKey);
                AssociateFile(filePath);
            }
            
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

        public static bool Load(string filePath)
        {
            if (!CameraShotsManager.Instance.LoadFile(filePath)) return false;
            AssociateFile(filePath);
            FilePathSaveManager.Instance.SetLastFilePath(filePath, FilePathSaveManager.LastOpened_CameraShotsKey);
            return true;
        }

        private static void AssociateFile(string path)
        {
            path = UnityEditor.FileUtil.GetProjectRelativePath(path) is string relative && !string.IsNullOrEmpty(relative) ? relative : path;
            CameraShotsManager.Instance.CurrentFilePath = path;
            if (NodeManager.Instance.StartNode != null)
                NodeManager.Instance.StartNode.CameraShotFilePath = path;
        }

        public static void New()
        {
            CameraShotsManager.Instance.CameraShots.Clear();
            CameraShotsManager.Instance.CurrentFilePath = null;
            var _ = CameraShotsManager.Instance.DefaultShot;
        }
    }
}
#endif
