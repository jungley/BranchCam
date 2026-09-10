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
    /// Builds a replacement conversation from validated Ink. The live editor graph
    /// changes only when the caller explicitly applies the completed result.
    /// </summary>
    public static class BranchCamGraphBuilder
    {
        public static List<Node> Build(InkImportModel model, string sourceGuid, string name, List<Node> previous)
        {
            var previousStart = previous.OfType<StartNode>().FirstOrDefault();
            bool isRefreshing = previousStart?.InkSourceGuid == sourceGuid;
            var previousNodes = IndexPreviousNodes(previous, isRefreshing);
            var start = CreateStartNode(model, sourceGuid, name, previousStart, isRefreshing);
            var positions = CalculateInitialPositions(model);
            var nodes = new Dictionary<string, Node>();

            foreach (var source in model.Nodes.Values)
            {
                Node node = CreateConversationNode(source, positions[source.Id]);
                AssignDialogue(node, source, model, start);
                RestoreNodeSettings(node, source, previousNodes);
                ApplyShotTag(node, source);
                UpdateConnectionOwners(node);
                nodes.Add(source.Id, node);
            }

            ConnectNodes(model, start, nodes);
            var result = new List<Node> { start };
            result.AddRange(nodes.Values);
            return result;
        }

        private static Dictionary<string, Node> IndexPreviousNodes(List<Node> previous, bool isRefreshing)
        {
            // Source IDs are meaningful only within the same Ink file. A new file
            // must not inherit another conversation's node identities or actor bindings.
            if (!isRefreshing) return new Dictionary<string, Node>();

            return previous.Where(node => !string.IsNullOrEmpty(node.InkSourceId))
                .GroupBy(node => node.InkSourceId)
                .ToDictionary(group => group.Key, group => group.First());
        }

        private static StartNode CreateStartNode(InkImportModel model, string sourceGuid,
            string name, StartNode previous, bool isRefreshing)
        {
            var start = new StartNode(new Vector2(400, 60))
            {
                SequenceName = name,
                InkSourceGuid = sourceGuid,
                InkEntryKnot = model.Entry,
                InkSourceId = "start",
                // Keep the current camera setup even when opening a different Ink file.
                CameraShotFilePath = previous?.CameraShotFilePath ?? BranchConstants.DefaultCameraShotFile,
                CameraSide = previous?.CameraSide ?? Side.Right
            };

            if (isRefreshing) RestoreStartSettings(start, previous);
            return start;
        }

        private static void RestoreStartSettings(StartNode start, StartNode previous)
        {
            // Reuse actor objects so their scene targets and starting poses survive.
            foreach (var actor in previous.ActorsInScene) start.ActorsInScene.Add(actor);
            start.StartPositionsEnabled = previous.StartPositionsEnabled;
            start.OverrideRotation = previous.OverrideRotation;
            start.ReturnToOriginalPositions = previous.ReturnToOriginalPositions;
            start.UnitySceneName = previous.UnitySceneName;
            start.NodeId = previous.NodeId;
            start.EditorPosition = previous.EditorPosition;
            start.PointOut[0].Node = start;
        }

        private static Node CreateConversationNode(InkImportNode source, Vector2 position)
        {
            Node node;
            if (source.Choices.Count > 0)
            {
                var decision = new DecisionNode(position) { DecisionOptions = source.Choices.ToList() };
                decision.PointOut = source.Choices
                    .Select(choice => new ConnectionPoint(decision, ConnectionPointType.Out)).ToList();
                node = decision;
            }
            else
            {
                node = new DialogueNode(position);
            }

            node.InkSourceId = source.Id;
            return node;
        }

        private static void AssignDialogue(Node node, InkImportNode source, InkImportModel model, StartNode start)
        {
            var dialogue = ((ITalkable)node).NodeConvodata;
            dialogue.Actor = GetOrCreateSpeaker(start, source.Actor);
            if (node is DecisionNode && dialogue.Actor == null)
            {
                // Untagged choices belong to the preceding speaker when available.
                // Preserve source order when several incoming lines name a speaker.
                var precedingLine = model.Nodes.Values.FirstOrDefault(line =>
                    line.Next.Contains(source.Id) && line.Actor != null);
                dialogue.Actor = GetOrCreateSpeaker(start, precedingLine?.Actor)
                    ?? start.ActorsInScene.FirstOrDefault();
            }

            dialogue.OppositeActor = GetOrCreateSpeaker(start, source.Target);
            dialogue.DialogTextList = new List<string> { source.Text ?? "" };
        }

        private static ActorInfo GetOrCreateSpeaker(StartNode start, string speaker)
        {
            if (string.IsNullOrEmpty(speaker)) return null;
            var actor = start.ActorsInScene.FirstOrDefault(candidate =>
                candidate.InkSpeakerName == speaker || candidate.ActorName == speaker);
            if (actor != null) return actor;

            // A named placeholder can be bound to a scene GameObject after import.
            actor = new ActorInfo { InkSpeakerName = speaker };
            start.ActorsInScene.Add(actor);
            return actor;
        }

        private static void RestoreNodeSettings(Node node, InkImportNode source,
            Dictionary<string, Node> previousNodes)
        {
            if (!previousNodes.TryGetValue(source.Id, out Node previous) || previous.GetType() != node.GetType())
                return;

            // Ink owns text and edges. Matching IDs and node types retain manual
            // positioning and camera choices; explicit source tags still take priority.
            node.NodeId = previous.NodeId;
            node.EditorPosition = previous.EditorPosition;
            var dialogue = ((ITalkable)node).NodeConvodata;
            var previousDialogue = ((ITalkable)previous).NodeConvodata;
            dialogue.ShotConfig = previousDialogue.ShotConfig;
            if (source.Actor == null) dialogue.Actor = previousDialogue.Actor;
            if (source.Target == null) dialogue.OppositeActor = previousDialogue.OppositeActor;
        }

        private static void ApplyShotTag(Node node, InkImportNode source)
        {
            if (string.IsNullOrEmpty(source.Shot)) return;
            string shotName = NormalizeShotName(source.Shot);
            var shot = CameraShotsManager.Instance.CameraShots.FirstOrDefault(candidate =>
                NormalizeShotName(candidate.ShotName) == shotName || candidate.ShotId == source.Shot);
            if (shot == null)
                throw new InvalidOperationException($"Unknown camera shot '{source.Shot}' at {source.Id}. Open its shot file or correct the tag.");

            ((ITalkable)node).NodeConvodata.ShotConfig = shot;
        }

        private static string NormalizeShotName(string name)
        {
            return (name ?? "").Replace("_", "").Replace(" ", "").Replace("-", "").ToLowerInvariant();
        }

        private static void UpdateConnectionOwners(Node node)
        {
            // The setter updates each point's serialized NodeId. Do this after
            // restoring the node identity so refreshed connections reference it correctly.
            node.PointIn.Node = node;
            foreach (var point in node.PointOut) point.Node = node;
        }

        private static void ConnectNodes(InkImportModel model, StartNode start, Dictionary<string, Node> nodes)
        {
            // All destinations must exist before wiring forward diverts or branch merges.
            start.PointOut[0].ConnectedNodeId = nodes[model.First].NodeId;
            foreach (var source in model.Nodes.Values)
            {
                for (int index = 0; index < source.Next.Count; index++)
                {
                    string destination = source.Next[index];
                    nodes[source.Id].PointOut[index].ConnectedNodeId =
                        destination == null ? null : nodes[destination].NodeId;
                }
            }
        }

        private static Dictionary<string, Vector2> CalculateInitialPositions(InkImportModel model)
        {
            var depths = new Dictionary<string, int>();

            // Use the deepest incoming route at a merge, so shared destinations
            // sit below every incoming branch. The importer has already rejected cycles.
            void AssignDepth(string id, int depth)
            {
                if (id == null || (depths.TryGetValue(id, out int current) && current >= depth)) return;
                depths[id] = depth;
                foreach (string next in model.Nodes[id].Next) AssignDepth(next, depth + 1);
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
