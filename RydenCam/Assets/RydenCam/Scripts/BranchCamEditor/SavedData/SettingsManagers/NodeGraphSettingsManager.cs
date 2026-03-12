using Assets.RydenCam.Scripts.BranchCamCC;
using Assets.RydenCam.Scripts.BranchCamEditor.Managers;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.Common;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

namespace RydenCam.BranchCamEditor.Serialization
{
    [ExecuteAlways]
    public static class NodeGraphSettingsManager
    {
        public static bool Save(string filePath)
        {
            if (!NodeManager.Instance.IsValidSequence())
            {
                BranchLog.Log("Node graph not in a valid sequence. Aborting save.");
                return false;
            }

            if (string.IsNullOrEmpty(filePath))
            {
                Debug.Log("No file path provided. Aborting save");
                return false;
            }

            if (string.IsNullOrEmpty(filePath))
            {
                filePath = SettingsService.ShowSaveAsDialog("Save Node Graph As", BranchConstants.DefaultDialogueFolder, "NewDialogue", "json");
                if (string.IsNullOrEmpty(filePath)) return false;
            }

            NodeGraphFileWrapper saveDataContainer = new NodeGraphFileWrapper();

            //Set the file path to load camera shots
            FilePathSaveManager.Instance.SetLastFilePath(saveDataContainer.CameraShotJsonFilePath, FilePathSaveManager.LastOpened_CameraShotsKey);
            
            
            bool ok = SettingsService.Save(saveDataContainer, filePath, FilePathSaveManager.LastOpened_NodeGraphKey);
            if (ok) BranchLog.Log($"Saved node graph to {filePath}");
            else BranchLog.Error("Failed saving node graph.");

            return ok;
        }

        public static bool SaveAs()
        {
            string path = SettingsService.ShowSaveAsDialog("Save Node Graph As", BranchConstants.DefaultDialogueFolder, "New Dialogue", "json");
            if (string.IsNullOrEmpty(path)) return false;
            return Save(path);
        }

        public static void OpenAndLoad()
        {
            string path = SettingsService.ShowOpenFileDialog("Select JSON File", BranchConstants.DefaultDialogueFolder, "json");
            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("No file selected.");
                return;
            }

            FilePathSaveManager.Instance.SetLastFilePath(path, FilePathSaveManager.LastOpened_NodeGraphKey);
            Load(path);
        }

        public static void Load(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                Debug.Log("No file path provided. Please select a file.");
                return;
            }

            // Reuse the existing logic in LoadFile.DeserializeNodes if you prefer.
            // Here we read container and rebuild nodes similarly.
            var container = SettingsService.Load<NodeGraphFileWrapper>(filePath);
            if (container == null)
            {
                BranchLog.Error("Failed to load node graph: container null");
                return;
            }

            List<Node> deserializedNodes = new List<Node>();
            foreach (var nodeJsonContent in container.JsonList)
            {
                switch (nodeJsonContent.NodeType)
                {
                    case NodeType.StartNode:
                        StartNode startnode = JsonUtility.FromJson<StartNode>(nodeJsonContent.JsonString);
                        startnode.PointIn = null;
                        deserializedNodes.Add(startnode);
                        break;

                    case NodeType.DialogueNode:
                        deserializedNodes.Add(JsonUtility.FromJson<DialogueNode>(nodeJsonContent.JsonString));
                        break;

                    case NodeType.DecisionNode:
                        deserializedNodes.Add(JsonUtility.FromJson<DecisionNode>(nodeJsonContent.JsonString));
                        break;

                    case NodeType.ActionNode:
                        ActionNode actionNode = JsonUtility.FromJson<ActionNode>(nodeJsonContent.JsonString);
                        actionNode.GameActionDatas.ForEach(data => data.AssignLoadedValues());
                        deserializedNodes.Add(actionNode);
                        break;
                }
            }
            NodeManager.Instance.Clear();
            ConnectionManager.Instance.Clear();
            NodeManager.Instance.LoadNodes(deserializedNodes);
            ConnectionManager.Instance.CreateConnections(deserializedNodes);

            //Set Camera
            FilePathSaveManager.Instance.SetLastFilePath(container.CameraShotJsonFilePath, FilePathSaveManager.LastOpened_CameraShotsKey);

        }

        public static void New()
        {
            NodeManager.Instance.ClearActorsInScene();
            NodeManager.Instance.Clear();
            ConnectionManager.Instance.Clear();
            NodeManager.Instance.ActiveNode = null;
            FilePathSaveManager.Instance.SetLastFilePath(string.Empty, FilePathSaveManager.LastOpened_NodeGraphKey);
            FilePathSaveManager.Instance.SetLastFilePath(string.Empty, FilePathSaveManager.LastOpened_CameraShotsKey);
            BranchLog.Log("New node graph (cleared).");
        }
    }
}