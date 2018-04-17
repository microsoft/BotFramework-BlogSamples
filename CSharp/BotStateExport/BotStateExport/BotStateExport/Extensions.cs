using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Azure
{
    internal static class StringExtensions
    {
        private static readonly Dictionary<string, string> _DefaultReplacementsForCharactersDisallowedByAzure = new Dictionary<string, string>() { { "/", "|s|" }, { @"\", "|b|" }, { "#", "|h|" }, { "?", "|q|" } };

        internal static string SanitizeForAzureKeys(this string input, Dictionary<string, string> replacements = null)
        {
            var repmap = replacements ?? _DefaultReplacementsForCharactersDisallowedByAzure;
            return input.Trim().Replace("/", repmap["/"]).Replace(@"\", repmap[@"\"]).Replace("#", repmap["#"]).Replace("?", repmap["?"]);
        }
    }
}
