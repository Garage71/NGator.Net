using System.Text.RegularExpressions;

namespace NGator.Net.Infrastructure
{
    /// <summary>
    /// Application level enumeration
    /// </summary>
    public enum ApplicationLevel
    {
        RssParser,
        NewsParser
    }

    /// <summary>
    /// Error description service container
    /// </summary>
    public class ErrorDescription
    {
        public ApplicationLevel Level { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    ///  Methods to remove HTML from strings.
    /// </summary>
    public static class HtmlRemoval
    {
        /// <summary>
        ///     Regular expressions compiled for better performance.
        /// </summary>
        private static readonly Regex HtmlRegex = new Regex("<.*?>", RegexOptions.Compiled);
        private static readonly Regex QuoteRegex = new Regex("&.*?;", RegexOptions.Compiled);
        private static readonly Regex DivRegex = new Regex(@"<div\b[^>]*>(.*?)<\/div>", RegexOptions.Compiled);
        private static readonly Regex ScriptRegex =
            new Regex(@"(?<startTag><\s*script[^>]*>)(?<content>[\s\S]*?)(?<endTag><\s*/script[^>]*>)",
                RegexOptions.Compiled);

        /// <summary>
        ///  Removes HTML tags from string
        /// </summary>
        public static string StripTagsRegex(this string source)
        {
            var stripped = HtmlRegex.Replace(source, string.Empty);
            var unquoted = QuoteRegex.Replace(stripped, " ");

            return unquoted;
        }


        /// <summary>
        /// Removes div and script tags from string. Scripts tags are removed with body
        /// </summary>        
        public static string RemoveDivAndScriptBlocks(this string source)
        {
            var undived = DivRegex.Replace(source, string.Empty);
            var unscripted = ScriptRegex.Replace(undived, string.Empty);

            return unscripted;
        }
    }
}