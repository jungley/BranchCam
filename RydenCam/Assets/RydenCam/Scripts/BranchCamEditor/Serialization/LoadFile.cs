using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using RydenCam.BranchCamEditor.Nodes;
using RydenCam.Common;
using RydenCam.BranchCamEditor.Managers;
using System;

namespace RydenCam.BranchCamEditor.Serialization
{
    [ExecuteAlways]
    public static class LoadFile
    {
        private static string path;

        public static bool SelectDialogueWindow(string folderTitle, string defaultName, bool isSaveAs)
        {
            string fullPath = EditorUtility.OpenFolderPanel(folderTitle, BranchCamEditorPreferences.GetLastFileFolderPath(), defaultName);

            //Cancel Button Pressed
            if (string.IsNullOrEmpty(fullPath)) return false; 
         

            // Get the data path of the Unity project
            string projectPath = Application.dataPath;

            try
            {
                // Calculate the relative path
                string relativePath = "Assets" + fullPath.Substring(projectPath.Length);

                if (isSaveAs)
                {
                    EditorStartNode startNodeRef = (EditorStartNode)NodeManager.Instance.StartNode;
                    string name = string.IsNullOrWhiteSpace(startNodeRef.SequenceName) ? "NewDialogueFile" : startNodeRef.SequenceName;

                    relativePath += "/" + name;
                }

                BranchCamEditorPreferences.SetLastFilePath(relativePath);
            }
            catch (Exception)
            {
                BranchLog.Log("Cannot open file or no file chosen.");
            }

            return true;
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