using System.IO;
using UnityEngine;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.Common;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEditor;
using Assets.RydenCam.Scripts.BranchCamCC;
using Assets.RydenCam.Scripts.BranchCamEditor.Managers;

namespace RydenCam.BranchCamEditor.Serialization
{
    [ExecuteAlways]
    public static class SaveFile
    {

        public static string SaveAsFileExplorer()
        {
            var fullPath = GetSaveAsFilePath();

            if (string.IsNullOrEmpty(fullPath))
            {
                Debug.Log("No file was chosen");
            }

            return fullPath;
        }

        private static string GetSaveAsFilePath()
        {
            string fullPath;
            try
            {
                // Open a "Save As" dialog where the user can choose the file name and location
                fullPath = EditorUtility.SaveFilePanel("Save JSON File As", BranchConstants.DefaultDialogueFolder, "NewDialogue", "json");
            }
            catch (Exception)
            {
                BranchLog.Log("Could not open Save File dialog.");
                return string.Empty;
            }

            return fullPath;
        }

        public static void SaveEditorSettings()
        {
            try
            {
                string jsonSettings = JsonUtility.ToJson(EditorSettingsManager.Instance.SettingsData);
                File.WriteAllText(BranchConstants.EditorSettingsPath, jsonSettings);

                AssetDatabase.Refresh();
            }
            catch(Exception)
            {
                BranchLog.Error("An error with Saving Settings occured");
            }
        }


        public static void SaveConversation(string filePath = "")
        {
            if (!NodeManager.Instance.IsValidSequence()) return;

            try
            {
                if(string.IsNullOrEmpty(filePath)) filePath = EditorSettingsManager.Instance.LastUsedJsonPath;

                EditorSettingsManager.Instance.SetLastFilePath(filePath);

                List<SerializedNode> nodeDatas = new List<SerializedNode>();
                foreach (Node save in NodeManager.Instance.Nodes)
                {
                    string jsonNode = JsonUtility.ToJson(save);
                    
                    NodeType type = save.TypeOfNode;

                    nodeDatas.Add(new SerializedNode(type, jsonNode));
                }

                SerializedNodeList saveDataContainer = new SerializedNodeList(nodeDatas);
                string combinedJson = JsonUtility.ToJson(saveDataContainer);

                File.WriteAllText(filePath, combinedJson);

                AssetDatabase.Refresh();

                PingObject(filePath);

                BranchLog.Log("Saved File");
            }

            catch (Exception)
            {
                BranchLog.Error("An error with Saving occured");
            }

            void PingObject(string path)
            {
                path = path.Replace("\\", "/");

                if (!path.StartsWith("Assets/")) return;

                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);

                EditorGUIUtility.PingObject(obj);
            }
        }
    }
}


