using System;
using System.Collections.Generic;
using System.Linq;
using Assets.RydenCam.Scripts.BranchCamCC;
using Assets.RydenCam.Scripts.Editor.CameraShotEditor;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.BranchCamEditor.Nodes.Connections;
using RydenCam.Common;
using RydenCam.SequenceData;
using UnityEngine;

namespace RydenCam.Editor.InkIntegration
{
    /// <summary>
    /// Converts the validated Ink model into BranchCam nodes. Build prepares a
    /// replacement without changing the open graph; Apply makes it visible.
    /// </summary>
    public static class BranchCamGraphBuilder
    {
        public static List<Node> Build(InkImportModel model, string sourceGuid, string name, List<Node> previous)
        {
            var previousStartNode = previous.OfType<StartNode>().FirstOrDefault();
            bool isRefreshing = previousStartNode?.InkSourceGuid == sourceGuid;
            // Source IDs identify the same narrative beat across refreshes. Only
            // reuse authoring settings when refreshing the same Ink asset.
            var previousNodesBySourceId = isRefreshing ? previous.Where(n => !string.IsNullOrEmpty(n.InkSourceId))
                .GroupBy(n => n.InkSourceId).ToDictionary(g => g.Key, g => g.First()) : new Dictionary<string, Node>();
            var start = new StartNode(new Vector2(400, 60)) {
                SequenceName = name,
                InkSourceGuid = sourceGuid,
                InkEntryKnot = model.Entry,
                InkSourceId = "start",
                CameraShotFilePath = previousStartNode?.CameraShotFilePath ?? BranchConstants.DefaultCameraShotFile,
                CameraSide = previousStartNode?.CameraSide ?? Side.Right
            };
            if (isRefreshing)
            {
                foreach (var actor in previousStartNode.ActorsInScene) start.ActorsInScene.Add(actor);
                start.StartPositionsEnabled = previousStartNode.StartPositionsEnabled;
                start.OverrideRotation = previousStartNode.OverrideRotation;
                start.ReturnToOriginalPositions = previousStartNode.ReturnToOriginalPositions;
                start.UnitySceneName = previousStartNode.UnitySceneName;
            }
            if (isRefreshing)
            {
                start.NodeId = previousStartNode.NodeId;
                start.EditorPosition = previousStartNode.EditorPosition;
                start.PointOut[0].Node = start;
            }
            // Speakers remain placeholders until the user picks scene actors.
            ActorInfo GetOrCreateSpeaker(string speaker)
            {
                if (string.IsNullOrEmpty(speaker)) return null;
                var found = start.ActorsInScene.FirstOrDefault(a => a.InkSpeakerName == speaker || a.ActorName == speaker);
                if (found != null) return found;
                found = new ActorInfo { InkSpeakerName = speaker };
                start.ActorsInScene.Add(found);
                return found;
            }
            var nodes = new Dictionary<string, Node>();
            var positions = CalculateInitialPositions(model);
            foreach (var item in model.Nodes.Values)
            {
                Node node;
                if (item.Choices.Count > 0)
                {
                    var decision = new DecisionNode(Vector2.zero) { DecisionOptions = item.Choices.ToList() };
                    decision.PointOut = item.Choices.Select(_ => new ConnectionPoint(decision, ConnectionPointType.Out)).ToList();
                    node = decision;
                }
                else node = new DialogueNode(Vector2.zero);
                node.InkSourceId = item.Id;
                node.EditorPosition = positions[item.Id];
                var data = ((ITalkable)node).NodeConvodata;
                data.Actor = GetOrCreateSpeaker(item.Actor);
                if (node is DecisionNode && data.Actor == null)
                    data.Actor = GetOrCreateSpeaker(model.Nodes.Values.FirstOrDefault(n => n.Next.Contains(item.Id) && n.Actor != null)?.Actor)
                        ?? start.ActorsInScene.FirstOrDefault();
                data.OppositeActor = GetOrCreateSpeaker(item.Target);
                data.DialogTextList = new List<string> { item.Text ?? "" };
                // Text and connections come from Ink. Camera choices and manual
                // positioning survive when the source ID and node type still match.
                if (previousNodesBySourceId.TryGetValue(item.Id, out Node previousNode) && previousNode.GetType() == node.GetType())
                {
                    node.NodeId = previousNode.NodeId;
                    node.EditorPosition = previousNode.EditorPosition;
                    var previousConversation = ((ITalkable)previousNode).NodeConvodata;
                    data.ShotConfig = previousConversation.ShotConfig;
                    if (item.Actor == null) data.Actor = previousConversation.Actor;
                    if (item.Target == null) data.OppositeActor = previousConversation.OppositeActor;
                }
                // An explicit Ink shot tag takes precedence over a saved camera.
                if (!string.IsNullOrEmpty(item.Shot))
                {
                    string Normalize(string s) => (s ?? "").Replace("_", "").Replace(" ", "").Replace("-", "").ToLowerInvariant();
                    var shot = CameraShotsManager.Instance.CameraShots.FirstOrDefault(s => Normalize(s.ShotName) == Normalize(item.Shot) || s.ShotId == item.Shot);
                    if (shot == null) throw new InvalidOperationException($"Unknown camera shot '{item.Shot}' at {item.Id}. Open its shot file or correct the tag.");
                    data.ShotConfig = shot;
                }
                // Setting Node also updates the connection's serialized NodeId.
                // This is necessary after restoring a previous node identity above.
                node.PointIn.Node = node;
                foreach (var point in node.PointOut) point.Node = node;
                nodes.Add(item.Id, node);
            }
            // Wire after creating every node so forward diverts and shared branch
            // destinations can resolve without depending on creation order.
            start.PointOut[0].ConnectedNodeId = nodes[model.First].NodeId;
            foreach (var item in model.Nodes.Values)
                for (int i = 0; i < item.Next.Count; i++)
                    nodes[item.Id].PointOut[i].ConnectedNodeId = item.Next[i] == null ? null : nodes[item.Next[i]].NodeId;
            var result = new List<Node> { start };
            result.AddRange(nodes.Values);
            return result;
        }

        private static Dictionary<string, Vector2> CalculateInitialPositions(InkImportModel model)
        {
            var depths = new Dictionary<string, int>();

            // Use the deepest incoming route at a merge, so a shared destination
            // appears below every branch that leads to it. The importer rejects cycles.
            void AssignDepth(string id, int depth)
            {
                if (id == null || (depths.TryGetValue(id, out int current) && current >= depth))
                    return;

                depths[id] = depth;
                foreach (var next in model.Nodes[id].Next)
                    AssignDepth(next, depth + 1);
            }

            AssignDepth(model.First, 0);
            var positions = new Dictionary<string, Vector2>();
            foreach (var level in depths.GroupBy(pair => pair.Value))
            {
                int column = 0;
                float centerColumn = (level.Count() - 1) * 0.5f;
                foreach (var nodeDepth in level)
                {
                    float x = 400 + (column++ - centerColumn) * 480;
                    float y = 220 + level.Key * 230;
                    positions[nodeDepth.Key] = new Vector2(x, y);
                }
            }
            return positions;
        }

        /// <summary>Replace the editor graph and rebuild its live connection objects.</summary>
        public static void Apply(List<Node> nodes)
        {
            NodeManager.Instance.Clear();
            ConnectionManager.Instance.Clear();
            NodeManager.Instance.LoadNodes(nodes);
            ConnectionManager.Instance.CreateConnections(nodes);
            NodeManager.Instance.ActiveNode = nodes.FirstOrDefault();
        }
    }
}
