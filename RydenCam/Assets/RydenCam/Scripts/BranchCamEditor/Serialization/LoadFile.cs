using Assets.RydenCam.Scripts.BranchCamCC;
using Assets.RydenCam.Scripts.BranchCamEditor.Managers;
using Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender.ActorPreviewSetup;
using Assets.RydenCam.Scripts.Editor.CameraShotEdtior;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RydenCam.BranchCamEditor.Serialization
{
    [ExecuteAlways]
    public static class LoadFile
    {
        private static string SelectJSONFile()
        {
            string fullPath;
            try
            {
                fullPath = EditorUtility.OpenFilePanel("Select JSON File", BranchConstants.DefaultDialogueFolder, "json");
            }
            catch(Exception)
            {
                BranchLog.Log("Cannot open file or no file chosen.");
                return string.Empty;
            }
            return fullPath;
        }

        public static string OpenFileExplorer()
        {
            var fullpath = SelectJSONFile();

            string assetFilePath = fullpath?.Replace("\\", "/");

            return assetFilePath;
        }

        public static void LoadSaveables(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                Debug.Log("No file path provided. Please select a file.");
                return;
            }    

            NodeManager.Instance.Clear();
            ConnectionManager.Instance.Clear();

            List<Node> deserializedNodes = DeserializeNodes(filePath);

            NodeManager.Instance.LoadNodes(deserializedNodes);
            ConnectionManager.Instance.CreateConnections(deserializedNodes);
        }


        public static void LoadEditorSettings()
        {
            if (File.Exists(BranchConstants.EditorSettingsPath))
            {
                try
                {
                    string jsonContent = File.ReadAllText(BranchConstants.EditorSettingsPath);
                    EditorSettingsManager.Instance.SettingsData = JsonUtility.FromJson<SaveEditorSettingsData>(jsonContent);
                }
                catch (Exception e)
                {
                    BranchLog.Error("Error occurred in reading editor settings data for \n" + BranchConstants.EditorSettingsPath + "\n" + e.Message);
                }
            }
        }

        private static List<Node> DeserializeNodes(string filePath)
        {
            List<Node> deserializedNodes = new List<Node>();
            if (File.Exists(filePath))
            {
                try
                {
                    string jsonContent = File.ReadAllText(filePath);
                    SerializedNodeList dataContainer = JsonUtility.FromJson<SerializedNodeList>(jsonContent);

                    foreach (var nodeJsonContent in dataContainer.JsonList)
                    {
                        switch (nodeJsonContent.NodeType)
                        {
                            case NodeType.StartNode:
                                StartNode startnode = JsonUtility.FromJson<StartNode>(nodeJsonContent.JsonString);
                                startnode.PointIn = null; //Limitation with serialization, JsonUtility cannot save null
                                deserializedNodes.Add(startnode);
                                NodeManager.StartNodeAdded = true;
                                break;

                            case NodeType.DialogueNode:
                                DialogueNode dianode = JsonUtility.FromJson<DialogueNode>(nodeJsonContent.JsonString);
                                deserializedNodes.Add(dianode);
                                break;

                            case NodeType.DecisionNode:
                                DecisionNode decnode = JsonUtility.FromJson<DecisionNode>(nodeJsonContent.JsonString);
                                deserializedNodes.Add(decnode);
                                break;

                            case NodeType.ActionNode:
                                ActionNode actionNode = JsonUtility.FromJson<ActionNode>(nodeJsonContent.JsonString);
                                actionNode.GameActionDatas.ForEach(data => data.AssignLoadedValues());
                                deserializedNodes.Add(actionNode);
                                break;

                            default:
                                break;
                        }
                    }
                }


                catch (Exception e)
                {
                    BranchLog.Error("Error occurred in reading conversation data for \n" + filePath + "\n" + e.Message);
                }
                
            }
            return deserializedNodes;
        }
    }
}