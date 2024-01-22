using Newtonsoft.Json;
using RydenCam.BranchCamEditor.Controllers;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.BranchCamEditor.Nodes;
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
    //COMBINE WITH NodeConversionFactory
    public static class NodeSerializer
    {
        public static List<EditorBaseNode> DeserializeNodes(string directoryPath)
        {
            List<EditorBaseNode> editorNodes = new List<EditorBaseNode>();
            if (Directory.Exists(directoryPath))
            {
                string assetFileName = Directory.GetFiles(directoryPath, "*.asset").FirstOrDefault();

                string assetFilePath = assetFileName.Replace("\\", "/");

                if (!string.IsNullOrEmpty(assetFilePath))
                {
                    SaveDataContainer saveDataContainer = AssetDatabase.LoadAssetAtPath<SaveDataContainer>(assetFilePath);

                    if (saveDataContainer != null)
                    {
                        List<Saveable> deserializedNodes = saveDataContainer.saveables;

                        NodeConversionFactory editorNodeFactory = new NodeConversionFactory();

                        editorNodes = deserializedNodes.Select(savenode => editorNodeFactory.CreateEditorNode(savenode)).ToList();

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
                                        node.PointOut[y].connectedTo = node_OUT.PointIn;
                                        node_OUT.PointIn.connectedTo = node.PointOut[y];
                                        ConnectionManager.Instance.AddConnection(node.PointOut[y], node_OUT.PointIn, EditorBaseNode.OnClickRemoveConnection);
                                    }
                                }
                            }
                        }
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
