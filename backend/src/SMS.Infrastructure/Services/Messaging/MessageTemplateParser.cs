using System.Text.RegularExpressions;

namespace SMS.Infrastructure.Services.Messaging;

/// <summary>
/// Utility to parse message templates and replace placeholders with provided data
/// </summary>
public static class MessageTemplateParser
{
    private static readonly Regex PlaceholderRegex = new Regex(@"\{\{([\w.]+)\}\}", RegexOptions.Compiled);

    /// <summary>
    /// Replaces placeholders in the format {{Key}} with values from the dictionary
    /// </summary>
    /// <param name="template">The template string</param>
    /// <param name="data">Dictionary of placeholder keys and values</param>
    /// <returns>The parsed message string</returns>
    public static string Parse(string template, Dictionary<string, string> data)
    {
        if (string.IsNullOrEmpty(template)) return string.Empty;

        return PlaceholderRegex.Replace(template, match =>
        {
            var key = match.Groups[1].Value;
            return data.TryGetValue(key, out var value) ? value : match.Value;
        });
    }
}
