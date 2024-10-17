using Assets.RydenCam.Scripts.BranchCamCC;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace RydenCam.BranchCamEditor.Serialization
{
    public static class NodeSerializer
    {
        public static List<Node> DeserializeNodes(string directoryPath)
        {
            List<Node> deserializedNodes = new List<Node>();
            if (Directory.Exists(directoryPath))
            {
                string assetFileName = Directory.GetFiles(directoryPath, "*.json").FirstOrDefault();

                string assetFilePath = assetFileName?.Replace("\\", "/");

                if (!string.IsNullOrEmpty(assetFilePath))
                {
                    try
                    {
                        string jsonContent = File.ReadAllText(assetFilePath);
                        SaveDataContainer dataContainer = JsonUtility.FromJson<SaveDataContainer>(jsonContent);

                        foreach(var nodeJsonContent in dataContainer.JsonList)
                        {
                            switch(nodeJsonContent.NodeType)
                            {
                                case NodeType.StartNode:
                                    StartNode startnode = JsonUtility.FromJson<StartNode>(nodeJsonContent.JsonString);
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
                        BranchLog.Error("Error occurred in reading conversation data for \n" + directoryPath + "\n" + e.Message);
                    }
                }
            }
            return deserializedNodes;
        }
    }
}
