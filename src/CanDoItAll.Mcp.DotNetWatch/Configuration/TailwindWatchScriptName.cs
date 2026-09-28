using System.Text.RegularExpressions;

namespace CanDoItAll.Mcp.DotNetWatch.Configuration;

// Script names reach npm as a process argument and, where npm is started through its Windows batch
// launcher, through cmd.exe as well; names are therefore limited to characters with no shell meaning.
internal static partial class TailwindWatchScriptName
{
    public static bool IsSupported(string? scriptName)
        => !string.IsNullOrWhiteSpace(scriptName) && SupportedName().IsMatch(scriptName.Trim());

    [GeneratedRegex(@"^[A-Za-z0-9][A-Za-z0-9:._-]{0,127}$")]
    private static partial Regex SupportedName();
}
