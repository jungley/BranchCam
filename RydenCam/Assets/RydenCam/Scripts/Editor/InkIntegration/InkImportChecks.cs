using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Assets.RydenCam.Scripts.BranchCamCC;
using UnityEditor;
using UnityEngine;

namespace RydenCam.Editor.InkIntegration
{
    public static class InkImportChecks
    {
        // Keep verification independent of the editable portfolio sample.
        private const string TestConversation = @"-> blacksmith_intro

=== blacksmith_intro ===
Blacksmith: You're not from around here. # id:intro # shot:portrait
Blacksmith: The northern road has been quiet lately. # id:road
* [Just passing through.]
    Player: I'm heading north. # id:north # shot:over_shoulder # target:Blacksmith
    -> friendly
* [None of your business.]
    Player: That's none of your concern. # id:refusal
    -> hostile

=== friendly ===
Blacksmith: Fair enough. You'll need a sound blade. # id:offer
Player: What happened to the road? # id:question
Blacksmith: The bridge is gone. Take the old mill path. # id:warning
* [Thank the blacksmith.]
    Player: Thank you. I'll remember that. # id:thanks
    -> farewell
* [Ask about supplies.]
    Player: Can I buy supplies here? # id:supplies
    Blacksmith: My sister runs the store next door. # id:store
    -> farewell

=== hostile ===
Blacksmith: Watch your attitude. # id:attitude
Player: Sorry. It's been a long journey. # id:apology
-> farewell

=== farewell ===
Blacksmith: Safe travels, stranger. # id:farewell # shot:frame_share # target:Player
-> END
";

        private static void CheckDialogueEditing(Action<bool, string> check)
        {
            // Exercise the context menu commands without changing the open graph.
            foreach (string sourceId in new[] { "", "id:imported" })
            {
                var node = new DialogueNode(Vector2.zero) { InkSourceId = sourceId };
                node.NodeConvodata.DialogTextList.Clear();
                node.NodeConvodata.DialogTextList.Add("First");
                node.NodeConvodata.DialogTextList.Add("Last");
                var command = new Assets.RydenCam.Scripts.NodeCommands.DialogueNodeCommand(node);
                try
                {
                    command.AddSpeakingEntry(1);
                    check(node.NodeConvodata.DialogTextList.SequenceEqual(new[] { "First", "", "Last" }),
                        "Add inserts dialogue after the selected entry: " + sourceId);
                    node.NodeConvodata.DialogTextList[1] = "New dialogue";
                    var restored = JsonUtility.FromJson<DialogueNode>(JsonUtility.ToJson(node));
                    check(restored.NodeConvodata.DialogTextList[1] == "New dialogue" && restored.InkSourceId == sourceId,
                        "Added dialogue and Ink identity survive serialization: " + sourceId);
                    command.RemoveSpeakingEntry(1);
                    check(node.NodeConvodata.DialogTextList.SequenceEqual(new[] { "First", "Last" }),
                        "Remove preserves neighboring dialogue: " + sourceId);
                }
                finally
                {
                    RydenCam.BranchCamEditor.Managers.NodeManager.Instance.NodeCommandLookup.RemoveByKey(node);
                }
            }
        }
        [MenuItem("Tools/BranchCam/Run Ink Import Checks")]
        public static void Run()
        {
            var report = new List<string>();
            try
            {
                InkImportModel Import(string text) => new InkBranchCamImporter().Import(text, "test.ink");
                
                void Check(bool condition, string name) 
                {
                    if (!condition)
                    {
                        throw new Exception(name);
                    }
                    report.Add("PASS " + name);
                }

                var sample = Import(TestConversation);
                CheckDialogueEditing(Check);
                
                Check(sample.Nodes.Count == 15, "Sample has 13 dialogue nodes and 2 decisions (actual " + sample.Nodes.Count + ")");
                Check(sample.Nodes.Values.Sum(n => n.Choices.Count) == 4, "All four choices imported");
                Check(sample.Nodes["id:north"].Actor == "Player" && sample.Nodes["id:north"].Target == "Blacksmith" && sample.Nodes["id:north"].Shot == "over_shoulder", "Speaker and camera tags");
                Check(sample.Nodes["id:farewell"].Next.Single() == null, "END terminates flow");
                Check(sample.Nodes.Values.Count(n => n.Next.Contains("id:farewell")) == 3, "Three branches merge at farewell");
                
                
                // Refresh should update Ink-owned text while retaining editor layout and cameras.
                var originalGraph = BranchCamGraphBuilder.Build(sample, "test-guid", "Test", new List<Node>());
                var originalIntroNode = originalGraph.OfType<DialogueNode>().First(n => n.InkSourceId == "id:intro");
                originalIntroNode.EditorPosition = new Vector2(123, 456);
                var refreshed = BranchCamGraphBuilder.Build(Import(TestConversation.Replace("You're not from around here.", "Welcome back.")), "test-guid", "Test", originalGraph);
                var refreshedIntroNode = refreshed.OfType<DialogueNode>().First(n => n.InkSourceId == "id:intro");
                
                Check(refreshedIntroNode.NodeId == originalIntroNode.NodeId && refreshedIntroNode.EditorPosition == originalIntroNode.EditorPosition && refreshedIntroNode.NodeConvodata.DialogTextList.Single() == "Welcome back.", "Refresh changes dialogue and preserves node identity/layout");
                Check(refreshed.OfType<StartNode>().Single().ActorsInScene.Count == 2, "Refresh preserves two speaker bindings");
                
                var manuallyConfiguredNode = originalGraph.OfType<DialogueNode>().First(n => n.InkSourceId == "id:road");
                var custom = new RydenCam.BranchCamEditor.BranchCam.CameraShotConfiguration("Test camera");
                manuallyConfiguredNode.NodeConvodata.ShotConfig = custom;
                var retained = BranchCamGraphBuilder.Build(sample, "test-guid", "Test", originalGraph);
                Check(object.ReferenceEquals(retained.OfType<DialogueNode>().First(n => n.InkSourceId == "id:road").NodeConvodata.ShotConfig, custom), "Refresh preserves manually assigned camera");
                // Wire this isolated graph directly so the checks do not replace the open scene.
                var nodesById = originalGraph.ToDictionary(n => n.NodeId);
                foreach (var node in originalGraph)
                {
                    foreach (var output in node.PointOut)
                    {
                        if (!string.IsNullOrEmpty(output.ConnectedNodeId))
                        {
                            output.ConnectedTo = nodesById[output.ConnectedNodeId].PointIn;
                        }
                    }
                }
                int endings = 0;
                void VisitAllRuntimeBranches(Node node)
                {
                    if (node == null)
                    {
                        endings++;
                        return;
                    }
                    if (node is DecisionNode decision)
                    {
                        for (int i = 0; i < decision.DecisionOptions.Count; i++)
                        {
                            VisitAllRuntimeBranches(decision.MakeDecision(i));
                        }
                    }
                    else
                    {
                        VisitAllRuntimeBranches(node.GetNextNode());
                    }
                }
                VisitAllRuntimeBranches(originalGraph[0]);
                Check(endings == 3, "BranchCam runtime traversal reaches all three endings");
                var knot = Import("=== intro ===\nActor: Hello.\n-> END\n");
                Check(knot.Nodes.Count == 1 && knot.Entry == "intro", "First knot default entry");
                var plain = Import("A: One.\nB: Two.\nA: Three.\n-> END");
                Check(plain.Nodes.Count == 3, "Untagged dialogue has distinct stable content paths");
                var nested = Import("A: Hello\n* [Outer]\n    ** [Inner one]\n        A: One\n        -> END\n    ** [Inner two]\n        A: Two\n        -> END\n* [Other]\n    A: Other\n    -> END");
                Check(nested.Nodes.Values.Count(n => n.Choices.Count > 0) == 2, "Nested choices imported");
                var restoredStart = JsonUtility.FromJson<StartNode>(JsonUtility.ToJson(originalGraph[0]));
                var restoredLine = JsonUtility.FromJson<DialogueNode>(JsonUtility.ToJson(originalIntroNode));
                Check(restoredStart.InkSourceGuid == "test-guid" && restoredStart.ActorsInScene.Count == 2 && restoredLine.InkSourceId == "id:intro", "Ink source, speakers and node IDs survive scene serialization");
                // Unsupported dynamic flow must fail instead of producing a misleading graph.
                var unsupportedStories = new[]
                {
                    "VAR score = 1\nHello\n-> END",
                    "{true: Hello}\n-> END",
                    "-> loop\n=== loop ===\nHello\n-> loop",
                    "-> missing",
                    "Hello # id:duplicate\nGoodbye # id:duplicate\n-> END",
                    "=== function f() ===\n~ return 1"
                };
                foreach (var text in unsupportedStories)
                {
                    bool rejected = false;
                    try
                    {
                        Import(text);
                    }
                    catch (InvalidOperationException)
                    {
                        rejected = true;
                    }
                    Check(rejected, "Unsupported/invalid flow rejected: " + text.Split('\n')[0]);
                }
                report.Add("ALL CHECKS PASSED");
            }
            catch (Exception e)
            {
                report.Add("FAIL " + e);
            }
            File.WriteAllLines("Temp/branchcam-ink-checks.txt", report);
            Debug.Log(string.Join("\n", report));
        }
    }
}
