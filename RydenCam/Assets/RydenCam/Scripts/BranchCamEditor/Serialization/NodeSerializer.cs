using Assets.RydenCam.Scripts.BranchCamEditor.Serialization.Saveables;
using Newtonsoft.Json;
using RydenCam.BranchCamEditor.Controllers;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.BranchCamEditor.Nodes;
using RydenCam.BranchCamEditor.Serialization.Saveables;
using RydenCam.Common;
using RydenCam.SequenceData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using static RydenCam.BranchCamEditor.Nodes.EditorActionNode;

namespace RydenCam.BranchCamEditor.Serialization
{

    [System.Serializable]
    public class SimpleNodeData
    {
        [SerializeField]
        public NodeType typeOfNode;
    }


    //COMBINE WITH NodeConversionFactory
    public static class NodeSerializer
    {
        public static List<EditorBaseNode> DeserializeNodes(string directoryPath)
        {
            List<EditorBaseNode> editorNodes = new List<EditorBaseNode>();
            if (Directory.Exists(directoryPath))
            {
                string assetFileName = Directory.GetFiles(directoryPath, "*.json").FirstOrDefault();

                string assetFilePath = assetFileName?.Replace("\\", "/");

                if (!string.IsNullOrEmpty(assetFilePath))
                {
                    try
                    {
                        List<Saveable> deserializedNodes = new List<Saveable>();

                        string jsonContent = File.ReadAllText(assetFilePath);
                        SaveDataContainer dataContainer = JsonUtility.FromJson<SaveDataContainer>(jsonContent);

                        foreach(string nodeJsonContent in dataContainer.JsonList)
                        {
                            SimpleNodeData save = JsonUtility.FromJson<SimpleNodeData>(nodeJsonContent);
                            switch(save.typeOfNode)
                            {
                                case NodeType.StartNode:
                                    SaveableStartNode startnode = JsonUtility.FromJson<SaveableStartNode>(nodeJsonContent);
                                    deserializedNodes.Add(startnode);
                                    break;

                                case NodeType.DialogueNode:
                                    SaveableDialogueNode dianode = JsonUtility.FromJson<SaveableDialogueNode>(nodeJsonContent);
                                    deserializedNodes.Add(dianode);
                                    break;

                                case NodeType.DecisionNode:
                                    SaveableDecisionNode decnode = JsonUtility.FromJson<SaveableDecisionNode>(nodeJsonContent);
                                    deserializedNodes.Add(decnode);
                                    break;

                                case NodeType.ActionNode:
                                    SaveableActionNode actionNode = JsonUtility.FromJson<SaveableActionNode>(nodeJsonContent);
                                    deserializedNodes.Add(actionNode);
                                    break;

                                default:
                                    break;
                            }
                        }

                        NodeConversionFactory editorNodeFactory = new NodeConversionFactory();

                        editorNodes = deserializedNodes?.Select(savenode => editorNodeFactory.CreateEditorNode(savenode)).ToList();

                        if (editorNodes != null)
                        {
                            editorNodes.ForEach(node => NodeManager.Instance.AddNode(node));

                            //Associate Connections
                            for (int i = 0; i < deserializedNodes.Count; i++)
                            {
                                EditorBaseNode node = editorNodes[i];
                                Saveable savenode = deserializedNodes[i];
                                //Check out Connection
                                if (savenode.OUT_connTo.Count != 0)
                                {
                                    for (int y = 0; y < savenode.OUT_connTo.Count; y++)
                                    {
                                        EditorBaseNode node_OUT = NodeManager.Instance.FindNode(savenode.OUT_connTo[y]);
                                        if (node_OUT != null)
                                        {
                                            node.PointOut[y].ConnectedTo = node_OUT.PointIn;
                                            node_OUT.PointIn.ConnectedTo = node.PointOut[y];
                                            ConnectionManager.Instance.AddConnection(node.PointOut[y], node_OUT.PointIn, EditorBaseNode.OnClickRemoveConnection);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        BranchLog.Error("Error occurred in reading conversation data for \n" + directoryPath + "\n" + e.Message);
                    }
                }
            }

            return editorNodes;
        }

        public static List<Saveable> SerializeNodes(List<EditorBaseNode> nodeList)
        {
            NodeConversionFactory saveNodeFactory = new NodeConversionFactory();
            List<Saveable> serializedNodes = nodeList.Select(node => saveNodeFactory.CreateSaveNode(node)).ToList();
            return serializedNodes;
        }
    }
}
