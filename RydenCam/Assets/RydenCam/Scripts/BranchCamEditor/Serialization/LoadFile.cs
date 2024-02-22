using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using RydenCam.BranchCamEditor.Nodes;
using RydenCam.Common;
using RydenCam.BranchCamEditor.Managers;

namespace RydenCam.BranchCamEditor.Serialization
{
    [ExecuteAlways]
    public static class LoadFile
    {
        private static string path;

        public static void SelectDialogueWindow()
        {
            string fullPath = EditorUtility.OpenFolderPanel("Choose a folder containing Dialogue files only", BranchConstants.DialogueFolder, "Choose a folder containing Dialogue files only");

            // Get the data path of the Unity project
            string projectPath = Application.dataPath;

            // Calculate the relative path
            string relativePath = "Assets" + fullPath.Substring(projectPath.Length);

            BranchCamEditorPreferences.SetLastFilePath(relativePath);
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
        
    }
}