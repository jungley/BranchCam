using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using RydenCam.Common;
using RydenCam.BranchCamEditor.Managers;
using System;
using System.Linq;
using Assets.RydenCam.Scripts.BranchCamCC;

namespace RydenCam.BranchCamEditor.Serialization
{
    [ExecuteAlways]
    public static class LoadFile
    {
        private static string path;

        private static string FindPath(string folderTitle, string defaultName)
        {
            string lastFileFolderPath = BranchCamEditorPreferences.GetLastFilePath();


            if(string.IsNullOrEmpty(lastFileFolderPath) || !Directory.Exists(lastFileFolderPath))
            {
                string relativePath = BranchConstants.DefaultDialogueFolder;
                //string relativePath = $"RydenCam/DialogueFiles/";
                lastFileFolderPath = Path.Combine(Application.dataPath, relativePath);
                if(!Directory.Exists(lastFileFolderPath))
                {
                    Directory.CreateDirectory(lastFileFolderPath);
                }
            }

            string fullPath;
            try
            {
                fullPath = EditorUtility.OpenFolderPanel(folderTitle, lastFileFolderPath, defaultName);
                //Cancel Button Pressed
                if (string.IsNullOrEmpty(fullPath)) return string.Empty;
            }
            catch(Exception)
            {
                BranchLog.Log("Cannot open file or no file chosen.");
                return string.Empty;
            }
            return fullPath;
        }

        public static bool IsSavePathValid(string folderTitle, string defaultName)
        {
            var directoryPath = FindPath(folderTitle, defaultName);

            if (string.IsNullOrEmpty(directoryPath)) return false;

            string pathWithNodeName = $"{directoryPath}/{NodeManager.Instance.GetSequenceName()}";

            BranchCamEditorPreferences.SetLastFilePath(pathWithNodeName);

            return true;
        }

        public static bool HasDialogueFile(string folderTitle, string defaultName)
        {
            var directoryPath = FindPath(folderTitle, defaultName);

            if(string.IsNullOrEmpty(directoryPath)) return false;

            string assetFileName = Directory.GetFiles(directoryPath, "*.json").FirstOrDefault();
            string assetFilePath = assetFileName?.Replace("\\", "/");

            if (!string.IsNullOrEmpty(assetFilePath))
            {
                BranchCamEditorPreferences.SetLastFilePath(directoryPath);
                return true;
            }
            else
            {
                BranchLog.Log("No Dialogue File found. Select a folder containing a dialogue file.");
                return false;
            }
        }

        public static bool IsValidDialogueTriggerPath(string path)
        {
            return !string.IsNullOrEmpty(path);
        }

        public static void LoadSaveables()
        {
            NodeManager.Instance.Clear();
            ConnectionManager.Instance.Clear();

            path = BranchCamEditorPreferences.GetLastFilePath();

            List<Node> deserializedNodes = NodeSerializer.DeserializeNodes(path);

            NodeManager.Instance.LoadNodes(deserializedNodes);
            ConnectionManager.Instance.CreateConnections(deserializedNodes);
        }

        public static void SetLastFilePath()
        {
            string fullPath = EditorUtility.OpenFolderPanel("Choose a folder containing Dialogue files only", BranchConstants.DefaultDialogueFolder, "Choose a folder containing Dialogue files only");
            string projectPath = Application.dataPath;

            try
            {
                // Calculate the relative path
                string relativePath = "Assets" + fullPath.Substring(projectPath.Length);

                BranchCamEditorPreferences.SetLastFilePath(relativePath);
            }
            catch (Exception)
            {
                BranchLog.Log("Cannot open file or no file chosen.");
            }
        }

    }
}