using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ink;
using Ink.Runtime;

namespace RydenCam.Editor.InkIntegration
{
    /// <summary>
    /// A conversation without Unity scene objects. The entire import can be
    /// validated before the caller replaces the graph the user is editing.
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

        // Dialogue has one successor. Decisions have one per choice, in the same
        // order as Choices. A null successor ends that branch of the conversation.
        public readonly List<string> Next = new List<string>();
    }

    /// <summary>
    /// Compiles Ink and reads its static conversation graph. Syntax checks and
    /// runtime traversal are separate so each can be understood independently.
    /// </summary>
    public sealed class InkBranchCamImporter
    {
        public InkImportModel Import(string source, string filename, string entry = "")
        {
            // Each call owns its state, including the time spent compiling. A failed
            // import cannot leave runtime state behind for the next file.
            var timer = Stopwatch.StartNew();
            var model = new InkImportModel();
            Story story = CompileStory(source, filename, model.Warnings, out var parsed);
            model.Entry = SelectEntry(story, parsed, entry);

            var reader = new InkFlowReader(story, model, timer);
            reader.Read();
            return model;
        }

        private static Story CompileStory(string source, string filename,
            List<string> warnings, out Ink.Parsed.Story parsed)
        {
            var errors = new List<string>();
            void RecordDiagnostic(string message, ErrorType type)
            {
                if (type == ErrorType.Error) errors.Add(message);
                else warnings.Add(message);
            }

            var compiler = new Compiler(source, new Compiler.Options
            {
                sourceFilename = filename,
                errorHandler = RecordDiagnostic
            });
            parsed = compiler.Parse();
            if (parsed == null || errors.Count > 0)
                throw new InvalidOperationException(string.Join("\n", errors));

            // Reject dynamic constructs before executing any part of the story.
            InkStaticSyntax.Validate(parsed);
            Story story = parsed.ExportRuntime(RecordDiagnostic);
            if (story == null || errors.Count > 0)
                throw new InvalidOperationException(string.Join("\n", errors));

            story.onError += ThrowRuntimeError;
            return story;
        }

        private static void ThrowRuntimeError(string message, ErrorType type)
        {
            if (type == ErrorType.Error)
                throw new InvalidOperationException(message);
        }

        private static string SelectEntry(Story story, Ink.Parsed.Story parsed, string entry)
        {
            if (!string.IsNullOrWhiteSpace(entry))
            {
                story.ChoosePathString(entry.Trim());
            }
            else if (!story.canContinue || !InkStaticSyntax.HasRootNarrative(parsed))
            {
                // Files containing only knots start at the first knot. Root text
                // or a root divert takes precedence when either is present.
                entry = parsed.content.OfType<Ink.Parsed.Knot>().FirstOrDefault()?.name ?? "";
                if (entry.Length > 0) story.ChoosePathString(entry);
            }

            return entry;
        }
    }
}
