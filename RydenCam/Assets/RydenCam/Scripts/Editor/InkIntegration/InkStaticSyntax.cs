using System;
using System.Collections.Generic;
using System.Linq;
using Ink.Parsed;

namespace RydenCam.Editor.InkIntegration
{
    /// <summary>Defines the Ink subset that a static BranchCam graph can represent.</summary>
    internal static class InkStaticSyntax
    {
        private static readonly HashSet<string> AllowedConstructs = new HashSet<string>
        {
            "Story", "Knot", "Weave", "ContentList", "Text",
            "Choice", "Divert", "Gather", "Tag"
        };

        public static void Validate(Ink.Parsed.Object element)
        {
            if (!IsSupported(element))
            {
                throw new InvalidOperationException(
                    $"Line {element.debugMetadata?.startLineNumber}: unsupported Ink construct {element.GetType().Name}. " +
                    "Use dialogue, knots, choices, static diverts, END and tags.");
            }

            if (element.content == null) return;
            foreach (var child in element.content) Validate(child);
        }

        private static bool IsSupported(Ink.Parsed.Object element)
        {
            if (!AllowedConstructs.Contains(element.GetType().Name)) return false;

            // A familiar construct can still carry dynamic behavior. For example,
            // a conditional choice cannot be represented by a fixed set of edges.
            if (element is FlowBase flow)
                return !flow.isFunction && !flow.hasParameters;
            if (element is Choice choice)
                return choice.condition == null && !choice.isInvisibleDefault;
            if (element is Divert divert)
                return !divert.isTunnel && !divert.isThread && !divert.isFunctionCall &&
                    !(divert.arguments?.Count > 0);

            return true;
        }

        public static bool HasRootNarrative(Story story)
        {
            return story.content.Where(element => !(element is Knot)).Any(HasNarrative);
        }

        private static bool HasNarrative(Ink.Parsed.Object element)
        {
            if (element is Choice) return true;
            if (element is Text text && !string.IsNullOrWhiteSpace(text.text)) return true;
            if (element is Divert divert && !divert.isEnd && !divert.isDone) return true;

            return element.content != null && element.content.Any(HasNarrative);
        }
    }
}
