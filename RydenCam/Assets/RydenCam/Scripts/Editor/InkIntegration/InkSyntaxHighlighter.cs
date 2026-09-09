using System.Text;
using System.Text.RegularExpressions;

namespace RydenCam.Editor.InkIntegration
{
    /// <summary>
    /// Presentation-only token coloring. Ink's compiler remains responsible for
    /// parsing and validation; highlighting does not imply importer support.
    /// </summary>
    public static class InkSyntaxHighlighter
    {
        // Earlier alternatives win, keeping tags and keywords inside comments uncolored.
        private static readonly Regex Tokens = new Regex(
            @"(?<comment>//[^\r\n]*|/\*[\s\S]*?\*/)|(?<section>^[ \t]*={2,}[^\r\n]*)|(?<tag>\#[^\r\n]*)|(?<divert>->\s*[\w.]+)|(?<choice>^[ \t]*[*+]+|\[[^\]\r\n]*\])|(?<speaker>^[ \t]*[\w][\w \t]*:)|(?<keyword>\b(?:END|DONE|VAR|CONST|LIST|INCLUDE|TODO|true|false)\b)",
            RegexOptions.Multiline | RegexOptions.CultureInvariant);

        public static string Highlight(string source, bool darkTheme)
        {
            var result = new StringBuilder(source.Length + 128);
            int position = 0;
            foreach (Match token in Tokens.Matches(source))
            {
                AppendLiteral(result, source.Substring(position, token.Index - position));
                string color;
                if (token.Groups["comment"].Success) color = darkTheme ? "#81AA76" : "#426C37";
                else if (token.Groups["tag"].Success) color = darkTheme ? "#E4BD7B" : "#805000";
                else if (token.Groups["speaker"].Success) color = darkTheme ? "#72CFC5" : "#00675E";
                else if (token.Groups["choice"].Success) color = darkTheme ? "#91C9FF" : "#16599B";
                else color = darkTheme ? "#CCA7F0" : "#783599";
                result.Append("<color=").Append(color).Append('>');
                AppendLiteral(result, token.Value);
                result.Append("</color>");
                position = token.Index + token.Length;
            }
            AppendLiteral(result, source.Substring(position));
            return result.ToString();
        }

        private static void AppendLiteral(StringBuilder result, string text)
        {
            // Prevent dialogue such as <b>Hello</b> from becoming Unity rich text.
            // The zero-width separator is only in the rendered copy, never the source.
            result.Append(text.Replace("<", "<\u200B"));
        }
    }
}
