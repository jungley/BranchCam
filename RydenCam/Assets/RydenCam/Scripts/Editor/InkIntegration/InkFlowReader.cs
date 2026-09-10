using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ink.Runtime;

namespace RydenCam.Editor.InkIntegration
{
    /// <summary>
    /// Reads one compiled story, exploring each choice from a saved runtime state.
    /// This object belongs to a single import and never changes Unity's live graph.
    /// </summary>
    internal sealed class InkFlowReader
    {
        private const int MaximumTraversalSteps = 1000;
        private const double MaximumImportSeconds = 8;
        private const float RuntimeTimeSliceMilliseconds = 5;

        private readonly Story story;
        private readonly InkImportModel model;
        private readonly Stopwatch timer;
        private readonly HashSet<string> activeFlowPaths = new HashSet<string>();
        private readonly Dictionary<string, string> sourcePathByNodeId = new Dictionary<string, string>();
        private int traversalSteps;

        public InkFlowReader(Story story, InkImportModel model, Stopwatch timer)
        {
            this.story = story;
            this.model = model;
            this.timer = timer;
        }

        public void Read()
        {
            model.First = VisitCurrentFlow();
            if (model.First == null)
                throw new InvalidOperationException("The selected entry has no dialogue or choices.");

            ValidateNoGraphCycles();
        }

        private string VisitCurrentFlow()
        {
            CheckTraversalLimit();
            string flowPath = GetCurrentFlowPath();
            if (!activeFlowPaths.Add(flowPath))
                throw new InvalidOperationException("A loop was found. This importer supports acyclic conversations only.");

            try
            {
                while (story.canContinue)
                {
                    ContinueToNextLine();
                    string text = story.currentText.Trim();
                    if (text.Length == 0) continue;

                    return ImportDialogue(text, flowPath);
                }

                return ImportCurrentChoices();
            }
            finally
            {
                activeFlowPaths.Remove(flowPath);
            }
        }

        private void CheckTraversalLimit()
        {
            if (++traversalSteps > MaximumTraversalSteps || timer.Elapsed.TotalSeconds > MaximumImportSeconds)
            {
                throw new InvalidOperationException(
                    "Import exceeded the static graph limit (1,000 steps / 8 seconds). Loops and dynamic stories are not supported.");
            }
        }

        private string GetCurrentFlowPath()
        {
            // At a choice boundary Ink may have no current instruction path.
            return story.state.currentPathString ??
                "choices:" + string.Join(",", story.currentChoices.Select(choice => choice.sourcePath));
        }

        private void ContinueToNextLine()
        {
            // Continue() can hang on a loop that emits no text. Small execution
            // slices let us enforce the deadline even if Ink never reaches a line.
            do
            {
                story.ContinueAsync(RuntimeTimeSliceMilliseconds);
                if (timer.Elapsed.TotalSeconds > MaximumImportSeconds)
                    throw new InvalidOperationException("Ink execution did not finish. Check for loops.");
            }
            while (!story.asyncContinueComplete);
        }

        private string ImportDialogue(string text, string fallbackPath)
        {
            var output = story.state.outputStream.OfType<StringValue>()
                .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value.value));
            string sourceId = "line:" + (output?.path.ToString() ?? fallbackPath);
            var node = new InkImportNode { Id = sourceId, Text = text };
            ApplyTags(node, story.currentTags);
            ReadSpeakerPrefix(node);

            if (model.Nodes.ContainsKey(node.Id))
            {
                // Two branches may reach the same source line. Two different lines
                // claiming the same explicit # id, however, would hide dialogue.
                if (sourcePathByNodeId[node.Id] != sourceId)
                    throw new InvalidOperationException($"Duplicate Ink id tag '{node.Id}'. Give each dialogue line a unique id.");
                return node.Id;
            }

            sourcePathByNodeId[node.Id] = sourceId;
            model.Nodes.Add(node.Id, node);
            node.Next.Add(VisitCurrentFlow());
            return node.Id;
        }

        private static void ReadSpeakerPrefix(InkImportNode node)
        {
            int colon = node.Text.IndexOf(':');
            if (colon <= 0 || colon >= 60) return;

            string prefix = node.Text.Substring(0, colon);
            if (prefix.Contains("\n")) return;

            // An explicit actor tag wins over "Speaker: dialogue" notation.
            node.Actor = node.Actor ?? prefix.Trim();
            node.Text = node.Text.Substring(colon + 1).Trim();
        }

        private string ImportCurrentChoices()
        {
            var choices = story.currentChoices.ToList();
            if (choices.Count == 0) return null;

            string decisionId = "choice:" + choices[0].sourcePath;
            if (model.Nodes.ContainsKey(decisionId)) return decisionId;

            var decision = new InkImportNode { Id = decisionId };
            model.Nodes.Add(decisionId, decision);
            string snapshot = story.state.ToJson();
            foreach (var choice in choices)
            {
                WarnAboutChoiceCameraTags(choice);
                decision.Choices.Add(choice.text.Trim());

                // Restore before every branch: visiting the previous choice consumed
                // runtime state that its siblings must still be able to read.
                story.state.LoadJson(snapshot);
                story.ChooseChoiceIndex(choice.index);
                decision.Next.Add(VisitCurrentFlow());
            }

            return decisionId;
        }

        private void WarnAboutChoiceCameraTags(Choice choice)
        {
            if (choice.tags == null) return;
            bool hasCameraTag = choice.tags.Any(tag => tag.StartsWith("actor:") ||
                tag.StartsWith("shot:") || tag.StartsWith("target:"));
            if (hasCameraTag)
                model.Warnings.Add("Choice camera tags are not applied; put camera tags on the branch's dialogue line.");
        }

        private static void ApplyTags(InkImportNode node, List<string> tags)
        {
            if (tags == null) return;
            foreach (string tag in tags)
            {
                int separator = tag.IndexOf(':');
                if (separator < 0) continue;

                string name = tag.Substring(0, separator).Trim().ToLowerInvariant();
                string value = tag.Substring(separator + 1).Trim();
                switch (name)
                {
                    case "actor": node.Actor = value; break;
                    case "target": node.Target = value; break;
                    case "shot": node.Shot = value; break;
                    case "id": node.Id = "id:" + value; break;
                }
            }
        }

        private void ValidateNoGraphCycles()
        {
            var completed = new HashSet<string>();
            var visiting = new HashSet<string>();

            void Visit(string id)
            {
                if (id == null || completed.Contains(id)) return;

                // A completed node is a valid branch merge. A node on the current
                // recursion stack means this route loops back into itself.
                if (!visiting.Add(id))
                    throw new InvalidOperationException("Loops are not supported by the static Ink importer.");

                foreach (string next in model.Nodes[id].Next) Visit(next);
                visiting.Remove(id);
                completed.Add(id);
            }

            Visit(model.First);
        }
    }
}
