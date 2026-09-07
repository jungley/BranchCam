using Assets.RydenCam.Scripts.BranchCamCC;
using Assets.RydenCam.Scripts.BranchCamEditor.Managers;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RydenCam.BranchCamEditor.Serialization
{
    [ExecuteAlways]
    public static class NodeGraphSettingsManager
    {
#if UNITY_EDITOR
        public static bool Save(string filePath)
        {
            if (!NodeManager.Instance.IsValidSequence())
            {
                BranchLog.Log("Node graph not in a valid sequence. Aborting save.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                filePath = SettingsService.ShowSaveAsDialog("Save Node Graph As", BranchConstants.DefaultDialogueFolder, "NewDialogue", "json");
                if (string.IsNullOrEmpty(filePath)) return false;
            }

            NodeGraphFileWrapper saveDataContainer = new NodeGraphFileWrapper();

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
#endif

        public static bool Load(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            var container = SettingsService.Load<NodeGraphFileWrapper>(filePath);
            if (container == null)
            {
                BranchLog.Error($"Failed to load node graph from: {filePath}");
                return false;
            }

            if (container.JsonList == null)
            {
                BranchLog.Error("Node graph file has no node data.");
                return false;
            }

            List<Node> deserializedNodes = new List<Node>();
            try
            {
                foreach (var nodeJsonContent in container.JsonList)
                {
                    if (nodeJsonContent == null || string.IsNullOrEmpty(nodeJsonContent.JsonString)) continue;

                    switch (nodeJsonContent.NodeType)
                    {
                        case NodeType.StartNode:
                            StartNode startnode = JsonUtility.FromJson<StartNode>(nodeJsonContent.JsonString);
                            if (startnode != null)
                            {
                                startnode.PointIn = null;
                                deserializedNodes.Add(startnode);
                            }
                            break;

                        case NodeType.DialogueNode:
                            var dialogueNode = JsonUtility.FromJson<DialogueNode>(nodeJsonContent.JsonString);
                            if (dialogueNode != null) deserializedNodes.Add(dialogueNode);
                            break;

                        case NodeType.DecisionNode:
                            var decisionNode = JsonUtility.FromJson<DecisionNode>(nodeJsonContent.JsonString);
                            if (decisionNode != null) deserializedNodes.Add(decisionNode);
                            break;

                        case NodeType.ActionNode:
                            ActionNode actionNode = JsonUtility.FromJson<ActionNode>(nodeJsonContent.JsonString);
                            if (actionNode != null)
                            {
                                actionNode.GameActionDatas ??= new List<GameActionData>();
                                actionNode.GameActionDatas.ForEach(data => data?.AssignLoadedValues());
                                deserializedNodes.Add(actionNode);
                            }
                            break;

                        default:
                            BranchLog.Log($"Unknown node type '{nodeJsonContent.NodeType}' skipped during load.");
                            break;
                    }
                }
            }
            catch (System.Exception exception)
            {
                BranchLog.Error($"Failed to deserialize node graph: {exception.Message}");
                return false;
            }

            bool hasInvalidNode = deserializedNodes.Any(node =>
                node == null || string.IsNullOrEmpty(node.NodeId) || node.PointOut == null ||
                (node is DecisionNode decision &&
                 (decision.DecisionOptions == null || decision.DecisionOptions.Count != decision.PointOut.Count)));
            bool hasDuplicateIds = deserializedNodes
                .Where(node => node != null && !string.IsNullOrEmpty(node.NodeId))
                .GroupBy(node => node.NodeId)
                .Any(group => group.Count() > 1);
            if (deserializedNodes.Count == 0 || hasInvalidNode || hasDuplicateIds)
            {
                BranchLog.Error("Node graph contains missing or duplicate node data and was not loaded.");
                return false;
            }

            NodeManager.Instance.Clear();
            ConnectionManager.Instance.Clear();
            NodeManager.Instance.LoadNodes(deserializedNodes);
            ConnectionManager.Instance.CreateConnections(deserializedNodes);

            if (!string.IsNullOrEmpty(container.CameraShotJsonFilePath))
            {
                FilePathSaveManager.Instance.SetLastFilePath(container.CameraShotJsonFilePath, FilePathSaveManager.LastOpened_CameraShotsKey);
            }

            return true;
        }

#if UNITY_EDITOR
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
#endif
    }
}
