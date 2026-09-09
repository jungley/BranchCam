using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ink;
using Ink.Runtime;

namespace RydenCam.Editor.InkIntegration
{
    /// <summary>
    /// A conversation described without Unity scene objects. Keeping this separate
    /// lets us validate an import before replacing the user's BranchCam graph.
    /// </summary>
    public sealed class InkImportModel
    {
        public string Entry;
        public string First;
        public readonly Dictionary<string, InkImportNode> Nodes = new Dictionary<string, InkImportNode>();
        public readonly List<string> Warnings = new List<string>();
    }

    public sealed class InkImportNode
    {
        public string Id;
        public string Text;
        public string Actor;
        public string Target;
        public string Shot;
        public readonly List<string> Choices = new List<string>();
        // Dialogue has one successor; decisions have one per choice in Choices order.
        // A null successor represents the end of a conversation.
        public readonly List<string> Next = new List<string>();
    }

    // Ink owns parsing and execution. This adapter only accepts static narrative flow,
    // then snapshots the runtime at each choice to visit every supported branch.
    public sealed class InkBranchCamImporter
    {
        private const int MaximumTraversalSteps = 1000;
        private const double MaximumImportSeconds = 8;
        private const float RuntimeTimeSliceMilliseconds = 5;
        readonly Stopwatch timer = new Stopwatch();
        InkImportModel model;
        Ink.Runtime.Story story;
        readonly HashSet<string> activeFlowPaths = new HashSet<string>();
        readonly Dictionary<string, string> sourcePathByNodeId = new Dictionary<string, string>();
        int traversalSteps;

        public InkImportModel Import(string source, string filename, string entry = "")
        {
            model = new InkImportModel();
            activeFlowPaths.Clear();
            sourcePathByNodeId.Clear();
            traversalSteps = 0;
            timer.Restart();
            var errors = new List<string>();
            var compiler = new Compiler(source, new Compiler.Options {
                sourceFilename = filename,
                errorHandler = (message, type) => {
                    if (type == ErrorType.Error) errors.Add(message); else model.Warnings.Add(message);
                }
            });
            var parsed = compiler.Parse();
            if (parsed == null || errors.Count > 0) throw new InvalidOperationException(string.Join("\n", errors));
            // Reject dynamic constructs before running the story: the editor graph
            // describes static branches, not every possible variable-dependent state.
            ValidateSupportedSyntax(parsed);
            story = parsed.ExportRuntime((message, type) => {
                if (type == ErrorType.Error) errors.Add(message); else model.Warnings.Add(message);
            });
            if (story == null || errors.Count > 0) throw new InvalidOperationException(string.Join("\n", errors));
            story.onError += (message, type) => { if (type == ErrorType.Error) throw new InvalidOperationException(message); };
            // Support both root-level narrative and files containing only knots.
            if (!string.IsNullOrWhiteSpace(entry)) story.ChoosePathString(entry.Trim());
            else if (!story.canContinue || !HasRootNarrative(parsed))
            {
                entry = parsed.content.OfType<Ink.Parsed.Knot>().FirstOrDefault()?.name ?? "";
                if (entry.Length > 0) story.ChoosePathString(entry);
            }
            model.Entry = entry;
            model.First = VisitCurrentFlow();
            if (model.First == null) throw new InvalidOperationException("The selected entry has no dialogue or choices.");
            ValidateNoGraphCycles();
            return model;
        }

        private void ValidateNoGraphCycles()
        {
            // Reaching a completed node is a valid merge. Reaching a node still on
            // the current recursion stack means a branch loops back into itself.
            var complete = new HashSet<string>();
            var visiting = new HashSet<string>();
            
            void CheckCycles(string id)
            {
                if (id == null || complete.Contains(id))
                {
                    return;
                }
                if (!visiting.Add(id))
                {
                    throw new InvalidOperationException("Loops are not supported by the static Ink importer.");
                }
                
                foreach (var next in model.Nodes[id].Next) CheckCycles(next);
                visiting.Remove(id);
                complete.Add(id);
            }

            CheckCycles(model.First);
        }



        static bool HasRootNarrative(Ink.Parsed.Story parsed) => parsed.content
            .Where(x => !(x is Ink.Parsed.Knot)).Any(HasNarrative);
        static bool HasNarrative(Ink.Parsed.Object obj) => obj is Ink.Parsed.Choice ||
            (obj is Ink.Parsed.Text t && !string.IsNullOrWhiteSpace(t.text)) ||
            (obj is Ink.Parsed.Divert d && !d.isEnd && !d.isDone) ||
            (obj.content != null && obj.content.Any(HasNarrative));

        static void ValidateSupportedSyntax(Ink.Parsed.Object obj)
        {
            var allowed = new[] { "Story", "Knot", "Weave", "ContentList", "Text", "Choice", "Divert", "Gather", "Tag" };
            bool unsupported = !allowed.Contains(obj.GetType().Name);
            if (obj is Ink.Parsed.FlowBase flow) unsupported |= flow.isFunction || flow.hasParameters;
            if (obj is Ink.Parsed.Choice choice) unsupported |= choice.condition != null || choice.isInvisibleDefault;
            if (obj is Ink.Parsed.Divert divert) unsupported |= divert.isTunnel || divert.isThread || divert.isFunctionCall || (divert.arguments?.Count > 0);
            if (unsupported) throw new InvalidOperationException($"Line {obj.debugMetadata?.startLineNumber}: unsupported Ink construct {obj.GetType().Name}. Use dialogue, knots, choices, static diverts, END and tags.");
            if (obj.content != null) foreach (var child in obj.content) ValidateSupportedSyntax(child);
        }

        string VisitCurrentFlow()
        {
            if (++traversalSteps > MaximumTraversalSteps || timer.Elapsed.TotalSeconds > MaximumImportSeconds)
                throw new InvalidOperationException("Import exceeded the static graph limit (1,000 steps / 8 seconds). Loops and dynamic stories are not supported.");
            
            string stateKey = story.state.currentPathString ?? ("choices:" + string.Join(",", story.currentChoices.Select(c => c.sourcePath)));
            
            if (!activeFlowPaths.Add(stateKey))
            {
                throw new InvalidOperationException("A loop was found. This importer supports acyclic conversations only.");
            }
            try
            {
                while (story.canContinue)
                {
                    ContinueToNextLine();

                    string text = story.currentText.Trim();
                    if (text.Length == 0)
                    {
                        continue;
                    }
                    StringValue value = story.state.outputStream.OfType<Ink.Runtime.StringValue>().FirstOrDefault(v => !string.IsNullOrWhiteSpace(v.value));
                    
                    string id = "line:" + (value?.path.ToString() ?? stateKey);
                    var node = new InkImportNode { Id = id, Text = text };
                    ApplyTags(node, story.currentTags);
                    int colon = text.IndexOf(':');
                    if (colon > 0 && colon < 60 && !text.Substring(0, colon).Contains("\n"))
                    {
                        node.Actor = node.Actor ?? text.Substring(0, colon).Trim();
                        node.Text = text.Substring(colon + 1).Trim();
                    }
                    // Reuse shared destinations. A repeated id is only an error
                    // when two different source lines claim the same explicit tag.
                    if (model.Nodes.ContainsKey(node.Id))
                    {
                        if (sourcePathByNodeId[node.Id] != id) throw new InvalidOperationException($"Duplicate Ink id tag '{node.Id}'. Give each dialogue line a unique id.");
                        return node.Id;
                    }
                    sourcePathByNodeId[node.Id] = id;
                    model.Nodes.Add(node.Id, node);
                    node.Next.Add(VisitCurrentFlow());
                    return node.Id;
                }
                return ImportCurrentChoices();
            }
            finally
            {
                activeFlowPaths.Remove(stateKey);
            }
        }

        private void ContinueToNextLine()
        {
            // Continue() could hang on a loop that emits no text. Small execution
            // slices let us check the deadline even when Ink never reaches a line.
            do
            {
                story.ContinueAsync(RuntimeTimeSliceMilliseconds);
                if (timer.Elapsed.TotalSeconds > MaximumImportSeconds)
                    throw new InvalidOperationException("Ink execution did not finish. Check for loops.");
            } while (!story.asyncContinueComplete);
        }

        private string ImportCurrentChoices()
        {
            var choices = story.currentChoices.ToList();
            if (choices.Count == 0) return null;
            string decisionId = "choice:" + choices[0].sourcePath;
            if (model.Nodes.ContainsKey(decisionId)) return decisionId;
            var decision = new InkImportNode { Id = decisionId };
            model.Nodes.Add(decisionId, decision);
            // Each branch starts from this same snapshot. Otherwise exploring
            // one choice would consume story state needed by the next choice.
            string snapshot = story.state.ToJson();
            foreach (var choice in choices)
            {
                if (choice.tags != null && choice.tags.Any(t => t.StartsWith("actor:") || t.StartsWith("shot:") || t.StartsWith("target:")))
                    model.Warnings.Add("Choice camera tags are not applied; put camera tags on the branch's dialogue line.");
                decision.Choices.Add(choice.text.Trim());
                story.state.LoadJson(snapshot);
                story.ChooseChoiceIndex(choice.index);
                decision.Next.Add(VisitCurrentFlow());
            }
            return decisionId;
        }

        static void ApplyTags(InkImportNode node, List<string> tags)
        {
            if (tags == null) return;
            foreach (string tag in tags)
            {
                int split = tag.IndexOf(':');
                if (split < 0) continue;
                
                string value = tag.Substring(split + 1).Trim();
                
                switch (tag.Substring(0, split).Trim().ToLowerInvariant())
                {
                    case "actor": node.Actor = value; break;
                    case "target": node.Target = value; break;
                    case "shot": node.Shot = value; break;
                    case "id": node.Id = "id:" + value; break;
                }
            }
        }
    }
}
