using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using RydenCam.BranchCamEditor.Nodes;
using RydenCam.Common;
using RydenCam.BranchCamEditor.Managers;
using System;
using System.Linq;

namespace RydenCam.BranchCamEditor.Serialization
{
    [ExecuteAlways]
    public static class LoadFile
    {
        private static string path;

        private static string FindPath(string folderTitle, string defaultName)
        {
            string fullPath = EditorUtility.OpenFolderPanel(folderTitle, BranchCamEditorPreferences.GetLastFileFolderPath(), defaultName);

            //Cancel Button Pressed
            if (string.IsNullOrEmpty(fullPath)) return string.Empty;


            // Get the data path of the Unity project
            string projectPath = Application.dataPath;

            try
            {
                // Calculate the relative path
                string relativePath = "Assets" + fullPath.Substring(projectPath.Length);
                return relativePath;
            }
            catch (Exception)
            {
                BranchLog.Log("Cannot open file or no file chosen.");
                return string.Empty;
            }
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
        public static bool IsValidEditorPath()
        {
            string filepath = BranchCamEditorPreferences.GetLastFilePath();
            return !string.IsNullOrEmpty(filepath);
        }

        public static bool IsValidDialogueTriggerPath(string path)
        {
            return !string.IsNullOrEmpty(path);
        }

        public static void LoadSaveables()
        {
            path = BranchCamEditorPreferences.GetLastFilePath();

            List<EditorBaseNode> loadedEditorNodes = NodeSerializer.DeserializeNodes(path);

            NodeManager.Instance.Clear();

            if (loadedEditorNodes != null)
            {
                loadedEditorNodes.ForEach(n => NodeManager.Instance.AddNode(n));
            }
        }

        public static void SetLastFilePath()
        {
            string fullPath = EditorUtility.OpenFolderPanel("Choose a folder containing Dialogue files only", BranchConstants.DialogueFolder, "Choose a folder containing Dialogue files only");


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